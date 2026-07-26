using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.Enums;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.UnitTests.Fakes;

public sealed class FakeRotaRepository(FakeDatabase db) : IRotaRepository
{
    public Task<IReadOnlyList<Rota>> ListarAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Rota>>(db.Rotas.OrderBy(r => r.Origem).ToList());
}

public sealed class FakeViagemRepository(FakeDatabase db) : IViagemRepository
{
    public Task<IReadOnlyList<ViagemComOcupacao>> BuscarAsync(
        string origem, string destino, DateOnly dataPartida, CancellationToken cancellationToken)
    {
        var resultado = db.Viagens
            .Select(v => new { Viagem = v, Rota = db.Rotas.First(r => r.Id == v.RotaId) })
            .Where(x =>
                string.Equals(x.Rota.Origem, origem, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.Rota.Destino, destino, StringComparison.OrdinalIgnoreCase) &&
                DateOnly.FromDateTime(x.Viagem.DataHoraPartida.UtcDateTime) == dataPartida)
            .OrderBy(x => x.Viagem.DataHoraPartida)
            .Select(x => new ViagemComOcupacao(
                x.Viagem.Id, x.Rota.Origem, x.Rota.Destino, x.Viagem.DataHoraPartida,
                x.Rota.DuracaoEstimada, x.Viagem.PrecoBase, x.Viagem.TotalAssentos,
                db.Reservas.Count(res => res.ViagemId == x.Viagem.Id && res.Status == StatusReserva.Confirmada)))
            .ToList();

        return Task.FromResult<IReadOnlyList<ViagemComOcupacao>>(resultado);
    }

    public Task<Viagem?> ObterComRotaAsync(Guid id, CancellationToken cancellationToken)
    {
        var viagem = db.Viagens.FirstOrDefault(v => v.Id == id);
        if (viagem is not null)
        {
            viagem.ComRota(db.Rotas.First(r => r.Id == viagem.RotaId));
        }

        return Task.FromResult(viagem);
    }

    public Task<IReadOnlyList<int>> ObterAssentosOcupadosAsync(Guid viagemId, CancellationToken cancellationToken)
    {
        var ocupados = db.Reservas
            .Where(r => r.ViagemId == viagemId && r.Status == StatusReserva.Confirmada)
            .Select(r => r.NumeroAssento)
            .OrderBy(n => n)
            .ToList();

        return Task.FromResult<IReadOnlyList<int>>(ocupados);
    }
}

public sealed class FakePassageiroRepository(FakeDatabase db) : IPassageiroRepository
{
    public Task<Passageiro?> ObterPorCpfAsync(Cpf cpf, CancellationToken cancellationToken) =>
        Task.FromResult(db.Passageiros.FirstOrDefault(p => p.Cpf == cpf));

    public void Adicionar(Passageiro passageiro) => db.Passageiros.Add(passageiro);
}

public sealed class FakeReservaRepository(FakeDatabase db) : IReservaRepository
{
    public Task<Reserva?> ObterPorCodigoComViagemAsync(CodigoReserva codigo, CancellationToken cancellationToken)
    {
        var reserva = db.Reservas.FirstOrDefault(r => r.Codigo == codigo);
        if (reserva is not null)
        {
            var viagem = db.Viagens.First(v => v.Id == reserva.ViagemId);
            viagem.ComRota(db.Rotas.First(r => r.Id == viagem.RotaId));
            reserva.ComViagem(viagem)
                   .ComPassageiro(db.Passageiros.First(p => p.Id == reserva.PassageiroId));
        }

        return Task.FromResult(reserva);
    }

    public Task<bool> ExisteConfirmadaParaAssentoAsync(Guid viagemId, int numeroAssento, CancellationToken cancellationToken) =>
        Task.FromResult(db.Reservas.Any(r =>
            r.ViagemId == viagemId &&
            r.NumeroAssento == numeroAssento &&
            r.Status == StatusReserva.Confirmada));

    public Task<bool> CodigoEmUsoAsync(CodigoReserva codigo, CancellationToken cancellationToken) =>
        Task.FromResult(db.Reservas.Any(r => r.Codigo == codigo));

    public void Adicionar(Reserva reserva) => db.Reservas.Add(reserva);
}

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int Chamadas { get; private set; }

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken)
    {
        Chamadas++;
        return Task.FromResult(1);
    }
}
