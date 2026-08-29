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
    [Fact]
    public async Task Baza_cuva_kurs_bez_gubitka_decimala()
    {
        await using var db = _postgres.CreateDbContext();
        var repository = new PostgresRateRepository(db, new FakeClock(FixedNow));

        var snapshot = RateSnapshot.Of(new DateOnly(2026, 8, 26),
        [
            ExchangeRate.Of("EUR", "GBP", 0.85613m),
            ExchangeRate.Of("EUR", "IDR", 20668.66m),
        ]);

        await repository.SaveAsync(snapshot, "TEST-PRECISION", CancellationToken.None);

        var read = await repository.GetLatestAsync("TEST-PRECISION", CancellationToken.None);

        var gbp = read!.Rates.Single(r => r.Quote.Value == "GBP");
        var idr = read.Rates.Single(r => r.Quote.Value == "IDR");

        Assert.Equal(0.85613m, gbp.Value);
        Assert.Equal(20668.66m, idr.Value);
    }

    [Fact]
    public async Task SaveAsync_azurira_umesto_da_duplira()
    {
        await using var db = _postgres.CreateDbContext();
        var repository = new PostgresRateRepository(db, new FakeClock(FixedNow));

        var date = new DateOnly(2026, 8, 26);

        await repository.SaveAsync(
            RateSnapshot.Of(date, [ExchangeRate.Of("EUR", "USD", 1.1669m)]),
            "TEST-UPSERT", CancellationToken.None);

        await repository.SaveAsync(
            RateSnapshot.Of(date, [ExchangeRate.Of("EUR", "USD", 1.1700m)]),
            "TEST-UPSERT", CancellationToken.None);

        var read = await repository.GetLatestAsync("TEST-UPSERT", CancellationToken.None);

        Assert.Single(read!.Rates);
        Assert.Equal(1.1700m, read.Rates[0].Value);
    }
    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }
}