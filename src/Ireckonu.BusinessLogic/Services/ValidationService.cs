using Ireckonu.BusinessLogic.Models;
using Ireckonu.BusinessLogic.Models.Issues;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ireckonu.BusinessLogic.Services
{
    public class ValidationService : IValidationService
    {
        // We could inject IDbContext here to do validation against DB if necessary.
        // It could go something like that (just a mock, needs actual implementation, caching etc.):
        // 
        //  public ValidationService(IDbContext db)
        //  {
        //      _db = db;
        //  }
        //
        //  public async Task<IEnumerable<Issue>> Validate(CsvRecord record) 
        //  {
        //     ...
        //     var colors = await _db.GetAllColors();
        //     if (!colors.Contains(record.Color)) 
        //     {
        //         var warning = new Warning($"New color {record.Color} spotted");
        //         issues.Add(warning);
        //     }
        //     ...
        //  }
        // 

        // Intentionally made async with more complicated validation in mind
        public async Task<IEnumerable<Issue>> Validate(CsvRecord record)
        {
            var issues = new List<Issue>();

            if (string.IsNullOrEmpty(record.Key))
            {
                issues.Add(new Error("Key cannot be null or empty"));
            }

            if (record.Size < 0)
            {
                issues.Add(new Warning("Size cannot be less than 0"));
            }

            // TODO add warnings etc.

            return issues;
        }
    }
}
