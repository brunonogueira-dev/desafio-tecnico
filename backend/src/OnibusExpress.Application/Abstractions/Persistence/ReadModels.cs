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
