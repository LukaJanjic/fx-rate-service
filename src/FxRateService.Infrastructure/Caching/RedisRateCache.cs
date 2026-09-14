using System.Text.Json;
using FxRateService.Core.Abstractions;
using FxRateService.Core.Domain;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace FxRateService.Infrastructure.Caching;

/// <summary>
/// Cache-aside nad Redisom. TTL je sigurnosna mreza — glavni mehanizam
/// je eksplicitna invalidacija posle refresh-a.
/// Pad Redisa NE sme da obori zahtev: izvor istine je baza.
/// </summary>
public sealed class RedisRateCache : IRateCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisRateCache> _logger;

    public RedisRateCache(IConnectionMultiplexer redis, ILogger<RedisRateCache> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<RateSnapshot?> GetAsync(string source, CancellationToken cancellationToken)
    {
        RedisValue value;

        try
        {
            value = await _redis.GetDatabase().StringGetAsync(KeyFor(source));
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis nedostupan pri citanju — nastavljam sa bazom.");
            return null;
        }

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

        try
        {
            await _redis.GetDatabase().StringSetAsync(KeyFor(source), json, Ttl);
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis nedostupan pri upisu — preskacem cache.");
        }
    }

    public async Task InvalidateAsync(string source, CancellationToken cancellationToken)
    {
        try
        {
            await _redis.GetDatabase().KeyDeleteAsync(KeyFor(source));
        }
        catch (RedisConnectionException ex)
        {
            _logger.LogWarning(ex, "Redis nedostupan pri invalidaciji — unos ce isteci po TTL-u.");
        }
    }

    private static string KeyFor(string source) => $"rates:latest:{source}";
}