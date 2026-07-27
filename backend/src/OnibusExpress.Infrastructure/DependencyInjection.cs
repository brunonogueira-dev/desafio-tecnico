using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Domain.Abstractions;
using OnibusExpress.Infrastructure.Persistence;
using OnibusExpress.Infrastructure.Persistence.Repositories;
using OnibusExpress.Infrastructure.Time;

namespace OnibusExpress.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException(
                "Connection string 'Postgres' não configurada. Defina ConnectionStrings__Postgres.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IRotaRepository, RotaRepository>();
        services.AddScoped<IViagemRepository, ViagemRepository>();
        services.AddScoped<IPassageiroRepository, PassageiroRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
