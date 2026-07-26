using OnibusExpress.Domain.Common;
using OnibusExpress.Domain.Exceptions;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Domain.Entities;

/// <summary>Pessoa que compra a passagem. Identificada de forma única pelo CPF.</summary>
public sealed class Passageiro : Entity
{
    public string Nome { get; private set; } = null!;
    public Cpf Cpf { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public DateOnly DataNascimento { get; private set; }

    private Passageiro()
    {
    }

    public Passageiro(string nome, Cpf cpf, string email, DateOnly dataNascimento)
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new DomainException("Nome do passageiro é obrigatório.");
        }

        if (cpf is null)
        {
            throw new DomainException("CPF do passageiro é obrigatório.");
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            throw new DomainException("E-mail do passageiro é inválido.");
        }

        Nome = nome.Trim();
        Cpf = cpf;
        Email = email.Trim();
        DataNascimento = dataNascimento;
    }

    public void AtualizarDados(string nome, string email)
    {
        if (!string.IsNullOrWhiteSpace(nome))
        {
            Nome = nome.Trim();
        }

        if (!string.IsNullOrWhiteSpace(email) && email.Contains('@'))
        {
            Email = email.Trim();
        }
    }
}
