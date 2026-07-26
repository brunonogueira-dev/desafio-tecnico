using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Application.Common;

namespace OnibusExpress.Application.Features.Viagens;

/// <summary>Busca viagens por origem, destino e data (GET /viagens).</summary>
public sealed class BuscarViagensHandler(IViagemRepository viagens)
{
    public async Task<Result<IReadOnlyList<ViagemResumoDto>>> ExecutarAsync(
        BuscarViagensRequest request, CancellationToken cancellationToken)
    {
        var encontradas = await viagens.BuscarAsync(
            request.Origem, request.Destino, request.Data, cancellationToken);

        var dtos = encontradas
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

        return Result.Success<IReadOnlyList<ViagemResumoDto>>(dtos);
    }
}
