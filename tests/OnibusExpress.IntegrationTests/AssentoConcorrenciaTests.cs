using System.Net;
using FluentAssertions;
using OnibusExpress.IntegrationTests.Infrastructure;

namespace OnibusExpress.IntegrationTests;

public sealed class AssentoConcorrenciaTests(OnibusApiFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task PostReservas_NoMesmoAssentoDuasVezes_DeveRetornar409NaSegunda()
    {
        var viagemId = await Factory.SeedViagemAsync(Factory.Clock.UtcNow.AddDays(5));

        var primeira = await PostReservaAsync(viagemId, assento: 10, cpf: CpfGen.Valido(1));
        var segunda = await PostReservaAsync(viagemId, assento: 10, cpf: CpfGen.Valido(2));

        primeira.StatusCode.Should().Be(HttpStatusCode.Created);
        segunda.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // Prova de que o índice único parcial resolve a corrida: 10 requisições
    // simultâneas no mesmo assento, cada uma com CPF distinto.
    [Fact]
    public async Task PostReservas_DezRequisicoesParalelasNoMesmoAssento_DeveTerExatamenteUmaVencedora()
    {
        var viagemId = await Factory.SeedViagemAsync(Factory.Clock.UtcNow.AddDays(5));

        var tarefas = Enumerable.Range(1, 10)
            .Select(i => PostReservaAsync(viagemId, assento: 7, cpf: CpfGen.Valido(i)))
            .ToArray();

        var respostas = await Task.WhenAll(tarefas);

        respostas.Count(r => r.StatusCode == HttpStatusCode.Created).Should().Be(1);
        respostas.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(9);
    }

    // Edge: mesmo CPF novo reservando assentos DIFERENTES em paralelo. A corrida
    // no índice único de Passageiro.Cpf não pode virar 500 — vira 409 (retryável).
    [Fact]
    public async Task PostReservas_MesmoCpfNovoEmParalelo_NuncaRetorna500()
    {
        var viagemId = await Factory.SeedViagemAsync(Factory.Clock.UtcNow.AddDays(5));
        var cpf = CpfGen.Valido(42);

        var tarefas = Enumerable.Range(1, 5)
            .Select(assento => PostReservaAsync(viagemId, assento, cpf))
            .ToArray();

        var respostas = await Task.WhenAll(tarefas);

        respostas.Should().NotContain(r => r.StatusCode == HttpStatusCode.InternalServerError);
        respostas.Count(r => r.StatusCode == HttpStatusCode.Created).Should().BeGreaterThanOrEqualTo(1);
        respostas.Should().OnlyContain(r =>
            r.StatusCode == HttpStatusCode.Created || r.StatusCode == HttpStatusCode.Conflict);
    }
}
