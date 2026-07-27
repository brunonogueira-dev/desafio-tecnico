using OnibusExpress.Domain.Entities;

namespace OnibusExpress.Application.Abstractions.Persistence;

public interface IViagemRepository
{
    /// <summary>
    /// Busca paginada de viagens em um dia. Origem e destino são opcionais: quando
    /// nulos/vazios, retorna todas as viagens do dia (todas as rotas).
    /// </summary>
    Task<PaginaDeViagens> BuscarAsync(
        string? origem,
        string? destino,
        DateOnly dataPartida,
        int pagina,
        int tamanho,
        CancellationToken cancellationToken);

    /// <summary>Carrega a viagem com a rota associada.</summary>
    Task<Viagem?> ObterComRotaAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Números dos assentos com reserva confirmada na viagem.</summary>
    Task<IReadOnlyList<int>> ObterAssentosOcupadosAsync(Guid viagemId, CancellationToken cancellationToken);
}
