using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using OnibusExpress.Domain.Abstractions;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Infrastructure.Persistence;
using Respawn;
using Testcontainers.PostgreSql;

namespace OnibusExpress.IntegrationTests.Infrastructure;

/// <summary>
/// Sobe um Postgres real via Testcontainers, aponta a API para ele, injeta o
/// TestClock no lugar do relógio real e permite resetar o estado entre testes.
/// </summary>
public sealed class OnibusApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithDatabase("onibus_express_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _connection = null!;

    public TestClock Clock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Postgres", _container.GetConnectionString());

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDateTimeProvider>();
            services.AddSingleton<IDateTimeProvider>(Clock);
        });
    }

    async Task IAsyncLifetime.InitializeAsync()
    {
        await _container.StartAsync();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await context.Database.MigrateAsync();
        }

        _connection = new NpgsqlConnection(_container.GetConnectionString());
        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(_connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task ResetAsync()
    {
        await _respawner.ResetAsync(_connection);
        Clock.UtcNow = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }

    /// <summary>Cria uma rota e uma viagem, retornando o id da viagem.</summary>
    public async Task<Guid> SeedViagemAsync(DateTimeOffset partida, int totalAssentos = 42, decimal preco = 120m)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var rota = new Rota("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
        var viagem = new Viagem(rota.Id, partida, preco, totalAssentos);
        context.Rotas.Add(rota);
        context.Viagens.Add(viagem);
        await context.SaveChangesAsync();
        return viagem.Id;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await _container.DisposeAsync();
        await base.DisposeAsync();
    }
}
