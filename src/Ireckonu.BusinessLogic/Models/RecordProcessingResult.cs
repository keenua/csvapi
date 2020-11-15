using System.Collections.Generic;
using System.Linq;

namespace Ireckonu.BusinessLogic.Models
{
    public class RecordProcessingResult
    {
        public int Line { get; set; }
        public bool Success => !Issues.Any(e => e.Severity == Severity.Error);
        public List<Issue> Issues { get; } = new List<Issue>();
    }
}
