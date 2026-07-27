using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Application.Abstractions.Persistence;

public interface IPassageiroRepository
{
    Task<Passageiro?> ObterPorCpfAsync(Cpf cpf, CancellationToken cancellationToken);

    void Adicionar(Passageiro passageiro);
}
