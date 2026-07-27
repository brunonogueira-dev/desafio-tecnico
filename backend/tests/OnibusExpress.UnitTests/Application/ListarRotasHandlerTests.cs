using FluentAssertions;
using OnibusExpress.Application.Features.Rotas;
using OnibusExpress.Domain.Entities;
using OnibusExpress.UnitTests.Fakes;

namespace OnibusExpress.UnitTests.Application;

public sealed class ListarRotasHandlerTests
{
    [Fact]
    public async Task ExecutarAsync_DeveRetornarRotas_ComDuracaoEmMinutos()
    {
        var db = new FakeDatabase();
        db.Rotas.Add(new Rota("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6)));
        var handler = new ListarRotasHandler(new FakeRotaRepository(db));

        var resultado = await handler.ExecutarAsync(CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().ContainSingle();
        resultado.Value[0].Origem.Should().Be("São Paulo");
        resultado.Value[0].Destino.Should().Be("Rio de Janeiro");
        resultado.Value[0].DuracaoMinutos.Should().Be(360);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarListaVazia_QuandoNaoHaRotas()
    {
        var handler = new ListarRotasHandler(new FakeRotaRepository(new FakeDatabase()));

        var resultado = await handler.ExecutarAsync(CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Should().BeEmpty();
    }
}
