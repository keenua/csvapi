using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Ireckonu.BusinessLogic.Models;
using Ireckonu.BusinessLogic.Models.Issues;

namespace Ireckonu.BusinessLogic
{
    public class CsvProcessor : ICsvProcessor
    {
        private readonly ILogger<CsvProcessor> _logger;

        public CsvProcessor(ILogger<CsvProcessor> logger)
        {
            _logger = logger;
        }

        public async IAsyncEnumerable<RecordProcessingResult> Process(Stream stream, bool containsHeader)
        {
            var culture = new CultureInfo("en-US");
            var configuration = new CsvConfiguration(culture)
            {
                HasHeaderRecord = containsHeader
            };

            using var reader = new StreamReader(stream);

            using var csv = new CsvReader(reader, configuration);

            int line = containsHeader ? 1 : 0;

            while (await csv.ReadAsync().ConfigureAwait(false))
            {
                line++;

                var result = new RecordProcessingResult { Line = line };

                try
                {
                    var record = csv.GetRecord<CsvRecord>();
                    if (record == null) continue;
                    result.Record = record;
                }
                catch (MissingFieldException ex)
                {
                    var index = ex.ReadingContext.CurrentIndex;
                    var header = ex.ReadingContext.HeaderRecord;

                    var msg = header == null ? $"Field at column #{index} is missing" : $"{header[index]} is missing";
                    var error = new Error(msg);
                    result.Issues.Add(error);
                }
                catch (TypeConverterException ex)
                {
                    var columnName = ex.MemberMapData.Member.Name;
                    var msg = $"{columnName} has invalid format";
                    var error = new Error(msg);
                    result.Issues.Add(error);
                }

                if (line % 10000 == 0)
                {
                    _logger.LogInformation($"{line} records processed");
                }

                yield return result;
            }
        }
    }
}
