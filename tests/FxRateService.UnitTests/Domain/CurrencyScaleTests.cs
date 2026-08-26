using FxRateService.Core.Domain;

namespace FxRateService.UnitTests.Domain;

public class CurrencyScaleTests
{
    [Theory]
    [InlineData("EUR", 2)]
    [InlineData("RSD", 2)]
    [InlineData("USD", 2)]
    public void For_vraca_dve_decimale_za_uobicajene_valute(string code, int expected)
    {
        Assert.Equal(expected, CurrencyScale.For(CurrencyCode.Parse(code)));
    }

    [Theory]
    [InlineData("JPY", 0)]
    [InlineData("ISK", 0)]
    public void For_vraca_nula_decimala_za_valute_bez_podjedinice(string code, int expected)
    {
        Assert.Equal(expected, CurrencyScale.For(CurrencyCode.Parse(code)));
    }

    [Fact]
    public void For_vraca_tri_decimale_za_KWD()
    {
        Assert.Equal(3, CurrencyScale.For(CurrencyCode.Parse("KWD")));
    }
}