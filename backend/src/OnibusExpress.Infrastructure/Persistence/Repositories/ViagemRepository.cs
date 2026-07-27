using Microsoft.EntityFrameworkCore;
using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.Enums;

namespace OnibusExpress.Infrastructure.Persistence.Repositories;

public sealed class ViagemRepository(AppDbContext context) : IViagemRepository
{
    // A data da busca é um dia do calendário no fuso do usuário (Brasil), não em UTC.
    // Assim uma viagem às 22h de SP não "vaza" para o dia seguinte na busca.
    private static readonly TimeZoneInfo FusoBrasil =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public async Task<PaginaDeViagens> BuscarAsync(
        string? origem, string? destino, DateOnly dataPartida, int pagina, int tamanho,
        CancellationToken cancellationToken)
    {
        var inicio = InicioDoDiaLocalEmUtc(dataPartida);
        var fim = InicioDoDiaLocalEmUtc(dataPartida.AddDays(1));

        var query = context.Viagens
            .AsNoTracking()
            .Where(v => v.DataHoraPartida >= inicio && v.DataHoraPartida < fim);

        // Origem/destino são filtros opcionais (permite listar todas as viagens do dia).
        if (!string.IsNullOrWhiteSpace(origem))
        {
            query = query.Where(v => v.Rota!.Origem == origem);
        }

        if (!string.IsNullOrWhiteSpace(destino))
        {
            query = query.Where(v => v.Rota!.Destino == destino);
        }

        var total = await query.CountAsync(cancellationToken);

        // Projeção em um único round-trip: a contagem de ocupados é uma subquery
        // correlacionada, sem N+1 e sem carregar as reservas na memória.
        var itens = await query
            .OrderBy(v => v.DataHoraPartida)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
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

        return new PaginaDeViagens(itens, total);
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

    private static DateTimeOffset InicioDoDiaLocalEmUtc(DateOnly dia)
    {
        var localMeiaNoite = dia.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(localMeiaNoite, FusoBrasil), TimeSpan.Zero);
    }
}
