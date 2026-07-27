using OnibusExpress.Domain.Entities;

namespace OnibusExpress.Application.Abstractions.Persistence;

public interface IViagemRepository
{
    /// <summary>Busca viagens por origem, destino e dia de partida, já com a ocupação.</summary>
    Task<IReadOnlyList<ViagemComOcupacao>> BuscarAsync(
        string origem,
        string destino,
        DateOnly dataPartida,
        CancellationToken cancellationToken);

    /// <summary>Carrega a viagem com a rota associada.</summary>
    Task<Viagem?> ObterComRotaAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Números dos assentos com reserva confirmada na viagem.</summary>
    Task<IReadOnlyList<int>> ObterAssentosOcupadosAsync(Guid viagemId, CancellationToken cancellationToken);
}
