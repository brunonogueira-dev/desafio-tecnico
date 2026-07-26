using Microsoft.Extensions.DependencyInjection;
using OnibusExpress.Application.Features.Reservas;
using OnibusExpress.Application.Features.Rotas;
using OnibusExpress.Application.Features.Viagens;

namespace OnibusExpress.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ListarRotasHandler>();
        services.AddScoped<BuscarViagensHandler>();
        services.AddScoped<ObterDetalhesViagemHandler>();
        services.AddScoped<CriarReservaHandler>();
        services.AddScoped<ConsultarReservaHandler>();
        services.AddScoped<CancelarReservaHandler>();

        return services;
    }
}
