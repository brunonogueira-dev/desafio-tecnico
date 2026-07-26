using OnibusExpress.Domain.Entities;

namespace OnibusExpress.Application.Features.Reservas;

internal static class ReservaMapper
{
    public static ReservaDto ParaDto(Reserva reserva) =>
        ParaDto(reserva, reserva.Viagem!, reserva.Passageiro!);

    public static ReservaDto ParaDto(Reserva reserva, Viagem viagem, Passageiro passageiro)
    {
        var rota = viagem.Rota!;
        return new ReservaDto(
            reserva.Codigo.Valor,
            reserva.Status.ToString(),
            reserva.NumeroAssento,
            new ReservaViagemDto(
                viagem.Id,
                rota.Origem,
                rota.Destino,
                viagem.DataHoraPartida,
                (int)rota.DuracaoEstimada.TotalMinutes,
                viagem.PrecoBase),
            new PassageiroDto(passageiro.Nome, passageiro.Cpf.Formatado, passageiro.Email));
    }
}
