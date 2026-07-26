namespace OnibusExpress.Domain.Exceptions;

/// <summary>
/// Sinaliza violação de invariante de domínio. Não deve ser usada como fluxo
/// de controle de regra de negócio — para isso a camada Application usa Result.
/// Serve de rede de segurança para estados impossíveis.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
