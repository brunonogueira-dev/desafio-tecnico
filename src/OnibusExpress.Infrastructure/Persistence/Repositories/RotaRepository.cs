using Microsoft.EntityFrameworkCore;
using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Domain.Entities;

namespace OnibusExpress.Infrastructure.Persistence.Repositories;

public sealed class RotaRepository(AppDbContext context) : IRotaRepository
{
    public async Task<IReadOnlyList<Rota>> ListarAsync(CancellationToken cancellationToken) =>
        await context.Rotas
            .AsNoTracking()
            .OrderBy(r => r.Origem)
            .ThenBy(r => r.Destino)
            .ToListAsync(cancellationToken);
}
