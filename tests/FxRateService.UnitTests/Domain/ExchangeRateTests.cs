using FxRateService.Core.Domain;

namespace FxRateService.UnitTests.Domain;

public class ExchangeRateTests
{
    [Fact]
    public void Of_pravi_kurs_sa_parom_valuta()
    {
        var rate = ExchangeRate.Of("EUR", "RSD", 117.2050m);

        Assert.Equal("EUR", rate.Base.Value);
        Assert.Equal("RSD", rate.Quote.Value);
        Assert.Equal(117.2050m, rate.Value);
    }

    [Fact]
    public void Of_odbija_nulu()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ExchangeRate.Of("EUR", "RSD", 0m));
    }

    [Fact]
    public void Of_odbija_isti_par_valuta()
    {
        Assert.Throws<ArgumentException>(
            () => ExchangeRate.Of("EUR", "EUR", 1m));
    }
}