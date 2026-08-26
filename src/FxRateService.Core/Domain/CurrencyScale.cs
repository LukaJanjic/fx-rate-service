using System.Collections.Frozen;

namespace FxRateService.Core.Domain;

/// <summary>
/// Broj decimala u kojem se valuta iskazuje (ISO 4217 minor unit).
/// Vecina valuta ima 2; izuzetke drzimo eksplicitno.
/// </summary>
public static class CurrencyScale
{
    public const int Default = 2;

    private static readonly FrozenDictionary<string, int> Exceptions =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            ["JPY"] = 0,
            ["KRW"] = 0,
            ["ISK"] = 0,
            ["CLP"] = 0,
            ["VND"] = 0,
            ["BHD"] = 3,
            ["KWD"] = 3,
            ["JOD"] = 3,
            ["TND"] = 3,
            ["OMR"] = 3,
        }.ToFrozenDictionary(StringComparer.Ordinal);

    public static int For(CurrencyCode currency) =>
        Exceptions.TryGetValue(currency.Value, out var scale) ? scale : Default;
}