using OnibusExpress.Domain.Common;
using OnibusExpress.Domain.Exceptions;

namespace OnibusExpress.Domain.ValueObjects;

/// <summary>
/// CPF como Value Object imutável. Armazena apenas os 11 dígitos e garante,
/// já na construção, que é impossível existir um CPF inválido em memória.
/// </summary>
public sealed class Cpf : ValueObject
{
    public const int Tamanho = 11;

    /// <summary>Somente dígitos, sem máscara (ex.: "52998224725").</summary>
    public string Valor { get; }

    private Cpf(string valor) => Valor = valor;

    /// <summary>Representação com máscara para leitura (ex.: "529.982.247-25").</summary>
    public string Formatado =>
        $"{Valor[..3]}.{Valor.Substring(3, 3)}.{Valor.Substring(6, 3)}-{Valor.Substring(9, 2)}";

    /// <summary>Cria um CPF válido ou lança <see cref="CpfInvalidoException"/>.</summary>
    public static Cpf Criar(string? entrada)
    {
        if (!TryCriar(entrada, out var cpf))
        {
            throw new CpfInvalidoException();
        }

        return cpf!;
    }

    /// <summary>
    /// Tenta criar um CPF sem lançar exceção — usado pela Application para
    /// converter entrada inválida em erro de negócio (Result), não em exceção.
    /// </summary>
    public static bool TryCriar(string? entrada, out Cpf? cpf)
    {
        cpf = null;
        if (string.IsNullOrWhiteSpace(entrada))
        {
            return false;
        }

        var digitos = SomenteDigitos(entrada);
        if (!EhValido(digitos))
        {
            return false;
        }

        cpf = new Cpf(digitos);
        return true;
    }

    /// <summary>Valida uma sequência de dígitos por tamanho, repetição e ambos os DVs.</summary>
    public static bool EhValido(string digitos)
    {
        if (digitos.Length != Tamanho || !digitos.All(char.IsDigit))
        {
            return false;
        }

        // CPFs com todos os dígitos iguais passam no cálculo de DV, mas são inválidos.
        if (digitos.Distinct().Count() == 1)
        {
            return false;
        }

        var dv1 = CalcularDigito(digitos, 9);
        var dv2 = CalcularDigito(digitos, 10);
        return dv1 == digitos[9] - '0' && dv2 == digitos[10] - '0';
    }

    private static int CalcularDigito(string digitos, int quantidade)
    {
        var soma = 0;
        var peso = quantidade + 1;
        for (var i = 0; i < quantidade; i++)
        {
            soma += (digitos[i] - '0') * (peso - i);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    private static string SomenteDigitos(string entrada) =>
        new(entrada.Where(char.IsDigit).ToArray());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Formatado;
}
