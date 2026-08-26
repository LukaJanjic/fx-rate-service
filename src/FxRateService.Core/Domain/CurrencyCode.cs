namespace FxRateService.Core.Domain;

public readonly record struct CurrencyCode
{
    private readonly string? _value;

    private CurrencyCode(string value) => _value = value;

    public string Value =>
        _value ?? throw new InvalidOperationException(
            "CurrencyCode nije inicijalizovan. Koristi CurrencyCode.Parse.");

    public static CurrencyCode Parse(string code)
    {
        if (code is not { Length: 3 } || !code.All(char.IsAsciiLetter))
        {
            throw new ArgumentException(
                $"'{code}' nije validan ISO 4217 kod valute — ocekujem tri slova.",
                nameof(code));
        }

        return new CurrencyCode(code.ToUpperInvariant());
    }
}