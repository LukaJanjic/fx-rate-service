namespace FxRateService.Core.Domain;

/// <summary>
/// Kurs jednog para valuta: 1 jedinica Base = Value jedinica Quote.
/// Primer: EUR/RSD = 117.2050 znaci 1 EUR = 117.2050 RSD.
/// </summary>
public readonly record struct ExchangeRate
{
    private ExchangeRate(CurrencyCode baseCurrency, CurrencyCode quote, decimal value)
    {
        Base = baseCurrency;
        Quote = quote;
        Value = value;
    }

    public CurrencyCode Base { get; }
    public CurrencyCode Quote { get; }
    public decimal Value { get; }

    public static ExchangeRate Of(string baseCurrency, string quote, decimal value) =>
        Of(CurrencyCode.Parse(baseCurrency), CurrencyCode.Parse(quote), value);

    public static ExchangeRate Of(CurrencyCode baseCurrency, CurrencyCode quote, decimal value)
    {
        if (value <= 0m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value), value, "Kurs mora biti veci od nule.");
        }

        if (baseCurrency == quote)
        {
            throw new ArgumentException(
                $"Kurs valute prema samoj sebi nema smisla: {baseCurrency.Value}/{quote.Value}.",
                nameof(quote));
        }

        return new ExchangeRate(baseCurrency, quote, value);
    }
}