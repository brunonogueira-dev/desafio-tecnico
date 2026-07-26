namespace OnibusExpress.Application.Common;

/// <summary>
/// Códigos de erro de negócio. A tradução para status HTTP acontece na Api,
/// não aqui — a Application não conhece HTTP.
/// </summary>
public enum ErrorCode
{
    Validacao,
    NaoEncontrado,
    AssentoIndisponivel,
    ViagemJaPartiu,
    ReservaJaCancelada,
    ForaDoPrazoDeCancelamento
}
