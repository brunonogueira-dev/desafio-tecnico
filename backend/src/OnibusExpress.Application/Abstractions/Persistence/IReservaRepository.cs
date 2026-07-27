using OnibusExpress.Domain.Entities;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Application.Abstractions.Persistence;

public interface IReservaRepository
{
    /// <summary>Carrega a reserva pelo código, incluindo a viagem (para o cancelamento).</summary>
    Task<Reserva?> ObterPorCodigoComViagemAsync(CodigoReserva codigo, CancellationToken cancellationToken);

    /// <summary>True se já existe reserva confirmada para o assento na viagem.</summary>
    Task<bool> ExisteConfirmadaParaAssentoAsync(Guid viagemId, int numeroAssento, CancellationToken cancellationToken);

    /// <summary>True se o código já está em uso (para o retry de geração).</summary>
    Task<bool> CodigoEmUsoAsync(CodigoReserva codigo, CancellationToken cancellationToken);

    void Adicionar(Reserva reserva);
}
