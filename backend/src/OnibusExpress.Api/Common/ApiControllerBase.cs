using Microsoft.AspNetCore.Mvc;
using OnibusExpress.Application.Common;

namespace OnibusExpress.Api.Common;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Converte um Error de negócio na resposta ProblemDetails (RFC 7807) adequada.</summary>
    protected IActionResult ProblemFromError(Error error)
    {
        var status = ErrorHttpMapper.ToStatusCode(error.Code);
        var title = ErrorHttpMapper.ToTitle(error.Code);

        if (error.Code == ErrorCode.Validacao && error.Errors is not null)
        {
            var modelState = new ValidationProblemDetails(
                error.Errors.ToDictionary(kv => kv.Key, kv => kv.Value))
            {
                Status = status,
                Title = title,
                Detail = error.Message
            };
            return new ObjectResult(modelState) { StatusCode = status };
        }

        return Problem(detail: error.Message, statusCode: status, title: title);
    }
}
