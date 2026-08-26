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

        var rate = FindRate(amount.Currency, target)
            ?? throw new InvalidOperationException(
                $"Nema kursa za par {amount.Currency.Value}/{target.Value}.");

        return new Money(amount.Amount * rate.Value, target).Round();
    }
    private ExchangeRate? FindRate(CurrencyCode from, CurrencyCode to) =>
     FindDirect(from, to) ?? FindInverted(from, to) ?? FindCross(from, to);

    private ExchangeRate? FindCross(CurrencyCode from, CurrencyCode to)
    {
        foreach (var fromRate in _rates)
        {
            if (fromRate.Quote != from)
            {
                continue;
            }

            foreach (var toRate in _rates)
            {
                if (toRate.Quote == to && toRate.Base == fromRate.Base)
                {
                    return ExchangeRate.Cross(fromRate, toRate);
                }
            }
        }

        return null;
    }

    private ExchangeRate? FindInverted(CurrencyCode from, CurrencyCode to)
    {
        var direct = FindDirect(to, from);

        return direct?.Invert();
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