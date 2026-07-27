using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Application.Common;
using OnibusExpress.Domain.Abstractions;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Application.Features.Reservas;

/// <summary>Cancela uma reserva pelo código (DELETE /reservas/{codigo}).</summary>
public sealed class CancelarReservaHandler(
    IReservaRepository reservas,
    IUnitOfWork unitOfWork,
    IDateTimeProvider clock)
{
    public async Task<Result> ExecutarAsync(string codigo, CancellationToken cancellationToken)
    {
        if (!CodigoReserva.TryParse(codigo, out var codigoReserva))
        {
            return Result.Failure(Error.NaoEncontrado("Reserva não encontrada."));
        }

        var reserva = await reservas.ObterPorCodigoComViagemAsync(codigoReserva!, cancellationToken);
        if (reserva is null)
        {
            return Result.Failure(Error.NaoEncontrado("Reserva não encontrada."));
        }

        if (!reserva.EstaConfirmada)
        {
            return Result.Failure(Error.ReservaJaCancelada("A reserva já está cancelada."));
        }

        var dataHoraPartida = reserva.Viagem!.DataHoraPartida;
        if (!reserva.PodeSerCancelada(dataHoraPartida, clock))
        {
            return Result.Failure(Error.ForaDoPrazoDeCancelamento(
                "Cancelamento permitido apenas até 2 horas antes da partida."));
        }

        reserva.Cancelar(dataHoraPartida, clock);
        await unitOfWork.SalvarAlteracoesAsync(cancellationToken);

        return Result.Success();
    }
}
