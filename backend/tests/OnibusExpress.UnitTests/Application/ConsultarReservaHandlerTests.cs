using FluentAssertions;
using OnibusExpress.Application.Common;
using OnibusExpress.Application.Features.Reservas;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.ValueObjects;
using OnibusExpress.UnitTests.Fakes;

namespace OnibusExpress.UnitTests.Application;

public sealed class ConsultarReservaHandlerTests
{
    private static readonly DateTimeOffset Partida =
        new(2026, 8, 10, 9, 0, 0, TimeSpan.Zero);

    private static (FakeDatabase Db, Reserva Reserva) ComReserva()
    {
        var db = new FakeDatabase();
        var rota = new Rota("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
        var viagem = new Viagem(rota.Id, Partida, 120m, 42);
        var passageiro = new Passageiro("Ana Souza", Cpf.Criar("52998224725"), "ana@x.com", new DateOnly(1990, 5, 20));
        var reserva = new Reserva(viagem.Id, passageiro.Id, 5, CodigoReserva.Parse("ABC-23456"));
        db.Rotas.Add(rota);
        db.Viagens.Add(viagem);
        db.Passageiros.Add(passageiro);
        db.Reservas.Add(reserva);
        return (db, reserva);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarReserva_QuandoCodigoExiste()
    {
        var (db, _) = ComReserva();
        var handler = new ConsultarReservaHandler(new FakeReservaRepository(db));

        var resultado = await handler.ExecutarAsync("abc-23456", CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Codigo.Should().Be("ABC-23456");
        resultado.Value.NumeroAssento.Should().Be(5);
        resultado.Value.Viagem.Origem.Should().Be("São Paulo");
        resultado.Value.Passageiro.CpfFormatado.Should().Be("529.982.247-25");
    }

    [Fact]
    public async Task ExecutarAsync_DeveFalharComNaoEncontrado_QuandoCodigoNaoExiste()
    {
        var (db, _) = ComReserva();
        var handler = new ConsultarReservaHandler(new FakeReservaRepository(db));

        var resultado = await handler.ExecutarAsync("ZZZ-99999", CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be(ErrorCode.NaoEncontrado);
    }

    [Fact]
    public async Task ExecutarAsync_DeveFalharComNaoEncontrado_QuandoCodigoMalformado()
    {
        var (db, _) = ComReserva();
        var handler = new ConsultarReservaHandler(new FakeReservaRepository(db));

        var resultado = await handler.ExecutarAsync("codigo-invalido", CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be(ErrorCode.NaoEncontrado);
    }
}
