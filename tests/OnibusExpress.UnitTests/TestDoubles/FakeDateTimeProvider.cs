using OnibusExpress.Domain.Abstractions;

namespace OnibusExpress.UnitTests.TestDoubles;

/// <summary>Relógio de teste com hora fixa e ajustável.</summary>
public sealed class FakeDateTimeProvider : IDateTimeProvider
{
    public FakeDateTimeProvider(DateTimeOffset now) => UtcNow = now;

    public DateTimeOffset UtcNow { get; set; }
}
