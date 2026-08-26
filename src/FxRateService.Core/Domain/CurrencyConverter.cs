namespace FxRateService.Core.Domain;

/// <summary>
/// Konvertuje iznos iz jedne valute u drugu koristeci zadati skup kurseva.
/// Ne zna za bazu, mrezu ni konfiguraciju — dobija kurseve i racuna.
/// </summary>
public sealed class CurrencyConverter
{
    private readonly IReadOnlyList<ExchangeRate> _rates;

    public CurrencyConverter(IReadOnlyList<ExchangeRate> rates) => _rates = rates;

    public Money Convert(Money amount, CurrencyCode target)
    {
        if (amount.Currency == target)
        {
            return amount;
        }

        var rate = FindDirect(amount.Currency, target)
            ?? throw new InvalidOperationException(
                $"Nema kursa za par {amount.Currency.Value}/{target.Value}.");

        return new Money(amount.Amount * rate.Value, target);
    }

    private ExchangeRate? FindDirect(CurrencyCode from, CurrencyCode to)
    {
        foreach (var rate in _rates)
        {
            if (rate.Base == from && rate.Quote == to)
            {
                return rate;
            }
        }

        return null;
    }
}