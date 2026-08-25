using FxRateService.Core.Money;

namespace FxRateService.UnitTests.Money;

public class RoundingTests
{
    [Fact]
    public void ToScale_zaokruzuje_na_dve_decimale()
    {
        var result = Rounding.ToScale(2.345m, 2, MidpointRounding.AwayFromZero);

        Assert.Equal(2.35m, result);
    }
}