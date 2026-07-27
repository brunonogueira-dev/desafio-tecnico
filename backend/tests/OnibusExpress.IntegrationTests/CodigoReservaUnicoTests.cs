using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using OnibusExpress.IntegrationTests.Infrastructure;

namespace OnibusExpress.IntegrationTests;

public sealed partial class CodigoReservaUnicoTests(OnibusApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task PostReservas_VariasReservas_DevemTerCodigosDistintosNoPadrao()
    {
        var viagemId = await Factory.SeedViagemAsync(Factory.Clock.UtcNow.AddDays(5));
        var codigos = new List<string>();

        for (var assento = 1; assento <= 15; assento++)
        {
            var resposta = await PostReservaAsync(viagemId, assento, CpfGen.Valido(assento));
            resposta.StatusCode.Should().Be(HttpStatusCode.Created);
            using var doc = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
            codigos.Add(doc.RootElement.GetProperty("codigo").GetString()!);
        }

        codigos.Should().OnlyHaveUniqueItems();
        codigos.Should().OnlyContain(c => PadraoCodigo().IsMatch(c));
    }

    [GeneratedRegex(@"^[A-Z]{3}-[0-9]{5}$")]
    private static partial Regex PadraoCodigo();
}
