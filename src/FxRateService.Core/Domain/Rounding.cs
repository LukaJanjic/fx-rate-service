namespace FxRateService.Core.Domain;

public static class Rounding
{
    public static decimal ToScale(decimal value, int scale, MidpointRounding mode)
    {
        return Math.Round(value, scale, mode);
    }
}