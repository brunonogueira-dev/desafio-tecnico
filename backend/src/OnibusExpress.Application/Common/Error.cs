namespace OnibusExpress.Application.Common;

/// <summary>
/// Erro de negócio: código tipado, mensagem amigável e, quando for validação,
/// o dicionário campo -> mensagens que a Api expõe em ProblemDetails.errors.
/// </summary>
public sealed record Error(
    ErrorCode Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Errors = null)
{
    public static Error Validacao(string message, IReadOnlyDictionary<string, string[]>? errors = null) =>
        new(ErrorCode.Validacao, message, errors);

    public static Error Validacao(string campo, string mensagem) =>
        new(ErrorCode.Validacao, mensagem, new Dictionary<string, string[]> { [campo] = new[] { mensagem } });

    public static Error NaoEncontrado(string message) =>
        new(ErrorCode.NaoEncontrado, message);

    public static Error AssentoIndisponivel(string message) =>
        new(ErrorCode.AssentoIndisponivel, message);

    public static Error ViagemJaPartiu(string message) =>
        new(ErrorCode.ViagemJaPartiu, message);

    public static Error ReservaJaCancelada(string message) =>
        new(ErrorCode.ReservaJaCancelada, message);

    public static Error ForaDoPrazoDeCancelamento(string message) =>
        new(ErrorCode.ForaDoPrazoDeCancelamento, message);
}
