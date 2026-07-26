using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Application.Common;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Application.Features.Reservas;

/// <summary>Consulta uma reserva pelo código (GET /reservas/{codigo}).</summary>
public sealed class ConsultarReservaHandler(IReservaRepository reservas)
{
    public async Task<Result<ReservaDto>> ExecutarAsync(string codigo, CancellationToken cancellationToken)
    {
        if (!CodigoReserva.TryParse(codigo, out var codigoReserva))
        {
            return Error.NaoEncontrado("Reserva não encontrada.");
        }

        var reserva = await reservas.ObterPorCodigoComViagemAsync(codigoReserva!, cancellationToken);
        if (reserva is null)
        {
            return Error.NaoEncontrado("Reserva não encontrada.");
        }

        return Result.Success(ReservaMapper.ParaDto(reserva));
    }
}
