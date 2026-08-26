using FxRateService.Core.Domain;

namespace FxRateService.UnitTests.Domain;

public class RoundingTests
{
    [Fact]
    public void ToScale_zaokruzuje_na_dve_decimale()
    {
        var result = Rounding.ToScale(2.345m, 2, MidpointRounding.AwayFromZero);

        Assert.Equal(2.35m, result);
    }

    [Fact]
    public void ToScale_negativan_iznos_bezi_od_nule()
    {
        var result = Rounding.ToScale(-2.345m, 2, MidpointRounding.AwayFromZero);

        Assert.Equal(-2.35m, result);
    }

    [Fact]
    public void ToScale_bankarsko_zaokruzuje_na_paran()
    {
        var result = Rounding.ToScale(2.345m, 2, MidpointRounding.ToEven);

        Assert.Equal(2.34m, result);
    }

    [Fact]
    public void ToScale_negativna_skala_baca_izuzetak()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Rounding.ToScale(1.23m, -1, MidpointRounding.AwayFromZero));
    }
}