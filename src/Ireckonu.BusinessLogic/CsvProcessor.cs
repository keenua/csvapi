using CsvHelper;
using CsvHelper.Configuration;
using Ireckonu.BusinessLogic.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

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
                    result.Record = record;
                }
                catch (MissingFieldException ex)
                {
                    var msg = ex.ReadingContext.HeaderRecord[ex.ReadingContext.CurrentIndex] + " is missing";
                    var error = new Error(msg);
                    result.Issues.Add(error);
                }

                if (line % 10000 == 0)
                {
                    _logger.LogDebug($"{line} records processed");
                }

                yield return result;
            }
        }
    }
}
