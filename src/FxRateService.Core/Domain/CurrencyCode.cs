using System.Diagnostics.CodeAnalysis;

namespace FxRateService.Core.Domain;

public readonly record struct CurrencyCode
{
    private readonly string? _value;

    private CurrencyCode(string value) => _value = value;

    public string Value =>
        _value ?? throw new InvalidOperationException(
            "CurrencyCode nije inicijalizovan. Koristi CurrencyCode.Parse.");

    public static CurrencyCode Parse(string code) =>
    TryParse(code, out var currency)
        ? currency
        : throw new ArgumentException(
            $"'{code}' nije validan ISO 4217 kod valute — ocekujem tri slova.",
            nameof(code));
    public static bool TryParse([NotNullWhen(true)] string? code, out CurrencyCode currency)
    {
        if (code is not { Length: 3 } || !code.All(char.IsAsciiLetter))
        {
            currency = default;
            return false;
        }

        currency = new CurrencyCode(code.ToUpperInvariant());
        return true;
    }
}