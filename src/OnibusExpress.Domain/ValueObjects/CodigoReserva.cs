using System.Security.Cryptography;
using System.Text.RegularExpressions;
using OnibusExpress.Domain.Common;
using OnibusExpress.Domain.Exceptions;

namespace OnibusExpress.Domain.ValueObjects;

/// <summary>
/// Código de reserva legível no formato AAA-99999. Value Object imutável.
/// A geração usa apenas caracteres não-ambíguos (sem I, L, O nas letras e sem
/// 0, 1 nos dígitos) e fonte de aleatoriedade criptográfica.
/// </summary>
public sealed partial class CodigoReserva : ValueObject
{
    // Sem I, L, O (confundem com 1 e 0).
    private const string Letras = "ABCDEFGHJKMNPQRSTUVWXYZ";

    // Sem 0 e 1 (confundem com O e I/L).
    private const string Digitos = "23456789";

    public string Valor { get; }

    private CodigoReserva(string valor) => Valor = valor;

    /// <summary>Gera um novo código aleatório no formato AAA-99999.</summary>
    public static CodigoReserva Gerar()
    {
        Span<char> buffer = stackalloc char[9];
        for (var i = 0; i < 3; i++)
        {
            buffer[i] = Letras[RandomNumberGenerator.GetInt32(Letras.Length)];
        }

        buffer[3] = '-';
        for (var i = 4; i < 9; i++)
        {
            buffer[i] = Digitos[RandomNumberGenerator.GetInt32(Digitos.Length)];
        }

        return new CodigoReserva(new string(buffer));
    }

    /// <summary>Faz parse de um código informado pelo usuário ou lança exceção.</summary>
    public static CodigoReserva Parse(string? entrada)
    {
        if (!TryParse(entrada, out var codigo))
        {
            throw new CodigoReservaInvalidoException();
        }

        return codigo!;
    }

    /// <summary>
    /// Faz parse tolerante (aceita espaços e minúsculas) validando o formato.
    /// Aceita todo A-Z/0-9 na consulta — a busca no banco decide se existe.
    /// </summary>
    public static bool TryParse(string? entrada, out CodigoReserva? codigo)
    {
        codigo = null;
        if (string.IsNullOrWhiteSpace(entrada))
        {
            return false;
        }

        var normalizado = entrada.Trim().ToUpperInvariant();
        if (!FormatoRegex().IsMatch(normalizado))
        {
            return false;
        }

        codigo = new CodigoReserva(normalizado);
        return true;
    }

    [GeneratedRegex(@"^[A-Z]{3}-[0-9]{5}$")]
    private static partial Regex FormatoRegex();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;
}
