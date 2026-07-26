using OnibusExpress.Domain.Common;
using OnibusExpress.Domain.Exceptions;

namespace OnibusExpress.Domain.Entities;

/// <summary>Trajeto entre duas cidades com duração estimada.</summary>
public sealed class Rota : Entity
{
    public string Origem { get; private set; } = null!;
    public string Destino { get; private set; } = null!;
    public TimeSpan DuracaoEstimada { get; private set; }

    private Rota()
    {
    }

    public Rota(string origem, string destino, TimeSpan duracaoEstimada)
    {
        if (string.IsNullOrWhiteSpace(origem))
        {
            throw new DomainException("Origem da rota é obrigatória.");
        }

        if (string.IsNullOrWhiteSpace(destino))
        {
            throw new DomainException("Destino da rota é obrigatório.");
        }

        if (string.Equals(origem.Trim(), destino.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Origem e destino não podem ser iguais.");
        }

        if (duracaoEstimada <= TimeSpan.Zero)
        {
            throw new DomainException("Duração estimada deve ser positiva.");
        }

        Origem = origem.Trim();
        Destino = destino.Trim();
        DuracaoEstimada = duracaoEstimada;
    }
}
