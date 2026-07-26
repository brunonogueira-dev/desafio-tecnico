using Microsoft.AspNetCore.Mvc;
using OnibusExpress.Api.Common;
using OnibusExpress.Application.Features.Reservas;

namespace OnibusExpress.Api.Controllers;

[Route("reservas")]
[Produces("application/json")]
public sealed class ReservasController(
    CriarReservaHandler criarHandler,
    ConsultarReservaHandler consultarHandler,
    CancelarReservaHandler cancelarHandler) : ApiControllerBase
{
    /// <summary>Cria uma reserva de assento.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarReservaRequest request, CancellationToken cancellationToken)
    {
        if (request?.Passageiro is null)
        {
            return ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["passageiro"] = ["Os dados do passageiro são obrigatórios."] })
            {
                Title = "Requisição inválida",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var resultado = await criarHandler.ExecutarAsync(request, cancellationToken);
        if (resultado.IsFailure)
        {
            return ProblemFromError(resultado.Error!);
        }

        var reserva = resultado.Value;
        return CreatedAtAction(nameof(Consultar), new { codigo = reserva.Codigo }, reserva);
    }

    /// <summary>Consulta uma reserva pelo código.</summary>
    [HttpGet("{codigo}")]
    [ProducesResponseType(typeof(ReservaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Consultar(string codigo, CancellationToken cancellationToken)
    {
        var resultado = await consultarHandler.ExecutarAsync(codigo, cancellationToken);
        return resultado.IsSuccess ? Ok(resultado.Value) : ProblemFromError(resultado.Error!);
    }

    /// <summary>Cancela uma reserva pelo código.</summary>
    [HttpDelete("{codigo}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancelar(string codigo, CancellationToken cancellationToken)
    {
        var resultado = await cancelarHandler.ExecutarAsync(codigo, cancellationToken);
        return resultado.IsSuccess ? NoContent() : ProblemFromError(resultado.Error!);
    }
}
