using Ireckonu.BusinessLogic.Models;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace Ireckonu.BusinessLogic.Services
{
    public class UploadService : IUploadService
    {
        public async Task<UploadResult> Upload(Stream stream, UploadConfiguration uploadConfiguration)
        {
            var result = new UploadResult();

            var culture = new CultureInfo("en-US");
            var configuration = new CsvConfiguration(culture);

            int line = 0;
            configuration.ReadingExceptionOccurred = ex =>
            {
                if (ex is MissingFieldException)
                {
                    var error = new ValueError
                    {
                        Text = ex.ReadingContext.HeaderRecord[ex.ReadingContext.CurrentIndex] + " is missing"
                    };

                    var recordResult = new RecordProcessingResult { Line = line };
                    recordResult.Issues.Add(error);
                    result.Records.Add(recordResult);
                }

                return false;
            };

            using var reader = new StreamReader(stream);

            using var csv = new CsvReader(reader, configuration);

            if (uploadConfiguration.ContainsHeader)
            {
                line++;
                await csv.ReadAsync().ConfigureAwait(false);
                if (!csv.ReadHeader())
                {
                    result.Success = false;
                    return result;
                }
            }

            while (await csv.ReadAsync().ConfigureAwait(false))
            {
                line++;
                csv.GetRecord<CsvRecord>();
                if (uploadConfiguration.ReportSuccessForRecords)
                {
                    result.Records.Add(new RecordProcessingResult { Line = line });
                }

            }

            result.Success = true;
            return result;
        }
    }
}
