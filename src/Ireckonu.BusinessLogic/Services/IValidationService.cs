using Ireckonu.BusinessLogic.Models;
using Ireckonu.BusinessLogic.Models.Issues;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ireckonu.BusinessLogic.Services
{
    public interface IValidationService
    {
        Task<IEnumerable<Issue>> Validate(CsvRecord record);
    }
}
