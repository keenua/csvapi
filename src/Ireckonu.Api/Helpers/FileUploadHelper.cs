using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Ireckonu.Api.Helpers
{
    public class FileUploadHelper : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var attribute = context.MethodInfo.GetCustomAttributes(typeof(ImplicitPayloadAttribute), false).FirstOrDefault();
            if (attribute == null)
            {
                return;
            }

            operation.RequestBody = new OpenApiRequestBody() { Required = true };
            operation.RequestBody.Content.Add("application/octet-stream", new OpenApiMediaType()
            {
                Schema = new OpenApiSchema()
                {
                    Type = "string",
                    Format = "binary",
                },
            });
        }
    }
}
