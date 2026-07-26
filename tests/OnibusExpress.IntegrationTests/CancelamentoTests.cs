using System.Net;
using System.Text.Json;
using FluentAssertions;
using OnibusExpress.IntegrationTests.Infrastructure;

namespace OnibusExpress.IntegrationTests;

public sealed class CancelamentoTests(OnibusApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Delete_TresHorasAntes_DeveRetornar204ELiberarOAssento()
    {
        var partida = new DateTimeOffset(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);
        var viagemId = await Factory.SeedViagemAsync(partida);
        var codigo = await CriarReservaEObterCodigo(viagemId, assento: 5);

        // Avança o relógio para 3h antes da partida.
        Factory.Clock.UtcNow = partida.AddHours(-3);
        var cancelamento = await Client.DeleteAsync($"/reservas/{codigo}");

        cancelamento.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var assentoOcupado = await AssentoEstaOcupado(viagemId, 5);
        assentoOcupado.Should().BeFalse("cancelar deve liberar o assento no mapa");
    }

    [Fact]
    public async Task Delete_UmaHoraAntes_DeveRetornar409()
    {
        var partida = new DateTimeOffset(2026, 1, 10, 12, 0, 0, TimeSpan.Zero);
        var viagemId = await Factory.SeedViagemAsync(partida);
        var codigo = await CriarReservaEObterCodigo(viagemId, assento: 5);

        Factory.Clock.UtcNow = partida.AddHours(-1);
        var cancelamento = await Client.DeleteAsync($"/reservas/{codigo}");

        cancelamento.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    private async Task<string> CriarReservaEObterCodigo(Guid viagemId, int assento)
    {
        var resposta = await PostReservaAsync(viagemId, assento, CpfGen.Valido(assento));
        resposta.StatusCode.Should().Be(HttpStatusCode.Created);
        using var doc = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("codigo").GetString()!;
    }

    private async Task<bool> AssentoEstaOcupado(Guid viagemId, int numero)
    {
        var resposta = await Client.GetAsync($"/viagens/{viagemId}");
        resposta.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await resposta.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("assentos").EnumerateArray()
            .First(a => a.GetProperty("numero").GetInt32() == numero)
            .GetProperty("ocupado").GetBoolean();
    }
}
