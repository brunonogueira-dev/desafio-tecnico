using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Infrastructure.Persistence.Converters;

/// <summary>Converte o VO Cpf para varchar(11) (só dígitos) e de volta.</summary>
public sealed class CpfConverter : ValueConverter<Cpf, string>
{
    public CpfConverter()
        : base(cpf => cpf.Valor, valor => Cpf.Criar(valor))
    {
    }
}
