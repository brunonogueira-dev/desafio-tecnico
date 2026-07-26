using OnibusExpress.Application.Common;

namespace OnibusExpress.Api.Common;

/// <summary>Traduz o código de erro de negócio da Application para status/título HTTP.</summary>
public static class ErrorHttpMapper
{
    public static int ToStatusCode(ErrorCode code) => code switch
    {
        ErrorCode.Validacao => StatusCodes.Status400BadRequest,
        ErrorCode.NaoEncontrado => StatusCodes.Status404NotFound,
        ErrorCode.AssentoIndisponivel => StatusCodes.Status409Conflict,
        ErrorCode.ViagemJaPartiu => StatusCodes.Status409Conflict,
        ErrorCode.ReservaJaCancelada => StatusCodes.Status409Conflict,
        ErrorCode.ForaDoPrazoDeCancelamento => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    public static string ToTitle(ErrorCode code) => code switch
    {
        ErrorCode.Validacao => "Requisição inválida",
        ErrorCode.NaoEncontrado => "Recurso não encontrado",
        ErrorCode.AssentoIndisponivel => "Assento indisponível",
        ErrorCode.ViagemJaPartiu => "Viagem já partiu",
        ErrorCode.ReservaJaCancelada => "Reserva já cancelada",
        ErrorCode.ForaDoPrazoDeCancelamento => "Fora do prazo de cancelamento",
        _ => "Erro interno"
    };
}
