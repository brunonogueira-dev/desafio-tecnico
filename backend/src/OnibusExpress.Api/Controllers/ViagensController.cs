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
    private const int TamanhoPadrao = 10;
    private const int TamanhoMaximo = 50;

    private static readonly TimeZoneInfo FusoBrasil =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    /// <summary>
    /// Lista viagens de um dia, paginadas (10 por página por padrão). Origem e
    /// destino são filtros OPCIONAIS: sem eles, retorna todas as viagens do dia.
    /// Sem <c>data</c>, usa o dia de hoje.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ViagensPaginadasDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Buscar(
        [FromQuery] string? origem,
        [FromQuery] string? destino,
        [FromQuery] DateOnly? data,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanho = TamanhoPadrao,
        CancellationToken cancellationToken = default)
    {
        var hoje = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.UtcNow, FusoBrasil).DateTime);

        var erros = ValidarFiltros(data, hoje, pagina);
        if (erros.Count > 0)
        {
            return ValidationProblem(new ValidationProblemDetails(erros)
            {
                Title = "Requisição inválida",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var request = new BuscarViagensRequest(
            origem?.Trim(), destino?.Trim(), data ?? hoje, pagina, Math.Clamp(tamanho, 1, TamanhoMaximo));
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

    private static Dictionary<string, string[]> ValidarFiltros(DateOnly? data, DateOnly hoje, int pagina)
    {
        var erros = new Dictionary<string, string[]>();

        if (data is not null && data.Value < hoje)
        {
            erros["data"] = ["A data não pode estar no passado."];
        }

        if (pagina < 1)
        {
            erros["pagina"] = ["A página deve ser maior ou igual a 1."];
        }

        return erros;
    }
}
