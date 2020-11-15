using Ireckonu.Api.Models;
using Ireckonu.BusinessLogic.Models;

namespace Ireckonu.Api.Converters
{
    class DtoConverter : IDtoConverter
    {
        public UploadConfiguration ToDomain(UploadRequest request)
        {
            var configuration = new UploadConfiguration
            {
                ReportSuccessForRecords = request.ReportSuccessForRecords,
                ContainsHeader = request.ContainsHeader
            };
            return configuration;
        }
    }
}
