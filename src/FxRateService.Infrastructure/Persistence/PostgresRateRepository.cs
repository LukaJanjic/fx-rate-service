using FxRateService.Core.Abstractions;
using FxRateService.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace FxRateService.Infrastructure.Persistence;

/// <summary>
/// Jedino mesto u sistemu koje prevodi izmedju domenskih tipova
/// i redova u tabeli.
/// </summary>
public sealed class PostgresRateRepository : IRateRepository
{
    private readonly FxDbContext _db;
    private readonly IClock _clock;

    public PostgresRateRepository(FxDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task SaveAsync(
        RateSnapshot snapshot, string source, CancellationToken cancellationToken)
    {
        var retrievedAt = _clock.UtcNow;

        var existing = await _db.ExchangeRates
            .Where(r => r.AsOf == snapshot.AsOf && r.Source == source)
            .ToDictionaryAsync(
                r => (r.BaseCurrency, r.QuoteCurrency),
                cancellationToken);

        foreach (var rate in snapshot.Rates)
        {
            var key = (rate.Base.Value, rate.Quote.Value);

            if (existing.TryGetValue(key, out var record))
            {
                record.Rate = rate.Value;
                record.RetrievedAt = retrievedAt;
            }
            else
            {
                _db.ExchangeRates.Add(new ExchangeRateRecord
                {
                    AsOf = snapshot.AsOf,
                    BaseCurrency = rate.Base.Value,
                    QuoteCurrency = rate.Quote.Value,
                    Rate = rate.Value,
                    Source = source,
                    RetrievedAt = retrievedAt,
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<RateSnapshot?> GetLatestAsync(
    string source, CancellationToken cancellationToken)
    {
        var latestDate = await _db.ExchangeRates
            .Where(r => r.Source == source)
            .MaxAsync(r => (DateOnly?)r.AsOf, cancellationToken);

        if (latestDate is null)
        {
            return null;
        }

        var records = await _db.ExchangeRates
            .Where(r => r.Source == source && r.AsOf == latestDate)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var rates = records
            .Select(r => ExchangeRate.Of(r.BaseCurrency.Trim(), r.QuoteCurrency.Trim(), r.Rate))
            .ToList();

        return RateSnapshot.Of(latestDate.Value, rates);
    }

    public async Task<IReadOnlyList<(DateOnly AsOf, ExchangeRate Rate)>> GetHistoryAsync(
    CurrencyCode baseCurrency,
    CurrencyCode quote,
    DateOnly from,
    DateOnly to,
    string source,
    CancellationToken cancellationToken)
    {
        var baseValue = baseCurrency.Value;
        var quoteValue = quote.Value;

        var records = await _db.ExchangeRates
            .Where(r => r.Source == source
                        && r.BaseCurrency == baseValue
                        && r.QuoteCurrency == quoteValue
                        && r.AsOf >= from
                        && r.AsOf <= to)
            .OrderBy(r => r.AsOf)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return records
            .Select(r => (
                r.AsOf,
                ExchangeRate.Of(r.BaseCurrency.Trim(), r.QuoteCurrency.Trim(), r.Rate)))
            .ToList();
    }
}