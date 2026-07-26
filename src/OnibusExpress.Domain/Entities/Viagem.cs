using OnibusExpress.Domain.Abstractions;
using OnibusExpress.Domain.Common;
using OnibusExpress.Domain.Exceptions;

namespace OnibusExpress.Domain.Entities;

/// <summary>
/// Instância de uma rota em uma data/hora específica, com preço e capacidade.
/// Os assentos livres NÃO são um contador aqui — são derivados das reservas
/// confirmadas, evitando uma classe inteira de bug de consistência.
/// </summary>
public sealed class Viagem : Entity
{
    public Guid RotaId { get; private set; }
    public Rota? Rota { get; private set; }
    public DateTimeOffset DataHoraPartida { get; private set; }
    public decimal PrecoBase { get; private set; }
    public int TotalAssentos { get; private set; }

    private Viagem()
    {
    }

    public Viagem(Guid rotaId, DateTimeOffset dataHoraPartida, decimal precoBase, int totalAssentos)
    {
        if (rotaId == Guid.Empty)
        {
            throw new DomainException("Viagem exige uma rota válida.");
        }

        if (precoBase <= 0)
        {
            throw new DomainException("Preço base deve ser positivo.");
        }

        if (totalAssentos <= 0)
        {
            throw new DomainException("Total de assentos deve ser positivo.");
        }

        RotaId = rotaId;
        DataHoraPartida = dataHoraPartida.ToUniversalTime();
        PrecoBase = precoBase;
        TotalAssentos = totalAssentos;
    }

    /// <summary>True se a partida já ocorreu (inclusive no instante exato da partida).</summary>
    public bool JaPartiu(IDateTimeProvider clock) => clock.UtcNow >= DataHoraPartida;

    /// <summary>True se o número informado está dentro de 1..TotalAssentos.</summary>
    public bool AssentoDentroDoRange(int numeroAssento) =>
        numeroAssento >= 1 && numeroAssento <= TotalAssentos;
}
