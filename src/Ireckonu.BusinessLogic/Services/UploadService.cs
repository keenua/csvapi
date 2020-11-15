using Ireckonu.BusinessLogic.Models;
using System.IO;
using System.Threading.Tasks;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using System.Text;

namespace Ireckonu.BusinessLogic.Services
{
    public class UploadService : IUploadService
    {
        public async Task<UploadResult> Upload(Stream stream)
        {
            var result = new UploadResult();

            var culture = new CultureInfo("en-US");
            var configuration = new CsvConfiguration(culture);

            int line = 1;
            configuration.ReadingExceptionOccurred = ex =>
            {
                if (ex is MissingFieldException)
                {
                    var error = new ValueError
                    {
                        Text = ex.ReadingContext.HeaderRecord[ex.ReadingContext.CurrentIndex] + " is missing"
                    };

                    var issue = new RecordProcessingIssue { Line = line };
                    issue.Issues.Add(error);
                    result.RecordIssues.Add(issue);
                }
                
                return false;
            };

            using var reader = new StreamReader(stream, Encoding.UTF8);

            using var csv = new CsvReader(reader, configuration);
            
            await csv.ReadAsync();
            if (!csv.ReadHeader())
            {
                result.Success = false;
                return result;
            }

            while (await csv.ReadAsync().ConfigureAwait(false))
            {
                line++;
                csv.GetRecord<CsvRecord>();
            }

            return result;
        }
    }
}
