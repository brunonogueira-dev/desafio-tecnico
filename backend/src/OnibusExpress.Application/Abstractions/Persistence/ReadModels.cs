namespace OnibusExpress.Application.Abstractions.Persistence;

/// <summary>
/// Projeção de leitura de uma viagem já com a contagem de assentos ocupados,
/// resolvida em um único round-trip pelo repositório (sem N+1).
/// </summary>
public sealed record ViagemComOcupacao(
    Guid Id,
    string Origem,
    string Destino,
    DateTimeOffset DataHoraPartida,
    TimeSpan DuracaoEstimada,
    decimal PrecoBase,
    int TotalAssentos,
    int AssentosOcupados);

/// <summary>Uma página de viagens mais o total de itens que casam com o filtro.</summary>
public sealed record PaginaDeViagens(IReadOnlyList<ViagemComOcupacao> Itens, int Total);
