namespace FxRateService.Core.Domain;

/// <summary>
/// Iznos vezan za valutu. Aritmetika preko razlicitih valuta nije dozvoljena.
/// </summary>
public readonly record struct Money(decimal Amount, CurrencyCode Currency)
{
    public static Money Of(decimal amount, string currency) =>
        new(amount, CurrencyCode.Parse(currency));

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return this with { Amount = Amount + other.Amount };
    }

    private void EnsureSameCurrency(Money other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException(
                $"Operacija nad razlicitim valutama: {Currency.Value} i {other.Currency.Value}. " +
                "Konverzija mora biti eksplicitna.");
        }
    }
}