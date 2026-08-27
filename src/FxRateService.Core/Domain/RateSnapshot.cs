namespace FxRateService.Core.Domain;

/// <summary>
/// Kursevi objavljeni na jedan datum. ECB objavljuje sve valute odjednom,
/// pa je ovo prirodna jedinica podatka — ne pojedinacan kurs.
/// </summary>
public sealed class RateSnapshot
{
    private RateSnapshot(DateOnly asOf, IReadOnlyList<ExchangeRate> rates)
    {
        AsOf = asOf;
        Rates = rates;
    }

    public DateOnly AsOf { get; }
    public IReadOnlyList<ExchangeRate> Rates { get; }

    public static RateSnapshot Of(DateOnly asOf, IReadOnlyList<ExchangeRate> rates)
    {
        if (rates.Count == 0)
        {
            throw new ArgumentException("Snapshot mora imati bar jedan kurs.", nameof(rates));
        }

        var seen = new HashSet<(string Base, string Quote)>();

        foreach (var rate in rates)
        {
            if (!seen.Add((rate.Base.Value, rate.Quote.Value)))
            {
                throw new ArgumentException(
                    $"Duplikat kursa za par {rate.Base.Value}/{rate.Quote.Value}.",
                    nameof(rates));
            }
        }

        return new RateSnapshot(asOf, rates);
    }
}