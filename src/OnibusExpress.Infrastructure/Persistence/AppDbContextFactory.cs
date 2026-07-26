using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OnibusExpress.Infrastructure.Persistence;

/// <summary>
/// Fábrica usada apenas em tempo de design pelo dotnet-ef para gerar migrations.
/// A connection string aqui não precisa apontar para um banco real — o comando
/// só inspeciona o modelo, não conecta.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Postgres")
            ?? "Host=localhost;Port=5432;Database=onibus_express;Username=onibus;Password=postgres";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
