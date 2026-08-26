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

    [Fact]
    public void Invert_okrece_par_valuta()
    {
        var rate = ExchangeRate.Of("EUR", "USD", 1.0850m);

        var inverted = rate.Invert();

        Assert.Equal("USD", inverted.Base.Value);
        Assert.Equal("EUR", inverted.Quote.Value);
    }

    [Fact]
    public void Invert_racuna_reciprocnu_vrednost()
    {
        var rate = ExchangeRate.Of("EUR", "USD", 2m);

        Assert.Equal(0.5m, rate.Invert().Value);
    }

    [Fact]
    public void Invert_dva_puta_vraca_priblizno_original()
    {
        var rate = ExchangeRate.Of("EUR", "USD", 1.0850m);

        var round_trip = rate.Invert().Invert();

        Assert.Equal(1.0850m, round_trip.Value, precision: 10);
    }

    [Fact]
    public void Cross_racuna_kurs_preko_zajednicke_bazne_valute()
    {
        var eurUsd = ExchangeRate.Of("EUR", "USD", 2m);
        var eurRsd = ExchangeRate.Of("EUR", "RSD", 120m);

        var usdRsd = ExchangeRate.Cross(eurUsd, eurRsd);

        Assert.Equal("USD", usdRsd.Base.Value);
        Assert.Equal("RSD", usdRsd.Quote.Value);
        Assert.Equal(60m, usdRsd.Value);
    }

    [Fact]
    public void Cross_odbija_kurseve_sa_razlicitom_baznom_valutom()
    {
        var eurUsd = ExchangeRate.Of("EUR", "USD", 1.0850m);
        var gbpRsd = ExchangeRate.Of("GBP", "RSD", 137m);

        Assert.Throws<InvalidOperationException>(() => ExchangeRate.Cross(eurUsd, gbpRsd));
    }
}