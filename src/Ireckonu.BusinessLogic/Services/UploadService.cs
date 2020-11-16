using Ireckonu.BusinessLogic.Exceptions;
using Ireckonu.BusinessLogic.Converters;
using Ireckonu.BusinessLogic.Models;
using Ireckonu.BusinessLogic.Models.Issues;
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
        private readonly ICsvProcessor _csvProcessor;
        private readonly IValidationService _validationService;
        private readonly IDbContext _db;
        private readonly IModelConverter _converter;
        private readonly UploadServiceSettings _settings;
        private readonly ILogger<UploadService> _logger;

        public UploadService(
            ICsvProcessor csvProcessor, 
            IValidationService validationService,
            IDbContext db, 
            IModelConverter converter, 
            UploadServiceSettings settings,
            ILogger<UploadService> logger
        )
        {
            _csvProcessor = csvProcessor ?? throw new ArgumentException(nameof(csvProcessor));
            _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            _db = db ?? throw new ArgumentException(nameof(db));
            _converter = converter ?? throw new ArgumentException(nameof(converter));
            _settings = settings ?? throw new ArgumentException(nameof(settings));
            _logger = logger;
        }

        private async Task Store(List<RecordProcessingResult> buffer)
        {
            var models = buffer.Where(pr => pr.Success).Select(pr => _converter.ToModel(pr.Record));
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

        private async Task ValidateRecord(RecordProcessingResult record)
        {
            if (record.Success)
            {
                var issues = await _validationService.Validate(record.Record).ConfigureAwait(false);
                record.Issues.AddRange(issues);
            }
        }

        public async Task<UploadResult> Upload(Stream stream, UploadConfiguration uploadConfiguration)
        {
            var result = new UploadResult();
            var buffer = new List<RecordProcessingResult>();

            DateTime start = DateTime.Now;

            try
            {
                var records = _csvProcessor.Process(stream, uploadConfiguration.ContainsHeader).ConfigureAwait(false);

                await foreach (var record in records)
                {
                    //await ValidateRecord(record).ConfigureAwait(false);

                    buffer.Add(record);
                    if (buffer.Count >= _settings.BufferSize)
                    {
                        await Store(buffer).ConfigureAwait(false);
                        MoveBufferToResult(result, buffer, uploadConfiguration);

                        if (result.Records.Count >= uploadConfiguration.MaxRecordsInResponse)
                        {
                            break;
                        }
                    }
                }

                await Store(buffer).ConfigureAwait(false);
                MoveBufferToResult(result, buffer, uploadConfiguration);
            }
            // For handling file validation exceptions etc., which we want to report to end user.
            // There are currently no business expections thrown, so this is just for show
            catch (BusinessException e)
            {
                _logger.LogWarning(e, "Upload failed with a business exception");
                result.Issue = new Error(e.Message);
            }
            // For handling other exceptions, i.e. storage failures. We don't want to propagate the exception message to user in this case
            catch (Exception e)
            {
                _logger.LogError(e, "Upload failed with an unexpected exception");
                result.Issue = new Error("Upload failed with an unexpected exception");
            }


            _logger.LogInformation($"Returning {result.Records.Count} records. Last processed line: {result.LastProcessedLine}. Time ellapsed: {(DateTime.Now - start).TotalSeconds} seconds");
            return result;
        }
    }
}
