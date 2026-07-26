using FluentAssertions;
using OnibusExpress.Application.Features.Viagens;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.ValueObjects;
using OnibusExpress.UnitTests.Fakes;

namespace OnibusExpress.UnitTests.Application;

public sealed class BuscarViagensHandlerTests
{
    private static readonly DateTimeOffset Partida =
        new(2026, 8, 10, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecutarAsync_DeveRetornarViagens_ComVagasDerivadas()
    {
        var db = new FakeDatabase();
        var rota = new Rota("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
        var viagem = new Viagem(rota.Id, Partida, precoBase: 120m, totalAssentos: 42);
        db.Rotas.Add(rota);
        db.Viagens.Add(viagem);
        // duas reservas confirmadas => 40 vagas
        var passageiro = new Passageiro("Ana", Cpf.Criar("52998224725"), "ana@x.com", new DateOnly(1990, 1, 1));
        db.Passageiros.Add(passageiro);
        db.Reservas.Add(new Reserva(viagem.Id, passageiro.Id, 1, CodigoReserva.Gerar()));
        db.Reservas.Add(new Reserva(viagem.Id, passageiro.Id, 2, CodigoReserva.Gerar()));

        var handler = new BuscarViagensHandler(new FakeViagemRepository(db));
        var request = new BuscarViagensRequest("São Paulo", "Rio de Janeiro", new DateOnly(2026, 8, 10));

        var resultado = await handler.ExecutarAsync(request, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().ContainSingle();
        resultado.Value[0].TotalAssentos.Should().Be(42);
        resultado.Value[0].VagasDisponiveis.Should().Be(40);
        resultado.Value[0].DuracaoMinutos.Should().Be(360);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarListaVazia_QuandoNenhumaViagemCasaOsFiltros()
    {
        var db = new FakeDatabase();
        var rota = new Rota("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
        db.Rotas.Add(rota);
        db.Viagens.Add(new Viagem(rota.Id, Partida, 120m, 42));

        var handler = new BuscarViagensHandler(new FakeViagemRepository(db));
        var request = new BuscarViagensRequest("Curitiba", "Florianópolis", new DateOnly(2026, 8, 10));

        var resultado = await handler.ExecutarAsync(request, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().BeEmpty();
    }
}
