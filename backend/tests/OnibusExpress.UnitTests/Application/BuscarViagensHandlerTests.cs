using FluentAssertions;
using OnibusExpress.Application.Features.Viagens;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.ValueObjects;
using OnibusExpress.UnitTests.Fakes;

namespace OnibusExpress.UnitTests.Application;

public sealed class BuscarViagensHandlerTests
{
    private static readonly DateOnly Dia = new(2026, 8, 10);

    private static DateTimeOffset EmHora(int hora) =>
        new(2026, 8, 10, hora, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExecutarAsync_DeveRetornarViagens_ComVagasDerivadas()
    {
        var db = new FakeDatabase();
        var rota = new Rota("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
        var viagem = new Viagem(rota.Id, EmHora(9), precoBase: 120m, totalAssentos: 42);
        db.Rotas.Add(rota);
        db.Viagens.Add(viagem);
        // duas reservas confirmadas => 40 vagas
        var passageiro = new Passageiro("Ana", Cpf.Criar("52998224725"), "ana@x.com", new DateOnly(1990, 1, 1));
        db.Passageiros.Add(passageiro);
        db.Reservas.Add(new Reserva(viagem.Id, passageiro.Id, 1, CodigoReserva.Gerar()));
        db.Reservas.Add(new Reserva(viagem.Id, passageiro.Id, 2, CodigoReserva.Gerar()));

        var handler = new BuscarViagensHandler(new FakeViagemRepository(db));
        var request = new BuscarViagensRequest("São Paulo", "Rio de Janeiro", Dia, Pagina: 1, Tamanho: 10);

        var resultado = await handler.ExecutarAsync(request, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Itens.Should().ContainSingle();
        resultado.Value.Total.Should().Be(1);
        resultado.Value.Itens[0].TotalAssentos.Should().Be(42);
        resultado.Value.Itens[0].VagasDisponiveis.Should().Be(40);
        resultado.Value.Itens[0].DuracaoMinutos.Should().Be(360);
    }

    [Fact]
    public async Task ExecutarAsync_DeveRetornarPaginaVazia_QuandoNenhumaViagemCasaOsFiltros()
    {
        var db = new FakeDatabase();
        var rota = new Rota("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
        db.Rotas.Add(rota);
        db.Viagens.Add(new Viagem(rota.Id, EmHora(9), 120m, 42));

        var handler = new BuscarViagensHandler(new FakeViagemRepository(db));
        var request = new BuscarViagensRequest("Curitiba", "Florianópolis", Dia, Pagina: 1, Tamanho: 10);

        var resultado = await handler.ExecutarAsync(request, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Itens.Should().BeEmpty();
        resultado.Value.Total.Should().Be(0);
        resultado.Value.TotalPaginas.Should().Be(0);
    }

    [Fact]
    public async Task ExecutarAsync_SemOrigemEDestino_DeveRetornarTodasAsViagensDoDia()
    {
        var db = new FakeDatabase();
        var sp = new Rota("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
        var cwb = new Rota("Curitiba", "Florianópolis", TimeSpan.FromHours(4));
        db.Rotas.Add(sp);
        db.Rotas.Add(cwb);
        db.Viagens.Add(new Viagem(sp.Id, EmHora(8), 120m, 42));
        db.Viagens.Add(new Viagem(cwb.Id, EmHora(9), 80m, 42));

        var handler = new BuscarViagensHandler(new FakeViagemRepository(db));
        var request = new BuscarViagensRequest(Origem: null, Destino: null, Dia, Pagina: 1, Tamanho: 10);

        var resultado = await handler.ExecutarAsync(request, CancellationToken.None);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Total.Should().Be(2);
        resultado.Value.Itens.Should().HaveCount(2);
    }

    [Fact]
    public async Task ExecutarAsync_DevePaginarDeDezEmDez()
    {
        var db = new FakeDatabase();
        var rota = new Rota("São Paulo", "Rio de Janeiro", TimeSpan.FromHours(6));
        db.Rotas.Add(rota);
        for (var hora = 0; hora < 12; hora++)
        {
            db.Viagens.Add(new Viagem(rota.Id, EmHora(hora), 100m, 42));
        }

        var handler = new BuscarViagensHandler(new FakeViagemRepository(db));

        var pagina1 = await handler.ExecutarAsync(
            new BuscarViagensRequest(null, null, Dia, Pagina: 1, Tamanho: 10), CancellationToken.None);
        var pagina2 = await handler.ExecutarAsync(
            new BuscarViagensRequest(null, null, Dia, Pagina: 2, Tamanho: 10), CancellationToken.None);

        pagina1.Value.Itens.Should().HaveCount(10);
        pagina1.Value.Total.Should().Be(12);
        pagina1.Value.TotalPaginas.Should().Be(2);
        pagina2.Value.Itens.Should().HaveCount(2);
    }
}
