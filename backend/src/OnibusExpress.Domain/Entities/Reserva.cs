using OnibusExpress.Domain.Abstractions;
using OnibusExpress.Domain.Common;
using OnibusExpress.Domain.Enums;
using OnibusExpress.Domain.Exceptions;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Domain.Entities;

/// <summary>
/// Reserva de um assento em uma viagem. O cancelamento é soft (muda o Status),
/// nunca DELETE físico, para liberar o assento e preservar auditoria.
/// </summary>
public sealed class Reserva : Entity
{
    /// <summary>
    /// Antecedência mínima da partida para permitir cancelamento. Regra de
    /// negócio nomeada — nunca literal solto no código.
    /// </summary>
    public static readonly TimeSpan PrazoMinimoAntecedenciaCancelamento = TimeSpan.FromHours(2);

    public Guid ViagemId { get; private set; }
    public Viagem? Viagem { get; private set; }
    public Guid PassageiroId { get; private set; }
    public Passageiro? Passageiro { get; private set; }
    public int NumeroAssento { get; private set; }
    public StatusReserva Status { get; private set; }
    public CodigoReserva Codigo { get; private set; } = null!;

    private Reserva()
    {
    }

    public Reserva(Guid viagemId, Guid passageiroId, int numeroAssento, CodigoReserva codigo)
    {
        if (viagemId == Guid.Empty)
        {
            throw new DomainException("Reserva exige uma viagem válida.");
        }

        if (passageiroId == Guid.Empty)
        {
            throw new DomainException("Reserva exige um passageiro válido.");
        }

        if (numeroAssento < 1)
        {
            throw new DomainException("Número do assento deve ser positivo.");
        }

        ViagemId = viagemId;
        PassageiroId = passageiroId;
        NumeroAssento = numeroAssento;
        Codigo = codigo ?? throw new DomainException("Código da reserva é obrigatório.");
        Status = StatusReserva.Confirmada;
    }

    public bool EstaConfirmada => Status == StatusReserva.Confirmada;

    /// <summary>
    /// True se a reserva está confirmada e ainda falta pelo menos o prazo
    /// mínimo para a partida. No limite exato de 2h, ainda permite (inclusivo).
    /// </summary>
    public bool PodeSerCancelada(DateTimeOffset dataHoraPartida, IDateTimeProvider clock)
    {
        if (Status != StatusReserva.Confirmada)
        {
            return false;
        }

        var limite = dataHoraPartida - PrazoMinimoAntecedenciaCancelamento;
        return clock.UtcNow <= limite;
    }

    public void Cancelar(DateTimeOffset dataHoraPartida, IDateTimeProvider clock)
    {
        if (Status == StatusReserva.Cancelada)
        {
            throw new DomainException("Reserva já está cancelada.");
        }

        if (!PodeSerCancelada(dataHoraPartida, clock))
        {
            throw new DomainException(
                "Cancelamento permitido apenas até 2 horas antes da partida.");
        }

        Status = StatusReserva.Cancelada;
    }
}
