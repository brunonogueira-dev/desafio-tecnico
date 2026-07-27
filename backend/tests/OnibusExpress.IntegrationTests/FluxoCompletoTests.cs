using System.Net;
using System.Text.Json;
using FluentAssertions;
using OnibusExpress.IntegrationTests.Infrastructure;

namespace OnibusExpress.IntegrationTests;

public sealed class FluxoCompletoTests(OnibusApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Fluxo_BuscarDetalharReservarConsultarCancelar_DeveFuncionar()
    {
        var partida = new DateTimeOffset(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);
        var viagemId = await Factory.SeedViagemAsync(partida);

        // Buscar
        var busca = await Client.GetAsync("/viagens?origem=S%C3%A3o%20Paulo&destino=Rio%20de%20Janeiro&data=2026-01-10");
        busca.StatusCode.Should().Be(HttpStatusCode.OK);
        (await Ler(busca)).GetArrayLength().Should().Be(1);

        // Detalhar
        var detalhe = await Client.GetAsync($"/viagens/{viagemId}");
        detalhe.StatusCode.Should().Be(HttpStatusCode.OK);
        (await Ler(detalhe)).GetProperty("assentos").GetArrayLength().Should().Be(42);

        // Reservar
        var reserva = await PostReservaAsync(viagemId, 8, CpfGen.Valido(8));
        reserva.StatusCode.Should().Be(HttpStatusCode.Created);
        var codigo = (await Ler(reserva)).GetProperty("codigo").GetString()!;

        // Consultar
        var consulta = await Client.GetAsync($"/reservas/{codigo}");
        consulta.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cancelar (dentro do prazo)
        Factory.Clock.UtcNow = partida.AddHours(-4);
        var cancelamento = await Client.DeleteAsync($"/reservas/{codigo}");
        cancelamento.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PostReservas_ViagemJaPartiu_DeveRetornar409()
    {
        var partida = new DateTimeOffset(2026, 1, 5, 12, 0, 0, TimeSpan.Zero);
        var viagemId = await Factory.SeedViagemAsync(partida);
        Factory.Clock.UtcNow = partida.AddMinutes(1);

        var resposta = await PostReservaAsync(viagemId, 3, CpfGen.Valido(3));

        resposta.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetReserva_CodigoInexistente_DeveRetornar404()
    {
        var resposta = await Client.GetAsync("/reservas/ZZZ-99999");

        resposta.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetViagens_SemResultado_DeveRetornar200ComListaVazia()
    {
        var resposta = await Client.GetAsync("/viagens?origem=Curitiba&destino=Florian%C3%B3polis&data=2026-01-10");

        resposta.StatusCode.Should().Be(HttpStatusCode.OK);
        (await Ler(resposta)).GetArrayLength().Should().Be(0);
    }

    // Viagem às 22h de São Paulo = 01h UTC do dia seguinte. A busca deve
    // encontrá-la no dia LOCAL (10/01), não no dia UTC (11/01).
    [Fact]
    public async Task GetViagens_ViagemNoturna_DeveAparecerNoDiaLocalNaoNoDiaUtc()
    {
        // 2026-01-11T01:00Z == 2026-01-10 22:00 em America/Sao_Paulo.
        await Factory.SeedViagemAsync(new DateTimeOffset(2026, 1, 11, 1, 0, 0, TimeSpan.Zero));
        const string filtro = "origem=S%C3%A3o%20Paulo&destino=Rio%20de%20Janeiro";

        var noDiaLocal = await Client.GetAsync($"/viagens?{filtro}&data=2026-01-10");
        var noDiaUtc = await Client.GetAsync($"/viagens?{filtro}&data=2026-01-11");

        (await Ler(noDiaLocal)).GetArrayLength().Should().Be(1, "a viagem pertence ao dia local 10/01");
        (await Ler(noDiaUtc)).GetArrayLength().Should().Be(0, "não deve aparecer no dia 11/01");
    }

    private static async Task<JsonElement> Ler(HttpResponseMessage resposta)
    {
        var texto = await resposta.Content.ReadAsStringAsync();
        return JsonDocument.Parse(texto).RootElement.Clone();
    }
}
