using Ireckonu.Api.Models;
using Ireckonu.BusinessLogic.Models;
using System.Linq;

namespace Ireckonu.Api.Converters
{
    class DtoConverter : IDtoConverter
    {
        public UploadConfiguration ToDomain(UploadRequest request)
        {
            var configuration = new UploadConfiguration
            {
                ReportSuccessForRecords = request.ReportSuccessForRecords,
                ContainsHeader = request.ContainsHeader,
                MaxRecordsInResponse = request.MaxRecordsInResponse
            };
            return configuration;
        }

        private ProcessingIssue ToDto(Issue issue)
        {
            if (issue == null)
            {
                return null;
            }

            return new ProcessingIssue
            {
                Severity = issue.Severity + "",
                Text = issue.Text
            };
        }

        private RecordResult ToDto(RecordProcessingResult model)
        {
            var result = new RecordResult();

            result.Issues = model.Issues.Select(ToDto).ToList();
            result.Line = model.Line;
            result.Success = model.Success;

            return result;
        }

        public UploadResponse ToDto(UploadResult model)
        {
            var response = new UploadResponse();

            response.Issue = ToDto(model.Issue);
            response.LastProcessedLine = model.LastProcessedLine;
            response.Success = model.Success;
            response.Records = model.Records.Select(ToDto).ToList();
            
            return response;
        }
    }
}
