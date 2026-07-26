using FluentAssertions;
using OnibusExpress.Application.Common;
using OnibusExpress.Application.Features.Reservas;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.Enums;
using OnibusExpress.Domain.ValueObjects;
using OnibusExpress.UnitTests.Fakes;
using OnibusExpress.UnitTests.TestDoubles;

namespace OnibusExpress.UnitTests.Application;

public sealed class CancelarReservaHandlerTests
{
    private static readonly DateTimeOffset Partida =
        new(2026, 8, 10, 9, 0, 0, TimeSpan.Zero);

    private static (FakeDatabase Db, Reserva Reserva) ComReserva()
    {
        var db = new FakeDatabase();
        var rota = new Rota("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
        var viagem = new Viagem(rota.Id, Partida, 120m, 42);
        var passageiro = new Passageiro("Ana", Cpf.Criar("52998224725"), "ana@x.com", new DateOnly(1990, 5, 20));
        var reserva = new Reserva(viagem.Id, passageiro.Id, 5, CodigoReserva.Parse("ABC-23456"));
        db.Rotas.Add(rota);
        db.Viagens.Add(viagem);
        db.Passageiros.Add(passageiro);
        db.Reservas.Add(reserva);
        return (db, reserva);
    }

    private static CancelarReservaHandler Handler(FakeDatabase db, DateTimeOffset agora, out FakeUnitOfWork uow)
    {
        uow = new FakeUnitOfWork();
        return new CancelarReservaHandler(new FakeReservaRepository(db), uow, new FakeDateTimeProvider(agora));
    }

    [Fact]
    public async Task ExecutarAsync_DeveCancelar_QuandoDentroDoPrazo()
    {
        var (db, reserva) = ComReserva();
        var handler = Handler(db, Partida.AddHours(-3), out var uow);

        var resultado = await handler.ExecutarAsync("ABC-23456", CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        reserva.Status.Should().Be(StatusReserva.Cancelada);
        uow.Chamadas.Should().Be(1);
    }

    [Fact]
    public async Task ExecutarAsync_DeveFalharComNaoEncontrado_QuandoCodigoNaoExiste()
    {
        var (db, _) = ComReserva();
        var handler = Handler(db, Partida.AddHours(-3), out _);

        var resultado = await handler.ExecutarAsync("ZZZ-99999", CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be(ErrorCode.NaoEncontrado);
    }

    [Fact]
    public async Task ExecutarAsync_DeveFalharComForaDoPrazo_QuandoFalta1Hora()
    {
        var (db, reserva) = ComReserva();
        var handler = Handler(db, Partida.AddHours(-1), out _);

        var resultado = await handler.ExecutarAsync("ABC-23456", CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be(ErrorCode.ForaDoPrazoDeCancelamento);
        reserva.Status.Should().Be(StatusReserva.Confirmada);
    }

    [Fact]
    public async Task ExecutarAsync_DeveFalharComJaCancelada_QuandoReservaJaCancelada()
    {
        var (db, reserva) = ComReserva();
        reserva.Cancelar(Partida, new FakeDateTimeProvider(Partida.AddHours(-5)));
        var handler = Handler(db, Partida.AddHours(-3), out _);

        var resultado = await handler.ExecutarAsync("ABC-23456", CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be(ErrorCode.ReservaJaCancelada);
    }
}
