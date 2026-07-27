using FluentAssertions;
using OnibusExpress.Application.Common;
using OnibusExpress.Application.Features.Reservas;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.Enums;
using OnibusExpress.Domain.ValueObjects;
using OnibusExpress.UnitTests.Fakes;
using OnibusExpress.UnitTests.TestDoubles;

namespace OnibusExpress.UnitTests.Application;

public sealed class CriarReservaHandlerTests
{
    private static readonly DateTimeOffset Partida =
        new(2026, 8, 10, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset Agora =
        new(2026, 8, 1, 0, 0, 0, TimeSpan.Zero);

    private const string CpfValido = "52998224725";

    private sealed record Cenario(
        FakeDatabase Db,
        Viagem Viagem,
        CriarReservaHandler Handler,
        FakeUnitOfWork UnitOfWork);

    private static Cenario Montar(DateTimeOffset? agora = null, int totalAssentos = 42)
    {
        var db = new FakeDatabase();
        var rota = new Rota("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
        var viagem = new Viagem(rota.Id, Partida, precoBase: 120m, totalAssentos: totalAssentos);
        db.Rotas.Add(rota);
        db.Viagens.Add(viagem);

        var uow = new FakeUnitOfWork();
        var handler = new CriarReservaHandler(
            new FakeViagemRepository(db),
            new FakeReservaRepository(db),
            new FakePassageiroRepository(db),
            uow,
            new FakeDateTimeProvider(agora ?? Agora));

        return new Cenario(db, viagem, handler, uow);
    }

    private static CriarReservaRequest Request(Guid viagemId, int assento = 5, string cpf = CpfValido) =>
        new(viagemId, assento, new PassageiroInput("Ana Souza", cpf, "ana@exemplo.com", new DateOnly(1990, 5, 20)));

    [Fact]
    public async Task ExecutarAsync_DeveCriarReserva_QuandoTudoValido()
    {
        var c = Montar();

        var resultado = await c.Handler.ExecutarAsync(Request(c.Viagem.Id), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.NumeroAssento.Should().Be(5);
        resultado.Value.Status.Should().Be(nameof(StatusReserva.Confirmada));
        resultado.Value.Codigo.Should().MatchRegex(@"^[A-Z]{3}-[0-9]{5}$");
        resultado.Value.Passageiro.CpfFormatado.Should().Be("529.982.247-25");
        c.Db.Reservas.Should().ContainSingle();
        c.Db.Passageiros.Should().ContainSingle();
        c.UnitOfWork.Chamadas.Should().Be(1);
    }

    [Fact]
    public async Task ExecutarAsync_DeveReaproveitarPassageiro_QuandoCpfJaExiste()
    {
        var c = Montar();
        c.Db.Passageiros.Add(new Passageiro("Nome Antigo", Cpf.Criar(CpfValido), "antigo@x.com", new DateOnly(1990, 5, 20)));

        var resultado = await c.Handler.ExecutarAsync(Request(c.Viagem.Id), CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        c.Db.Passageiros.Should().ContainSingle("não deve duplicar passageiro com o mesmo CPF");
        c.Db.Passageiros[0].Nome.Should().Be("Ana Souza", "reuso atualiza o nome informado");
    }

    [Fact]
    public async Task ExecutarAsync_DeveFalharComValidacao_QuandoDataNascimentoNoFuturo()
    {
        var c = Montar();
        var futura = new PassageiroInput("Ana", CpfValido, "ana@x.com", DateOnly.FromDateTime(Agora.UtcDateTime).AddDays(1));

        var resultado = await c.Handler.ExecutarAsync(
            new CriarReservaRequest(c.Viagem.Id, 5, futura), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be(ErrorCode.Validacao);
        c.Db.Reservas.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecutarAsync_DeveFalharComValidacao_QuandoCpfInvalido()
    {
        var c = Montar();

        var resultado = await c.Handler.ExecutarAsync(
            Request(c.Viagem.Id, cpf: "12345678900"), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be(ErrorCode.Validacao);
        c.Db.Reservas.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecutarAsync_DeveFalharComNaoEncontrado_QuandoViagemNaoExiste()
    {
        var c = Montar();

        var resultado = await c.Handler.ExecutarAsync(Request(Guid.NewGuid()), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be(ErrorCode.NaoEncontrado);
    }

    [Fact]
    public async Task ExecutarAsync_DeveFalharComValidacao_QuandoAssentoForaDoRange()
    {
        var c = Montar(totalAssentos: 42);

        var resultado = await c.Handler.ExecutarAsync(
            Request(c.Viagem.Id, assento: 99), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be(ErrorCode.Validacao);
    }

    [Fact]
    public async Task ExecutarAsync_DeveFalharComAssentoIndisponivel_QuandoAssentoJaConfirmado()
    {
        var c = Montar();
        var passageiro = new Passageiro("Outro", Cpf.Criar("16899535009"), "o@x.com", new DateOnly(1985, 1, 1));
        c.Db.Passageiros.Add(passageiro);
        c.Db.Reservas.Add(new Reserva(c.Viagem.Id, passageiro.Id, 5, CodigoReserva.Gerar()));

        var resultado = await c.Handler.ExecutarAsync(
            Request(c.Viagem.Id, assento: 5), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be(ErrorCode.AssentoIndisponivel);
    }

    [Fact]
    public async Task ExecutarAsync_DeveFalharComViagemJaPartiu_QuandoPartidaNoPassado()
    {
        var c = Montar(agora: Partida.AddMinutes(1));

        var resultado = await c.Handler.ExecutarAsync(Request(c.Viagem.Id), CancellationToken.None);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error!.Code.Should().Be(ErrorCode.ViagemJaPartiu);
    }
}
