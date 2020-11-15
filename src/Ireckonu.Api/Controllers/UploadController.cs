using System.Threading.Tasks;
using Ireckonu.Api.Helpers;
using Ireckonu.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ireckonu.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadController : ControllerBase
    {
        // 5 GB
        private const long MaxFileSize = 5L * 1024L * 1024L * 1024L;

        private readonly ILogger<UploadController> _logger;
        private readonly IUploadService _service;

        public UploadController(IUploadService service, ILogger<UploadController> logger)
        {
            _service = service ?? throw new System.ArgumentNullException(nameof(service));
            _logger = logger;
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(MaxFileSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
        public async Task<IActionResult> Csv()
        {
            var stream = Request.BodyReader.AsStream();

            var result = await _service.Upload(stream).ConfigureAwait(false);

            return Ok(result);
        }
    }
}
