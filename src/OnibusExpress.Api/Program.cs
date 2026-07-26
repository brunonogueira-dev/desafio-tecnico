using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OnibusExpress.Api.Middleware;
using OnibusExpress.Application;
using OnibusExpress.Domain.Abstractions;
using OnibusExpress.Infrastructure;
using OnibusExpress.Infrastructure.Persistence;
using OnibusExpress.Infrastructure.Persistence.Seed;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OniBus Express API",
        Version = "v1",
        Description = "Venda de passagens rodoviárias. Erros seguem ProblemDetails (RFC 7807)."
    });

    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml))
    {
        options.IncludeXmlComments(xml);
    }
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

var corsOrigins = ResolverOrigensCors(builder.Configuration);
const string CorsPolicy = "frontend";
builder.Services.AddCors(options =>
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "OniBus Express API v1");
    options.RoutePrefix = "swagger";
});

app.UseCors(CorsPolicy);
app.MapControllers();
app.MapHealthChecks("/health");

await AplicarMigrationsESeedAsync(app);

app.Run();

static string[] ResolverOrigensCors(IConfiguration configuration)
{
    var flat = configuration["CORS_ALLOWED_ORIGINS"];
    if (!string.IsNullOrWhiteSpace(flat))
    {
        return flat.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    return configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:5173", "http://localhost:3000"];
}

static async Task AplicarMigrationsESeedAsync(WebApplication app)
{
    if (!app.Environment.IsDevelopment())
    {
        return;
    }

    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var clock = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

    await context.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(context, clock);
}

// Necessário para a WebApplicationFactory<Program> nos testes de integração.
public partial class Program;
