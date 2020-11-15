﻿using Ireckonu.BusinessLogic.Models;
using Ireckonu.BusinessLogic.Exceptions;
using Ireckonu.BusinessLogic.Converters;
using Ireckonu.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Ireckonu.BusinessLogic.Services
{
    public class UploadService : IUploadService
    {
        private const int BufferSize = 1000;

        private readonly ICsvProcessor _csvProcessor;
        private readonly IDbContext _db;
        private readonly IModelConverter _converter;
        private readonly ILogger<UploadService> _logger;

        public UploadService(
            ICsvProcessor csvProcessor, 
            IDbContext db, 
            IModelConverter converter, 
            ILogger<UploadService> logger
        )
        {
            _csvProcessor = csvProcessor;
            _db = db;
            _converter = converter;
            _logger = logger;
        }

        private async Task Store(List<RecordProcessingResult> buffer)
        {
            var models = buffer.Select(pr => _converter.ToModel(pr.Record));
            await _db.BulkUpsert(models).ConfigureAwait(false);
        }

        private void MoveBufferToResult(UploadResult result, List<RecordProcessingResult> buffer, UploadConfiguration uploadConfiguration)
        {
            foreach (var r in buffer)
            {
                r.Record = null;
                result.LastProcessedLine = r.Line;

                if (uploadConfiguration.ReportSuccessForRecords || !r.Success)
                {
                    if (result.Records.Count < uploadConfiguration.MaxRecordsInResponse)
                    {
                        result.Records.Add(r);
                    }
                }
            }

            buffer.Clear();
        }

        public async Task<UploadResult> Upload(Stream stream, UploadConfiguration uploadConfiguration)
        {
            var result = new UploadResult();
            var buffer = new List<RecordProcessingResult>();

            try
            {
                var records = _csvProcessor.Process(stream, uploadConfiguration.ContainsHeader);

                await foreach (var record in records)
                {
                    if (record.Success)
                    {
                        buffer.Add(record);
                        if (buffer.Count >= BufferSize)
                        {
                            await Store(buffer).ConfigureAwait(false);
                            MoveBufferToResult(result, buffer, uploadConfiguration);

                            if (result.Records.Count >= uploadConfiguration.MaxRecordsInResponse)
                            {
                                break;
                            }
                        }
                    }
                }

                await Store(buffer).ConfigureAwait(false);
                MoveBufferToResult(result, buffer, uploadConfiguration);
            }
            // For handling file validation exceptions etc., which we want to report to end user
            catch (BusinessException e)
            {
                _logger.LogWarning(e, "Upload failed with a business exception");
                result.Issue = new FileProcessingError { Text = e.Message };
            }
            // For handling other exceptions, i.e. storage failures. We don't want to propagate the exception message to user in this case
            catch (Exception e)
            {
                _logger.LogError(e, "Upload failed with an unexpected exception");
                result.Issue = new FileProcessingError { Text = "Upload failed with an unexpected exception" };
            }

            _logger.LogInformation($"Returning {result.Records.Count} records. Last processed line: {result.LastProcessedLine}");
            return result;
        }
    }
}
