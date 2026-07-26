using Microsoft.EntityFrameworkCore;
using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.Enums;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Infrastructure.Persistence.Repositories;

public sealed class ReservaRepository(AppDbContext context) : IReservaRepository
{
    public async Task<Reserva?> ObterPorCodigoComViagemAsync(
        CodigoReserva codigo, CancellationToken cancellationToken) =>
        await context.Reservas
            .Include(r => r.Viagem)
                .ThenInclude(v => v!.Rota)
            .Include(r => r.Passageiro)
            .FirstOrDefaultAsync(r => r.Codigo == codigo, cancellationToken);

    public async Task<bool> ExisteConfirmadaParaAssentoAsync(
        Guid viagemId, int numeroAssento, CancellationToken cancellationToken) =>
        await context.Reservas.AnyAsync(
            r => r.ViagemId == viagemId
              && r.NumeroAssento == numeroAssento
              && r.Status == StatusReserva.Confirmada,
            cancellationToken);

    public async Task<bool> CodigoEmUsoAsync(CodigoReserva codigo, CancellationToken cancellationToken) =>
        await context.Reservas.AnyAsync(r => r.Codigo == codigo, cancellationToken);

    public void Adicionar(Reserva reserva) => context.Reservas.Add(reserva);
}
