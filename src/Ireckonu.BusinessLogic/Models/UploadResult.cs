using System.Collections.Generic;

namespace Ireckonu.BusinessLogic.Models
{
    public class UploadResult
    {
        public bool Success { get; set; }
        public List<RecordProcessingIssue> RecordIssues { get; } = new List<RecordProcessingIssue>();
    }
}
