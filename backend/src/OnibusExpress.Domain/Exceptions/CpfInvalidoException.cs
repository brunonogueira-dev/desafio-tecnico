namespace OnibusExpress.Domain.Exceptions;

/// <summary>
/// CPF que não passou na validação de formato ou dígito verificador.
/// A mensagem nunca ecoa o valor recebido para não vazar dado sensível em log.
/// </summary>
public sealed class CpfInvalidoException : DomainException
{
    public CpfInvalidoException()
        : base("CPF inválido: formato ou dígito verificador incorreto.")
    {
    }
}
