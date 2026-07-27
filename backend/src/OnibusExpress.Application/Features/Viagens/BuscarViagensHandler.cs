using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Application.Common;

namespace OnibusExpress.Application.Features.Viagens;

/// <summary>Busca paginada de viagens de um dia, com origem/destino opcionais (GET /viagens).</summary>
public sealed class BuscarViagensHandler(IViagemRepository viagens)
{
    public async Task<Result<ViagensPaginadasDto>> ExecutarAsync(
        BuscarViagensRequest request, CancellationToken cancellationToken)
    {
        var pagina = await viagens.BuscarAsync(
            request.Origem, request.Destino, request.Data, request.Pagina, request.Tamanho, cancellationToken);

        var itens = pagina.Itens
            .Select(v => new ViagemResumoDto(
                v.Id,
                v.Origem,
                v.Destino,
                v.DataHoraPartida,
                (int)v.DuracaoEstimada.TotalMinutes,
                v.PrecoBase,
                v.TotalAssentos,
                v.TotalAssentos - v.AssentosOcupados))
            .ToList();

        var totalPaginas = pagina.Total == 0
            ? 0
            : (int)Math.Ceiling(pagina.Total / (double)request.Tamanho);

        var dto = new ViagensPaginadasDto(itens, request.Pagina, request.Tamanho, pagina.Total, totalPaginas);
        return Result.Success(dto);
    }
}
