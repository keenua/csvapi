using Ireckonu.BusinessLogic.Models;
using Ireckonu.Data;

namespace Ireckonu.BusinessLogic.Converters
{
    public interface IModelConverter
    {
        Article ToModel(CsvRecord record);
    }
}
