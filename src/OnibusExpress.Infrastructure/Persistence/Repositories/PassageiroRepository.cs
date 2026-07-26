using Microsoft.EntityFrameworkCore;
using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Infrastructure.Persistence.Repositories;

public sealed class PassageiroRepository(AppDbContext context) : IPassageiroRepository
{
    public async Task<Passageiro?> ObterPorCpfAsync(Cpf cpf, CancellationToken cancellationToken) =>
        await context.Passageiros.FirstOrDefaultAsync(p => p.Cpf == cpf, cancellationToken);

    public void Adicionar(Passageiro passageiro) => context.Passageiros.Add(passageiro);
}
