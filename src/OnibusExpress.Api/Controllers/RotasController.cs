using Microsoft.AspNetCore.Mvc;
using OnibusExpress.Api.Common;
using OnibusExpress.Application.Features.Rotas;

namespace OnibusExpress.Api.Controllers;

[Route("rotas")]
[Produces("application/json")]
public sealed class RotasController(ListarRotasHandler handler) : ApiControllerBase
{
    /// <summary>Lista todas as rotas disponíveis.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RotaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(CancellationToken cancellationToken)
    {
        var resultado = await handler.ExecutarAsync(cancellationToken);
        return resultado.IsSuccess ? Ok(resultado.Value) : ProblemFromError(resultado.Error!);
    }
}
