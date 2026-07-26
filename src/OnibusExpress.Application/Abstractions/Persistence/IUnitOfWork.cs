namespace OnibusExpress.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken);
}
