using System.Reflection;
using OnibusExpress.Domain.Entities;

namespace OnibusExpress.UnitTests.Fakes;

/// <summary>Armazenamento em memória compartilhado pelos repositórios fake.</summary>
public sealed class FakeDatabase
{
    public List<Rota> Rotas { get; } = new();
    public List<Viagem> Viagens { get; } = new();
    public List<Passageiro> Passageiros { get; } = new();
    public List<Reserva> Reservas { get; } = new();
}

/// <summary>
/// Hidrata propriedades de navegação (setter privado) via reflexão, imitando
/// o que o EF Core faz ao materializar. Usado só na montagem dos cenários.
/// </summary>
internal static class TestNav
{
    public static Viagem ComRota(this Viagem viagem, Rota rota)
    {
        Set(viagem, nameof(Viagem.Rota), rota);
        return viagem;
    }

    public static Reserva ComViagem(this Reserva reserva, Viagem viagem)
    {
        Set(reserva, nameof(Reserva.Viagem), viagem);
        return reserva;
    }

    public static Reserva ComPassageiro(this Reserva reserva, Passageiro passageiro)
    {
        Set(reserva, nameof(Reserva.Passageiro), passageiro);
        return reserva;
    }

    private static void Set(object target, string property, object value) =>
        target.GetType()
            .GetProperty(property, BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(target, value);
}
