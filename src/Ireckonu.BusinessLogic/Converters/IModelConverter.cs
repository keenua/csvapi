using Ireckonu.BusinessLogic.Models;
using Ireckonu.Data.Models;

namespace Ireckonu.BusinessLogic.Converters
{
    public interface IModelConverter
    {
        Article ToModel(CsvRecord record);
    }
}
