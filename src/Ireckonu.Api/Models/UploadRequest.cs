using System.ComponentModel;

namespace Ireckonu.Api.Models
{
    public class UploadRequest
    {
        [DefaultValue(false)]
        public bool ReportSuccessForRecords { get; set; } = false;

        [DefaultValue(false)]
        public bool ContainsHeader { get; set; } = true;

        [DefaultValue(1000)]
        public int MaxRecordsInResponse { get; set; } = 1000;
    }
}
