namespace OnibusExpress.UnitTests.Common;

/// <summary>
/// Gera CPFs válidos por algoritmo (base aleatória + dígitos verificadores
/// calculados), para não copiar CPFs reais nos testes.
/// </summary>
public static class CpfFactory
{
    public static string GerarValido(Random rng)
    {
        var d = new int[11];
        do
        {
            for (var i = 0; i < 9; i++)
            {
                d[i] = rng.Next(0, 10);
            }
        }
        while (d.Take(9).Distinct().Count() == 1);

        d[9] = CalcularDigito(d, 9);
        d[10] = CalcularDigito(d, 10);
        return string.Concat(d);
    }

    private static int CalcularDigito(int[] digitos, int quantidade)
    {
        var soma = 0;
        var peso = quantidade + 1;
        for (var i = 0; i < quantidade; i++)
        {
            soma += digitos[i] * (peso - i);
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
