using FxRateService.Core.Abstractions;
using FxRateService.Core.Domain;
using FxRateService.Infrastructure.Persistence;

namespace FxRateService.IntegrationTests;

public class PostgresRateRepositoryTests : IClassFixture<PostgresFixture>
{
    private static readonly DateTimeOffset FixedNow =
        new(2026, 8, 26, 15, 0, 0, TimeSpan.Zero);

    private readonly PostgresFixture _postgres;

    public PostgresRateRepositoryTests(PostgresFixture postgres) => _postgres = postgres;

    [Fact]
    public async Task SaveAsync_upisuje_kurseve_koji_se_mogu_procitati()
    {
        await using var db = _postgres.CreateDbContext();
        var repository = new PostgresRateRepository(db, new FakeClock(FixedNow));

        var snapshot = RateSnapshot.Of(new DateOnly(2026, 8, 26),
        [
            ExchangeRate.Of("EUR", "USD", 1.1669m),
            ExchangeRate.Of("EUR", "JPY", 185.62m),
        ]);

        await repository.SaveAsync(snapshot, "TEST-BASIC", CancellationToken.None);

        var read = await repository.GetLatestAsync("TEST-BASIC", CancellationToken.None);

        Assert.NotNull(read);
        Assert.Equal(new DateOnly(2026, 8, 26), read.AsOf);
        Assert.Equal(2, read.Rates.Count);
    }

    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }
}