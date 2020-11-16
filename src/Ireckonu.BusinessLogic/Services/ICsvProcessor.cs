using Ireckonu.BusinessLogic.Models;
using System.Collections.Generic;
using System.IO;

namespace Ireckonu.BusinessLogic
{
    public interface ICsvProcessor
    {
        IAsyncEnumerable<RecordProcessingResult> Process(Stream stream, bool containsHeader);
    }
}
