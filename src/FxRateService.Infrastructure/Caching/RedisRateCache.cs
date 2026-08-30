using System.Text.Json;
using FxRateService.Core.Abstractions;
using FxRateService.Core.Domain;
using StackExchange.Redis;

namespace FxRateService.Infrastructure.Caching;

/// <summary>
/// Cache-aside nad Redisom. TTL je sigurnosna mreza — glavni mehanizam
/// je eksplicitna invalidacija posle refresh-a.
/// </summary>
public sealed class RedisRateCache : IRateCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IConnectionMultiplexer _redis;

    public RedisRateCache(IConnectionMultiplexer redis) => _redis = redis;

    public async Task<RateSnapshot?> GetAsync(string source, CancellationToken cancellationToken)
    {
        var value = await _redis.GetDatabase().StringGetAsync(KeyFor(source));

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        var dto = JsonSerializer.Deserialize<RateSnapshotDto>((string)value!, JsonOptions);

        if (dto is null || dto.Rates.Count == 0)
        {
            return null;
        }

        var rates = dto.Rates
            .Select(r => ExchangeRate.Of(r.Base, r.Quote, r.Value))
            .ToList();

        return RateSnapshot.Of(dto.AsOf, rates);
    }

    public async Task SetAsync(
        string source, RateSnapshot snapshot, CancellationToken cancellationToken)
    {
        var dto = new RateSnapshotDto
        {
            AsOf = snapshot.AsOf,
            Rates = snapshot.Rates
                .Select(r => new RateDto
                {
                    Base = r.Base.Value,
                    Quote = r.Quote.Value,
                    Value = r.Value,
                })
                .ToList(),
        };

        var json = JsonSerializer.Serialize(dto, JsonOptions);

        await _redis.GetDatabase().StringSetAsync(KeyFor(source), json, Ttl);
    }

    public async Task InvalidateAsync(string source, CancellationToken cancellationToken)
    {
        await _redis.GetDatabase().KeyDeleteAsync(KeyFor(source));
    }

    private static string KeyFor(string source) => $"rates:latest:{source}";
}