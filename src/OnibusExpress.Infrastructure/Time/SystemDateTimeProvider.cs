using OnibusExpress.Domain.Abstractions;

namespace OnibusExpress.Infrastructure.Time;

/// <summary>Único ponto do sistema que lê o relógio real.</summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
