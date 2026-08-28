namespace FxRateService.Infrastructure.Persistence;

/// <summary>
/// Red u tabeli exchange_rates. Namerno NIJE domenski tip —
/// baza cuva primitivne vrednosti, domen ih pretvara u ExchangeRate.
/// </summary>
public sealed class ExchangeRateRecord
{
    public long Id { get; set; }
    public DateOnly AsOf { get; set; }
    public string BaseCurrency { get; set; } = string.Empty;
    public string QuoteCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTimeOffset RetrievedAt { get; set; }
}