namespace FxRateService.Core.Domain;

/// <summary>
/// Jedino mesto u sistemu gde se novac zaokruzuje.
/// Math.Round se ne poziva direktno nigde drugde.
/// </summary>
public static class Rounding
{
    /// <summary>
    /// Politika servisa: pola se zaokruzuje dalje od nule (half-up).
    /// Napomena: Math.Round bez ovog argumenta radi ToEven, sto nije nase pravilo.
    /// </summary>
    public const MidpointRounding DefaultMode = MidpointRounding.AwayFromZero;

    public static decimal ToScale(decimal value, int scale, MidpointRounding mode = DefaultMode) =>
        Math.Round(value, scale, mode);
}