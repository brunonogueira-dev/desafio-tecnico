using System.Net;
using System.Text.Json;
using FluentAssertions;
using OnibusExpress.IntegrationTests.Infrastructure;

namespace OnibusExpress.IntegrationTests;

public sealed class ReservaCpfTests(OnibusApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task PostReservas_ComCpfInvalido_DeveRetornar400ComCampoApontado()
    {
        var viagemId = await Factory.SeedViagemAsync(Factory.Clock.UtcNow.AddDays(5));

        var resposta = await PostReservaAsync(viagemId, assento: 3, cpf: "12345678900");

        resposta.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        using var doc = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("errors").TryGetProperty("cpf", out _)
            .Should().BeTrue("o ProblemDetails deve apontar o campo cpf");
    }

    [Fact]
    public async Task PostReservas_ComCpfValido_DeveRetornar201()
    {
        var viagemId = await Factory.SeedViagemAsync(Factory.Clock.UtcNow.AddDays(5));

        var resposta = await PostReservaAsync(viagemId, assento: 3, cpf: CpfGen.Valido(1));

        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
