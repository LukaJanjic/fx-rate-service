using FxRateService.Core.Domain;

namespace FxRateService.Core.Abstractions;

/// <summary>
/// Trajno skladiste kurseva. Core definise ugovor; Postgres implementacija
/// zivi u Infrastructure.
/// </summary>
public interface IRateRepository
{
    /// <summary>
    /// Upisuje snapshot. Ako kurs za isti (datum, par, izvor) vec postoji,
    /// azurira ga umesto da napravi duplikat.
    /// </summary>
    Task SaveAsync(RateSnapshot snapshot, string source, CancellationToken cancellationToken);

    /// <summary>Najnoviji kursevi iz datog izvora.</summary>
    Task<RateSnapshot?> GetLatestAsync(string source, CancellationToken cancellationToken);

    /// <summary>Istorija jednog para za period, poredjana po datumu rastuce.</summary>
    Task<IReadOnlyList<(DateOnly AsOf, ExchangeRate Rate)>> GetHistoryAsync(
        CurrencyCode baseCurrency,
        CurrencyCode quote,
        DateOnly from,
        DateOnly to,
        string source,
        CancellationToken cancellationToken);
}