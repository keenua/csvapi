using Ireckonu.BusinessLogic.Models;
using System.IO;
using System.Threading.Tasks;

namespace Ireckonu.BusinessLogic.Services
{
    public interface IUploadService
    {
        Task<UploadResult> Upload(Stream stream);
    }
}
