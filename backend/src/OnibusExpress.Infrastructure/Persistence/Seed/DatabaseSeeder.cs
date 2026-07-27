using Microsoft.EntityFrameworkCore;
using OnibusExpress.Domain.Abstractions;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Infrastructure.Persistence.Seed;

/// <summary>
/// Popula o banco para o ambiente de teste. As rotas e as reservas de exemplo
/// são criadas uma única vez; as VIAGENS são garantidas a cada subida para os
/// próximos <see cref="DiasAFrente"/> dias, em vários horários — assim a busca
/// sempre tem dados atuais, sem duplicar (idempotente por rota + horário).
/// </summary>
public static class DatabaseSeeder
{
    private const int DiasAFrente = 7;
    private const int TotalAssentos = 42;

    // Horários de partida em horário LOCAL (Brasil); convertidos para UTC ao gravar.
    private static readonly int[] HorariosLocais = [6, 9, 12, 15, 18, 21];

    private static readonly decimal[] PrecosBasePorRota =
        [89.90m, 120.00m, 95.50m, 149.90m, 79.00m, 135.00m];

    private static readonly TimeZoneInfo FusoBrasil =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public static async Task SeedAsync(
        AppDbContext context, IDateTimeProvider clock, CancellationToken cancellationToken = default)
    {
        await GarantirRotasAsync(context, cancellationToken);

        var rotas = await context.Rotas.AsNoTracking().ToListAsync(cancellationToken);
        await GarantirViagensDosProximosDiasAsync(context, rotas, clock, cancellationToken);
        await GarantirReservasIniciaisAsync(context, clock, cancellationToken);
    }

    private static async Task GarantirRotasAsync(AppDbContext context, CancellationToken cancellationToken)
    {
        if (await context.Rotas.AnyAsync(cancellationToken))
        {
            return;
        }

        context.Rotas.AddRange(CriarRotas());
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task GarantirViagensDosProximosDiasAsync(
        AppDbContext context, IReadOnlyList<Rota> rotas, IDateTimeProvider clock, CancellationToken cancellationToken)
    {
        // Chaves já existentes (rota + instante), para não duplicar em subidas repetidas.
        var existentes = await context.Viagens
            .AsNoTracking()
            .Select(v => new { v.RotaId, v.DataHoraPartida })
            .ToListAsync(cancellationToken);

        var chaves = existentes
            .Select(x => (x.RotaId, x.DataHoraPartida))
            .ToHashSet();

        var hoje = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.UtcNow, FusoBrasil).DateTime);
        var novas = new List<Viagem>();

        for (var dia = 0; dia < DiasAFrente; dia++)
        {
            var data = hoje.AddDays(dia);

            for (var i = 0; i < rotas.Count; i++)
            {
                var rota = rotas[i];

                for (var h = 0; h < HorariosLocais.Length; h++)
                {
                    var partida = PartidaUtc(data, HorariosLocais[h]);
                    if (chaves.Contains((rota.Id, partida)))
                    {
                        continue;
                    }

                    var preco = PrecosBasePorRota[i % PrecosBasePorRota.Length] + h * 5m;
                    novas.Add(new Viagem(rota.Id, partida, preco, TotalAssentos));
                }
            }
        }

        if (novas.Count > 0)
        {
            context.Viagens.AddRange(novas);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    private static async Task GarantirReservasIniciaisAsync(
        AppDbContext context, IDateTimeProvider clock, CancellationToken cancellationToken)
    {
        if (await context.Reservas.AnyAsync(cancellationToken))
        {
            return;
        }

        // Ocupa alguns assentos da próxima viagem futura, para o mapa não nascer vazio.
        var agora = clock.UtcNow;
        var viagem = await context.Viagens
            .AsNoTracking()
            .Where(v => v.DataHoraPartida > agora)
            .OrderBy(v => v.DataHoraPartida)
            .FirstOrDefaultAsync(cancellationToken);

        if (viagem is null)
        {
            return;
        }

        var passageiros = CriarPassageiros();
        context.Passageiros.AddRange(passageiros);

        context.Reservas.AddRange(
            new Reserva(viagem.Id, passageiros[0].Id, 1, CodigoReserva.Gerar()),
            new Reserva(viagem.Id, passageiros[0].Id, 2, CodigoReserva.Gerar()),
            new Reserva(viagem.Id, passageiros[1].Id, 5, CodigoReserva.Gerar()),
            new Reserva(viagem.Id, passageiros[1].Id, 12, CodigoReserva.Gerar()));

        await context.SaveChangesAsync(cancellationToken);
    }

    private static DateTimeOffset PartidaUtc(DateOnly dia, int horaLocal)
    {
        var local = dia.ToDateTime(new TimeOnly(horaLocal, 0), DateTimeKind.Unspecified);
        var utc = TimeZoneInfo.ConvertTimeToUtc(local, FusoBrasil);
        return new DateTimeOffset(utc, TimeSpan.Zero);
    }

    private static List<Rota> CriarRotas() =>
    [
        new("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6)),
        new("Rio de Janeiro", "São Paulo", TimeSpan.FromHours(6)),
        new("São Paulo", "Curitiba", TimeSpan.FromHours(6)),
        new("São Paulo", "Belo Horizonte", TimeSpan.FromHours(8)),
        new("Curitiba", "Florianópolis", TimeSpan.FromHours(4.5)),
        new("Belo Horizonte", "Rio de Janeiro", TimeSpan.FromHours(7)),
    ];

    private static List<Passageiro> CriarPassageiros() =>
    [
        new("Mariana Oliveira", Cpf.Criar("52998224725"), "mariana.oliveira@exemplo.com", new DateOnly(1992, 3, 14)),
        new("Carlos Andrade", Cpf.Criar("16899535009"), "carlos.andrade@exemplo.com", new DateOnly(1987, 11, 2)),
    ];
}
