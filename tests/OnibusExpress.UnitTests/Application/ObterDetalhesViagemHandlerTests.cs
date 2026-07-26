using FluentAssertions;
using OnibusExpress.Application.Common;
using OnibusExpress.Application.Features.Viagens;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.ValueObjects;
using OnibusExpress.UnitTests.Fakes;

namespace OnibusExpress.UnitTests.Application;

public sealed class ObterDetalhesViagemHandlerTests
{
    private static readonly DateTimeOffset Partida =
        new(2026, 8, 10, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecutarAsync_DeveMontarMapaDeAssentos_ComOcupadosMarcados()
    {
        var db = new FakeDatabase();
        var rota = new Rota("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
        var viagem = new Viagem(rota.Id, Partida, 120m, totalAssentos: 10);
        var passageiro = new Passageiro("Ana", Cpf.Criar("52998224725"), "ana@x.com", new DateOnly(1990, 1, 1));
        db.Rotas.Add(rota);
        db.Viagens.Add(viagem);
        db.Passageiros.Add(passageiro);
        db.Reservas.Add(new Reserva(viagem.Id, passageiro.Id, 3, CodigoReserva.Gerar()));
        db.Reservas.Add(new Reserva(viagem.Id, passageiro.Id, 7, CodigoReserva.Gerar()));

        var handler = new ObterDetalhesViagemHandler(new FakeViagemRepository(db));

        var resultado = await handler.ExecutarAsync(viagem.Id, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        var detalhe = resultado.Value;
        detalhe.Assentos.Should().HaveCount(10);
        detalhe.Assentos.Select(a => a.Numero).Should().BeInAscendingOrder();
        detalhe.Assentos.Where(a => a.Ocupado).Select(a => a.Numero).Should().BeEquivalentTo(new[] { 3, 7 });
        detalhe.VagasDisponiveis.Should().Be(8);
    }

    [Fact]
    public async Task ExecutarAsync_DeveFalharComNaoEncontrado_QuandoViagemNaoExiste()
    {
        var handler = new ObterDetalhesViagemHandler(new FakeViagemRepository(new FakeDatabase()));

        var resultado = await handler.ExecutarAsync(Guid.NewGuid(), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be(ErrorCode.NaoEncontrado);
    }
}
