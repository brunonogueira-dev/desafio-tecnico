using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OnibusExpress.Domain.ValueObjects;

namespace OnibusExpress.Infrastructure.Persistence.Converters;

/// <summary>Converte o VO CodigoReserva para varchar(9) e de volta.</summary>
public sealed class CodigoReservaConverter : ValueConverter<CodigoReserva, string>
{
    public CodigoReservaConverter()
        : base(codigo => codigo.Valor, valor => CodigoReserva.Parse(valor))
    {
    }
}
