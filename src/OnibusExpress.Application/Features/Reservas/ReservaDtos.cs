namespace OnibusExpress.Application.Features.Reservas;

/// <summary>Dados do passageiro no momento da reserva.</summary>
public sealed record PassageiroInput(
    string Nome,
    string Cpf,
    string Email,
    DateOnly DataNascimento);

/// <summary>Requisição de criação de reserva.</summary>
public sealed record CriarReservaRequest(
    Guid ViagemId,
    int NumeroAssento,
    PassageiroInput Passageiro);

public sealed record PassageiroDto(string Nome, string CpfFormatado, string Email);

public sealed record ReservaViagemDto(
    Guid Id,
    string Origem,
    string Destino,
    DateTimeOffset DataHoraPartida,
    int DuracaoMinutos,
    decimal PrecoBase);

/// <summary>Reserva retornada na criação e na consulta.</summary>
public sealed record ReservaDto(
    string Codigo,
    string Status,
    int NumeroAssento,
    ReservaViagemDto Viagem,
    PassageiroDto Passageiro);
