using Ireckonu.Api.Models;
using Ireckonu.BusinessLogic.Models;

namespace Ireckonu.Api.Converters
{
    public interface IDtoConverter
    {
        UploadConfiguration ToDomain(UploadRequest request);
    }
}
