using FluentAssertions;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.Enums;
using OnibusExpress.Domain.Exceptions;
using OnibusExpress.Domain.ValueObjects;
using OnibusExpress.UnitTests.TestDoubles;

namespace OnibusExpress.UnitTests.Entities;

public sealed class ReservaTests
{
    private static readonly DateTimeOffset Partida =
        new(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);

    private static Reserva CriarReservaConfirmada() =>
        new(Guid.NewGuid(), Guid.NewGuid(), numeroAssento: 10, CodigoReserva.Gerar());

    [Fact]
    public void Nova_DeveNascerConfirmada()
    {
        CriarReservaConfirmada().Status.Should().Be(StatusReserva.Confirmada);
    }

    [Fact]
    public void PodeSerCancelada_DevePermitir_Quando3HorasAntes()
    {
        var reserva = CriarReservaConfirmada();
        var clock = new FakeDateTimeProvider(Partida.AddHours(-3));

        reserva.PodeSerCancelada(Partida, clock).Should().BeTrue();
    }

    [Fact]
    public void PodeSerCancelada_DeveNegar_Quando1HoraAntes()
    {
        var reserva = CriarReservaConfirmada();
        var clock = new FakeDateTimeProvider(Partida.AddHours(-1));

        reserva.PodeSerCancelada(Partida, clock).Should().BeFalse();
    }

    // Escolha documentada: no limite EXATO de 2h (inclusivo), o cancelamento
    // ainda é permitido.
    [Fact]
    public void PodeSerCancelada_DevePermitir_QuandoFaltamExatamente2Horas()
    {
        var reserva = CriarReservaConfirmada();
        var clock = new FakeDateTimeProvider(Partida.AddHours(-2));

        reserva.PodeSerCancelada(Partida, clock).Should().BeTrue();
    }

    [Fact]
    public void PodeSerCancelada_DeveNegar_QuandoFaltam1h59()
    {
        var reserva = CriarReservaConfirmada();
        var clock = new FakeDateTimeProvider(Partida.AddHours(-2).AddMinutes(1));

        reserva.PodeSerCancelada(Partida, clock).Should().BeFalse();
    }

    [Fact]
    public void Cancelar_DeveMudarStatus_QuandoDentroDoPrazo()
    {
        var reserva = CriarReservaConfirmada();
        var clock = new FakeDateTimeProvider(Partida.AddHours(-3));

        reserva.Cancelar(Partida, clock);

        reserva.Status.Should().Be(StatusReserva.Cancelada);
    }

    [Fact]
    public void Cancelar_DeveLancar_QuandoForaDoPrazo()
    {
        var reserva = CriarReservaConfirmada();
        var clock = new FakeDateTimeProvider(Partida.AddHours(-1));

        var acao = () => reserva.Cancelar(Partida, clock);

        acao.Should().Throw<DomainException>();
        reserva.Status.Should().Be(StatusReserva.Confirmada);
    }

    [Fact]
    public void Cancelar_DeveLancar_QuandoJaCancelada()
    {
        var reserva = CriarReservaConfirmada();
        var clock = new FakeDateTimeProvider(Partida.AddHours(-3));
        reserva.Cancelar(Partida, clock);

        var acao = () => reserva.Cancelar(Partida, clock);

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void PrazoMinimo_DeveSer2Horas()
    {
        Reserva.PrazoMinimoAntecedenciaCancelamento.Should().Be(TimeSpan.FromHours(2));
    }
}
