namespace FxRateService.Core.Money;

public static class Rounding
{
    public static decimal ToScale(decimal value, int scale, MidpointRounding mode)
    {
        return Math.Round(value, scale, mode);
    }
}