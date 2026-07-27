using OnibusExpress.Domain.Abstractions;

namespace OnibusExpress.IntegrationTests.Infrastructure;

/// <summary>Relógio controlável usado no lugar do SystemDateTimeProvider nos testes.</summary>
public sealed class TestClock : IDateTimeProvider
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
}
