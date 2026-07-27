using OnibusExpress.Application.Abstractions.Persistence;

namespace OnibusExpress.Infrastructure.Persistence;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken) =>
        context.SaveChangesAsync(cancellationToken);
}
