using Ireckonu.BusinessLogic.Models.Issues;
using System.Collections.Generic;

namespace Ireckonu.BusinessLogic.Models
{
    public class UploadResult
    {
        public bool Success => Issue == null; 
        public Issue Issue { get; set; }
        public List<RecordProcessingResult> Records { get; } = new List<RecordProcessingResult>();
        public int LastProcessedLine { get; set; }
    }
}
