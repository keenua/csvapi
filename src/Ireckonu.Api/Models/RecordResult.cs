using System.Collections.Generic;

namespace Ireckonu.Api.Models
{
    public class RecordResult
    {
        public bool Success { get; set; }
        public int Line { get; set; }
        public List<ProcessingIssue> Issues { get; set; } = new List<ProcessingIssue>();
    }
}
