using FxRateService.Core.Domain;
using FxRateService.Infrastructure.Caching;

namespace FxRateService.IntegrationTests;

public class RedisRateCacheTests : IClassFixture<RedisFixture>
{
    private readonly RedisFixture _redis;

    public RedisRateCacheTests(RedisFixture redis) => _redis = redis;

    [Fact]
    public async Task GetAsync_vraca_null_kad_nema_unosa()
    {
        var cache = new RedisRateCache(_redis.Connection);

        var result = await cache.GetAsync("TEST-EMPTY", CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task SetAsync_pa_GetAsync_vraca_iste_kurseve()
    {
        var cache = new RedisRateCache(_redis.Connection);

        var snapshot = RateSnapshot.Of(new DateOnly(2026, 8, 26),
        [
            ExchangeRate.Of("EUR", "USD", 1.1669m),
            ExchangeRate.Of("EUR", "GBP", 0.85613m),
        ]);

        await cache.SetAsync("TEST-ROUNDTRIP", snapshot, CancellationToken.None);

        var read = await cache.GetAsync("TEST-ROUNDTRIP", CancellationToken.None);

        Assert.NotNull(read);
        Assert.Equal(new DateOnly(2026, 8, 26), read.AsOf);
        Assert.Equal(2, read.Rates.Count);
        Assert.Equal(0.85613m, read.Rates.Single(r => r.Quote.Value == "GBP").Value);
    }

    [Fact]
    public async Task InvalidateAsync_brise_unos()
    {
        var cache = new RedisRateCache(_redis.Connection);

        var snapshot = RateSnapshot.Of(new DateOnly(2026, 8, 26),
            [ExchangeRate.Of("EUR", "USD", 1.1669m)]);

        await cache.SetAsync("TEST-INVALIDATE", snapshot, CancellationToken.None);
        await cache.InvalidateAsync("TEST-INVALIDATE", CancellationToken.None);

        var read = await cache.GetAsync("TEST-INVALIDATE", CancellationToken.None);

        Assert.Null(read);
    }
}