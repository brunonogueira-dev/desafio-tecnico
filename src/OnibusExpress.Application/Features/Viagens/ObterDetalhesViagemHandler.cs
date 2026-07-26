using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Application.Common;

namespace OnibusExpress.Application.Features.Viagens;

/// <summary>Detalhe de uma viagem com o mapa de assentos (GET /viagens/{id}).</summary>
public sealed class ObterDetalhesViagemHandler(IViagemRepository viagens)
{
    public async Task<Result<ViagemDetalheDto>> ExecutarAsync(Guid viagemId, CancellationToken cancellationToken)
    {
        var viagem = await viagens.ObterComRotaAsync(viagemId, cancellationToken);
        if (viagem is null)
        {
            return Error.NaoEncontrado("Viagem não encontrada.");
        }

        var ocupados = (await viagens.ObterAssentosOcupadosAsync(viagemId, cancellationToken)).ToHashSet();

        var assentos = Enumerable.Range(1, viagem.TotalAssentos)
            .Select(numero => new AssentoDto(numero, ocupados.Contains(numero)))
            .ToList();

        var rota = viagem.Rota!;
        var dto = new ViagemDetalheDto(
            viagem.Id,
            rota.Origem,
            rota.Destino,
            viagem.DataHoraPartida,
            (int)rota.DuracaoEstimada.TotalMinutes,
            viagem.PrecoBase,
            viagem.TotalAssentos,
            viagem.TotalAssentos - ocupados.Count,
            assentos);

        return Result.Success(dto);
    }
}
