namespace OnibusExpress.Application.Features.Viagens;

/// <summary>Filtros obrigatórios da busca de viagens.</summary>
public sealed record BuscarViagensRequest(string Origem, string Destino, DateOnly Data);

/// <summary>Viagem na listagem de busca, com vagas derivadas.</summary>
public sealed record ViagemResumoDto(
    Guid Id,
    string Origem,
    string Destino,
    DateTimeOffset DataHoraPartida,
    int DuracaoMinutos,
    decimal PrecoBase,
    int TotalAssentos,
    int VagasDisponiveis);

/// <summary>Assento no mapa de assentos.</summary>
public sealed record AssentoDto(int Numero, bool Ocupado);

/// <summary>Detalhe da viagem com o mapa de assentos.</summary>
public sealed record ViagemDetalheDto(
    Guid Id,
    string Origem,
    string Destino,
    DateTimeOffset DataHoraPartida,
    int DuracaoMinutos,
    decimal PrecoBase,
    int TotalAssentos,
    int VagasDisponiveis,
    IReadOnlyList<AssentoDto> Assentos);
