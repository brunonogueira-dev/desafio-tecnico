using FluentAssertions;
using OnibusExpress.Domain.Exceptions;
using OnibusExpress.Domain.ValueObjects;
using OnibusExpress.UnitTests.Common;

namespace OnibusExpress.UnitTests.ValueObjects;

public sealed class CpfTests
{
    [Fact]
    public void Criar_DeveAceitar_QuandoCpfValidoSemMascara()
    {
        var cpf = Cpf.Criar("52998224725");

        cpf.Valor.Should().Be("52998224725");
    }

    [Fact]
    public void Criar_DeveNormalizar_QuandoEntradaComMascara()
    {
        var cpf = Cpf.Criar("529.982.247-25");

        cpf.Valor.Should().Be("52998224725");
    }

    [Fact]
    public void Formatado_DeveAplicarMascara()
    {
        var cpf = Cpf.Criar("52998224725");

        cpf.Formatado.Should().Be("529.982.247-25");
    }

    [Fact]
    public void GerarPorAlgoritmo_DeveSempreSerAceito()
    {
        var rng = new Random(20260726);

        for (var i = 0; i < 1_000; i++)
        {
            var gerado = CpfFactory.GerarValido(rng);
            Cpf.EhValido(gerado).Should().BeTrue($"o CPF gerado {gerado} deve ser válido");
        }
    }

    [Theory]
    [InlineData("52998224724")] // último dígito trocado
    [InlineData("11144477730")] // DV incorreto
    public void TryCriar_DeveRejeitar_QuandoDigitoVerificadorInvalido(string entrada)
    {
        var ok = Cpf.TryCriar(entrada, out var cpf);

        ok.Should().BeFalse();
        cpf.Should().BeNull();
    }

    [Theory]
    [InlineData("00000000000")]
    [InlineData("11111111111")]
    [InlineData("99999999999")]
    public void TryCriar_DeveRejeitar_QuandoTodosDigitosIguais(string entrada)
    {
        Cpf.TryCriar(entrada, out _).Should().BeFalse();
    }

    [Theory]
    [InlineData("123")]
    [InlineData("5299822472")]    // 10 dígitos
    [InlineData("529982247250")]  // 12 dígitos
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void TryCriar_DeveRejeitar_QuandoTamanhoErrado(string? entrada)
    {
        Cpf.TryCriar(entrada, out _).Should().BeFalse();
    }

    [Fact]
    public void Criar_DeveLancar_QuandoInvalido()
    {
        var acao = () => Cpf.Criar("12345678900");

        acao.Should().Throw<CpfInvalidoException>();
    }

    [Fact]
    public void Igualdade_DeveSerPorValor()
    {
        var a = Cpf.Criar("529.982.247-25");
        var b = Cpf.Criar("52998224725");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
