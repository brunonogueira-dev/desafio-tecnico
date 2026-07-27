using System.Text.RegularExpressions;
using FluentAssertions;
using OnibusExpress.Domain.Exceptions;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.UnitTests.ValueObjects;

public sealed class CodigoReservaTests
{
    private static readonly Regex Padrao = new(@"^[A-Z]{3}-[0-9]{5}$");

    [Fact]
    public void Gerar_DeveSempreCasarComPadrao()
    {
        for (var i = 0; i < 10_000; i++)
        {
            var codigo = CodigoReserva.Gerar();
            Padrao.IsMatch(codigo.Valor).Should().BeTrue($"'{codigo.Valor}' deve casar com AAA-99999");
        }
    }

    [Fact]
    public void Gerar_NaoDeveConterCaracteresAmbiguos()
    {
        var ambiguos = new[] { 'I', 'L', 'O', '0', '1' };

        for (var i = 0; i < 10_000; i++)
        {
            var codigo = CodigoReserva.Gerar();
            codigo.Valor.Should().NotContainAny(ambiguos.Select(c => c.ToString()));
        }
    }

    [Fact]
    public void TryParse_DeveAceitar_ComEspacosEMinusculas()
    {
        var ok = CodigoReserva.TryParse("  abc-23456  ", out var codigo);

        ok.Should().BeTrue();
        codigo!.Valor.Should().Be("ABC-23456");
    }

    [Theory]
    [InlineData("ABC12345")]   // sem hífen
    [InlineData("AB-12345")]   // 2 letras
    [InlineData("ABCD-12345")] // 4 letras
    [InlineData("ABC-1234")]   // 4 dígitos
    [InlineData("ABC-123456")] // 6 dígitos
    [InlineData("A1C-12345")]  // dígito no lugar de letra
    [InlineData("")]
    [InlineData(null)]
    public void TryParse_DeveRejeitar_QuandoFormatoInvalido(string? entrada)
    {
        CodigoReserva.TryParse(entrada, out _).Should().BeFalse();
    }

    [Fact]
    public void Parse_DeveLancar_QuandoInvalido()
    {
        var acao = () => CodigoReserva.Parse("invalido");

        acao.Should().Throw<CodigoReservaInvalidoException>();
    }

    [Fact]
    public void Igualdade_DeveSerPorValor()
    {
        var a = CodigoReserva.Parse("ABC-23456");
        var b = CodigoReserva.Parse("abc-23456");

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
