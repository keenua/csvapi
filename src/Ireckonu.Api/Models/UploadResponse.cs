using System.Collections.Generic;

namespace Ireckonu.Api.Models
{
    public class UploadResponse
    {
        public bool Success { get; set; }
        public ProcessingIssue Issue { get; set; }
        public int LastProcessedLine { get; set; }
        public List<RecordResult> Records { get; set; } = new List<RecordResult>();
    }
}
