using Microsoft.EntityFrameworkCore;
using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.Enums;

namespace OnibusExpress.Infrastructure.Persistence.Repositories;

public sealed class ViagemRepository(AppDbContext context) : IViagemRepository
{
    public async Task<IReadOnlyList<ViagemComOcupacao>> BuscarAsync(
        string origem, string destino, DateOnly dataPartida, CancellationToken cancellationToken)
    {
        var inicio = new DateTimeOffset(dataPartida.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var fim = inicio.AddDays(1);

        // Projeção em um único round-trip: a contagem de ocupados é uma subquery
        // correlacionada, sem N+1 e sem carregar as reservas na memória.
        return await context.Viagens
            .AsNoTracking()
            .Where(v => v.Rota!.Origem == origem
                     && v.Rota.Destino == destino
                     && v.DataHoraPartida >= inicio
                     && v.DataHoraPartida < fim)
            .OrderBy(v => v.DataHoraPartida)
            .Select(v => new ViagemComOcupacao(
                v.Id,
                v.Rota!.Origem,
                v.Rota.Destino,
                v.DataHoraPartida,
                v.Rota.DuracaoEstimada,
                v.PrecoBase,
                v.TotalAssentos,
                context.Reservas.Count(r => r.ViagemId == v.Id && r.Status == StatusReserva.Confirmada)))
            .ToListAsync(cancellationToken);
    }

    public async Task<Viagem?> ObterComRotaAsync(Guid id, CancellationToken cancellationToken) =>
        await context.Viagens
            .Include(v => v.Rota)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public async Task<IReadOnlyList<int>> ObterAssentosOcupadosAsync(
        Guid viagemId, CancellationToken cancellationToken) =>
        await context.Reservas
            .AsNoTracking()
            .Where(r => r.ViagemId == viagemId && r.Status == StatusReserva.Confirmada)
            .Select(r => r.NumeroAssento)
            .OrderBy(n => n)
            .ToListAsync(cancellationToken);
}
