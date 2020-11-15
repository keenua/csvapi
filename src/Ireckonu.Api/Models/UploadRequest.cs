using System.ComponentModel;

namespace Ireckonu.Api.Models
{
    public class UploadRequest
    {
        [DefaultValue(true)]
        public bool ReportSuccessForRecords { get; set; } = true;
        [DefaultValue(false)]
        public bool ContainsHeader { get; set; } = true;
    }
}
