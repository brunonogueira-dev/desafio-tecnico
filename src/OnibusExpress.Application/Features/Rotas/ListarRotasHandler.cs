using OnibusExpress.Application.Abstractions.Persistence;
using OnibusExpress.Application.Common;

namespace OnibusExpress.Application.Features.Rotas;

/// <summary>Lista todas as rotas disponíveis (GET /rotas).</summary>
public sealed class ListarRotasHandler(IRotaRepository rotas)
{
    public async Task<Result<IReadOnlyList<RotaDto>>> ExecutarAsync(CancellationToken cancellationToken)
    {
        var encontradas = await rotas.ListarAsync(cancellationToken);
        var dtos = encontradas
            .Select(r => new RotaDto(r.Id, r.Origem, r.Destino, (int)r.DuracaoEstimada.TotalMinutes))
            .ToList();

        return Result.Success<IReadOnlyList<RotaDto>>(dtos);
    }
}
