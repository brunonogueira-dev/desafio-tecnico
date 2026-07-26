using Microsoft.EntityFrameworkCore;
using OnibusExpress.Domain.Abstractions;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Infrastructure.Persistence.Seed;

/// <summary>
/// Popula o banco com rotas, viagens futuras (datas relativas ao "agora") e
/// algumas reservas pré-existentes. Idempotente: não faz nada se já houver dados.
/// </summary>
public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        AppDbContext context, IDateTimeProvider clock, CancellationToken cancellationToken = default)
    {
        if (await context.Rotas.AnyAsync(cancellationToken))
        {
            return;
        }

        var rotas = CriarRotas();
        context.Rotas.AddRange(rotas);

        var viagens = CriarViagens(rotas, clock).ToList();
        context.Viagens.AddRange(viagens);

        var passageiros = CriarPassageiros();
        context.Passageiros.AddRange(passageiros);

        context.Reservas.AddRange(CriarReservasIniciais(viagens[0], passageiros));

        await context.SaveChangesAsync(cancellationToken);
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

    private static IEnumerable<Viagem> CriarViagens(IReadOnlyList<Rota> rotas, IDateTimeProvider clock)
    {
        var baseDia = clock.UtcNow.UtcDateTime.Date;
        var horarios = new[] { 8, 14, 22 };
        var diasOffset = new[] { 1, 3, 6, 10 };
        var precosBase = new[] { 89.90m, 120.00m, 95.50m, 149.90m, 79.00m, 135.00m };

        for (var i = 0; i < rotas.Count; i++)
        {
            var rota = rotas[i];
            // Alterna dias/horários por rota para variar sem gerar viagens demais.
            for (var j = 0; j < diasOffset.Length; j++)
            {
                var dia = baseDia.AddDays(diasOffset[j]);
                var hora = horarios[(i + j) % horarios.Length];
                var partida = new DateTimeOffset(dia.AddHours(hora), TimeSpan.Zero);
                var preco = precosBase[i] + j * 10m;
                yield return new Viagem(rota.Id, partida, preco, totalAssentos: 42);
            }
        }
    }

    private static List<Passageiro> CriarPassageiros() =>
    [
        new("Mariana Oliveira", Cpf.Criar("52998224725"), "mariana.oliveira@exemplo.com", new DateOnly(1992, 3, 14)),
        new("Carlos Andrade", Cpf.Criar("16899535009"), "carlos.andrade@exemplo.com", new DateOnly(1987, 11, 2)),
    ];

    private static IEnumerable<Reserva> CriarReservasIniciais(Viagem viagem, IReadOnlyList<Passageiro> passageiros)
    {
        // Ocupa alguns assentos para o mapa não nascer vazio.
        yield return new Reserva(viagem.Id, passageiros[0].Id, 1, CodigoReserva.Gerar());
        yield return new Reserva(viagem.Id, passageiros[0].Id, 2, CodigoReserva.Gerar());
        yield return new Reserva(viagem.Id, passageiros[1].Id, 5, CodigoReserva.Gerar());
        yield return new Reserva(viagem.Id, passageiros[1].Id, 12, CodigoReserva.Gerar());
    }
}
