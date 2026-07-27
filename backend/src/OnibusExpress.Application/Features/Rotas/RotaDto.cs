namespace OnibusExpress.Application.Features.Rotas;

public sealed record RotaDto(
    Guid Id,
    string Origem,
    string Destino,
    int DuracaoMinutos);
