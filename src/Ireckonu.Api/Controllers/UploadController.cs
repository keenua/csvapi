using System;
using System.Threading.Tasks;
using Ireckonu.Api.Converters;
using Ireckonu.Api.Helpers;
using Ireckonu.Api.Models;
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
        private readonly IDtoConverter _converter;

        public UploadController(IUploadService service, IDtoConverter converter, ILogger<UploadController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
            _logger = logger;
        }

        [HttpPost]
        [DisableFormValueModelBinding]
        [RequestSizeLimit(MaxFileSize)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
        [ImplicitPayload]
        public async Task<IActionResult> Csv([FromQuery] UploadRequest request)
        {
            var stream = Request.BodyReader.AsStream();
            var configuration = _converter.ToDomain(request);

            var result = await _service.Upload(stream, configuration).ConfigureAwait(false);

            return Ok(result);
        }
    }
}
