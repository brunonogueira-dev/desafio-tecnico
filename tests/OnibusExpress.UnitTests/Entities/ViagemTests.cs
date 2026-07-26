using FluentAssertions;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.Exceptions;
using OnibusExpress.UnitTests.TestDoubles;

namespace OnibusExpress.UnitTests.Entities;

public sealed class ViagemTests
{
    private static readonly DateTimeOffset Partida =
        new(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);

    private static Viagem CriarViagem() =>
        new(Guid.NewGuid(), Partida, precoBase: 150m, totalAssentos: 42);

    [Fact]
    public void JaPartiu_DeveSerFalse_QuandoAgoraAntesDaPartida()
    {
        var viagem = CriarViagem();
        var clock = new FakeDateTimeProvider(Partida.AddMinutes(-1));

        viagem.JaPartiu(clock).Should().BeFalse();
    }

    [Fact]
    public void JaPartiu_DeveSerTrue_QuandoAgoraDepoisDaPartida()
    {
        var viagem = CriarViagem();
        var clock = new FakeDateTimeProvider(Partida.AddMinutes(1));

        viagem.JaPartiu(clock).Should().BeTrue();
    }

    [Fact]
    public void JaPartiu_DeveSerTrue_QuandoAgoraExatamenteNaPartida()
    {
        var viagem = CriarViagem();
        var clock = new FakeDateTimeProvider(Partida);

        viagem.JaPartiu(clock).Should().BeTrue();
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(42, true)]
    [InlineData(43, false)]
    [InlineData(-1, false)]
    public void AssentoDentroDoRange_DeveRefletirLimites(int numero, bool esperado)
    {
        CriarViagem().AssentoDentroDoRange(numero).Should().Be(esperado);
    }

    [Fact]
    public void Construtor_DeveLancar_QuandoPrecoNaoPositivo()
    {
        var acao = () => new Viagem(Guid.NewGuid(), Partida, precoBase: 0m, totalAssentos: 42);

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Construtor_DeveLancar_QuandoTotalAssentosNaoPositivo()
    {
        var acao = () => new Viagem(Guid.NewGuid(), Partida, precoBase: 150m, totalAssentos: 0);

        acao.Should().Throw<DomainException>();
    }

    [Fact]
    public void Construtor_DeveConverterPartidaParaUtc()
    {
        var partidaComOffset = new DateTimeOffset(2026, 8, 1, 9, 0, 0, TimeSpan.FromHours(-3));

        var viagem = new Viagem(Guid.NewGuid(), partidaComOffset, 150m, 42);

        viagem.DataHoraPartida.Offset.Should().Be(TimeSpan.Zero);
        viagem.DataHoraPartida.Should().Be(partidaComOffset.ToUniversalTime());
    }
}
