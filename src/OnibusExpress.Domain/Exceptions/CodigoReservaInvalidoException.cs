namespace OnibusExpress.Domain.Exceptions;

/// <summary>
/// Código de reserva que não casa com o padrão AAA-99999.
/// </summary>
public sealed class CodigoReservaInvalidoException : DomainException
{
    public CodigoReservaInvalidoException()
        : base("Código de reserva inválido: esperado o formato AAA-99999.")
    {
    }
}
