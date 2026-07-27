using Microsoft.AspNetCore.Mvc;
using OnibusExpress.Api.Common;
using OnibusExpress.Application.Features.Viagens;
using OnibusExpress.Domain.Abstractions;

namespace OnibusExpress.Api.Controllers;

[Route("viagens")]
[Produces("application/json")]
public sealed class ViagensController(
    BuscarViagensHandler buscarHandler,
    ObterDetalhesViagemHandler detalhesHandler,
    IDateTimeProvider clock) : ApiControllerBase
{
    /// <summary>Busca viagens por origem, destino e data de partida.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ViagemResumoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? origem,
        [FromQuery] string? destino,
        [FromQuery] DateOnly? data,
        CancellationToken cancellationToken)
    {
        var erros = ValidarFiltros(origem, destino, data);
        if (erros.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(erros)
            {
                Title = "Requisição inválida",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var request = new BuscarViagensRequest(origem!.Trim(), destino!.Trim(), data!.Value);
        var resultado = await buscarHandler.ExecutarAsync(request, cancellationToken);
        return resultado.IsSuccess ? Ok(resultado.Value) : ProblemFromError(resultado.Error!);
    }

    /// <summary>Detalhe de uma viagem, incluindo o mapa de assentos.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ViagemDetalheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterDetalhes(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await detalhesHandler.ExecutarAsync(id, cancellationToken);
        return resultado.IsSuccess ? Ok(resultado.Value) : ProblemFromError(resultado.Error!);
    }

    private Dictionary<string, string[]> ValidarFiltros(string? origem, string? destino, DateOnly? data)
    {
        var erros = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(origem))
        {
            erros["origem"] = ["A origem é obrigatória."];
        }

        if (string.IsNullOrWhiteSpace(destino))
        {
            erros["destino"] = ["O destino é obrigatório."];
        }

        if (data is null)
        {
            erros["data"] = ["A data é obrigatória."];
        }
        else if (data.Value < DateOnly.FromDateTime(clock.UtcNow.UtcDateTime))
        {
            erros["data"] = ["A data não pode estar no passado."];
        }

        return erros;
    }
}
