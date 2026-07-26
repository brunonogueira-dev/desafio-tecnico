using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using OnibusExpress.Application.Features.Reservas;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnibusExpress.Api.Swagger;

/// <summary>Injeta exemplos de request/response nos principais DTOs do Swagger.</summary>
public sealed class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CriarReservaRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["viagemId"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                ["numeroAssento"] = new OpenApiInteger(12),
                ["passageiro"] = new OpenApiObject
                {
                    ["nome"] = new OpenApiString("Ana Souza"),
                    ["cpf"] = new OpenApiString("529.982.247-25"),
                    ["email"] = new OpenApiString("ana@exemplo.com"),
                    ["dataNascimento"] = new OpenApiString("1990-05-20"),
                },
            };
        }
        else if (context.Type == typeof(ReservaDto))
        {
            schema.Example = new OpenApiObject
            {
                ["codigo"] = new OpenApiString("ABC-23456"),
                ["status"] = new OpenApiString("Confirmada"),
                ["numeroAssento"] = new OpenApiInteger(12),
                ["viagem"] = new OpenApiObject
                {
                    ["id"] = new OpenApiString("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    ["origem"] = new OpenApiString("São Paulo"),
                    ["destino"] = new OpenApiString("Rio de Janeiro"),
                    ["dataHoraPartida"] = new OpenApiString("2026-08-10T12:00:00+00:00"),
                    ["duracaoMinutos"] = new OpenApiInteger(360),
                    ["precoBase"] = new OpenApiDouble(120.00),
                },
                ["passageiro"] = new OpenApiObject
                {
                    ["nome"] = new OpenApiString("Ana Souza"),
                    ["cpfFormatado"] = new OpenApiString("529.982.247-25"),
                    ["email"] = new OpenApiString("ana@exemplo.com"),
                },
            };
        }
    }
}
