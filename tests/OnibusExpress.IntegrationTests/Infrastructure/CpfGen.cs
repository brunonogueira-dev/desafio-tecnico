namespace OnibusExpress.IntegrationTests.Infrastructure;

/// <summary>Gera CPFs válidos e distintos por índice, para os testes.</summary>
public static class CpfGen
{
    public static string Valido(int seed)
    {
        var baseNum = 100000000 + (seed * 7919) % 800000000;
        var digitos = new int[11];
        var texto = baseNum.ToString("D9");
        for (var i = 0; i < 9; i++)
        {
            digitos[i] = texto[i] - '0';
        }

        digitos[9] = Dv(digitos, 9);
        digitos[10] = Dv(digitos, 10);
        return string.Concat(digitos);
    }

    private static int Dv(int[] digitos, int quantidade)
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
