using FxRateService.Core.Abstractions;
using FxRateService.Core.Domain;
using FxRateService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
    [Fact]
    public async Task GetHistoryAsync_vraca_period_poredjan_po_datumu()
    {
        await using var db = _postgres.CreateDbContext();
        var repository = new PostgresRateRepository(db, new FakeClock(FixedNow));

        const string source = "TEST-HISTORY";

        await repository.SaveAsync(
            RateSnapshot.Of(new DateOnly(2026, 8, 24), [ExchangeRate.Of("EUR", "USD", 1.1600m)]),
            source, CancellationToken.None);

        await repository.SaveAsync(
            RateSnapshot.Of(new DateOnly(2026, 8, 26), [ExchangeRate.Of("EUR", "USD", 1.1669m)]),
            source, CancellationToken.None);

        await repository.SaveAsync(
            RateSnapshot.Of(new DateOnly(2026, 8, 25), [ExchangeRate.Of("EUR", "USD", 1.1650m)]),
            source, CancellationToken.None);

        var history = await repository.GetHistoryAsync(
            CurrencyCode.Parse("EUR"),
            CurrencyCode.Parse("USD"),
            new DateOnly(2026, 8, 24),
            new DateOnly(2026, 8, 26),
            source,
            CancellationToken.None);

        Assert.Equal(3, history.Count);
        Assert.Equal(new DateOnly(2026, 8, 24), history[0].AsOf);
        Assert.Equal(new DateOnly(2026, 8, 25), history[1].AsOf);
        Assert.Equal(new DateOnly(2026, 8, 26), history[2].AsOf);
        Assert.Equal(1.1650m, history[1].Rate.Value);
    }
        [Fact]
    public async Task GetHistoryAsync_ukljucuje_granicne_datume_a_iskljucuje_ostale()
    {
        await using var db = _postgres.CreateDbContext();
        var repository = new PostgresRateRepository(db, new FakeClock(FixedNow));

        const string source = "TEST-RANGE";

        foreach (var day in new[] { 23, 24, 25, 26, 27 })
        {
            await repository.SaveAsync(
                RateSnapshot.Of(new DateOnly(2026, 8, day),
                    [ExchangeRate.Of("EUR", "USD", 1.1600m)]),
                source, CancellationToken.None);
        }

        var history = await repository.GetHistoryAsync(
            CurrencyCode.Parse("EUR"),
            CurrencyCode.Parse("USD"),
            new DateOnly(2026, 8, 24),
            new DateOnly(2026, 8, 26),
            source,
            CancellationToken.None);

        Assert.Equal(3, history.Count);
        Assert.Equal(new DateOnly(2026, 8, 24), history[0].AsOf);
        Assert.Equal(new DateOnly(2026, 8, 26), history[2].AsOf);
    }

    [Fact]
    public async Task Baza_odbija_duplikat_para_za_isti_dan_i_izvor()
    {
        await using var db = _postgres.CreateDbContext();

        const string source = "TEST-UNIQUE";
        var date = new DateOnly(2026, 8, 26);

        db.ExchangeRates.Add(new ExchangeRateRecord
        {
            AsOf = date,
            BaseCurrency = "EUR",
            QuoteCurrency = "USD",
            Rate = 1.1669m,
            Source = source,
            RetrievedAt = FixedNow,
        });

        db.ExchangeRates.Add(new ExchangeRateRecord
        {
            AsOf = date,
            BaseCurrency = "EUR",
            QuoteCurrency = "USD",
            Rate = 1.1700m,
            Source = source,
            RetrievedAt = FixedNow,
        });

        await Assert.ThrowsAsync<DbUpdateException>(
            () => db.SaveChangesAsync(CancellationToken.None));
    }
    private sealed class FakeClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }
}