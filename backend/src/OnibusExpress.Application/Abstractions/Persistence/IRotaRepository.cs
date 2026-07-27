using OnibusExpress.Domain.Entities;

namespace OnibusExpress.Application.Abstractions.Persistence;

public interface IRotaRepository
{
    Task<IReadOnlyList<Rota>> ListarAsync(CancellationToken cancellationToken);
}
