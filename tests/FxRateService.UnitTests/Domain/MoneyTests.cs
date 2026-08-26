using FxRateService.Core.Domain;

namespace FxRateService.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void Of_pravi_iznos_sa_valutom()
    {
        var money = Money.Of(10.50m, "EUR");

        Assert.Equal(10.50m, money.Amount);
        Assert.Equal("EUR", money.Currency.Value);
    }

    [Fact]
    public void Sabiranje_istih_valuta_radi()
    {
        var a = Money.Of(10.50m, "EUR");
        var b = Money.Of(0.25m, "EUR");

        Assert.Equal(10.75m, a.Add(b).Amount);
    }

    [Fact]
    public void Sabiranje_razlicitih_valuta_puca()
    {
        var rsd = Money.Of(100m, "RSD");
        var eur = Money.Of(100m, "EUR");

        Assert.Throws<InvalidOperationException>(() => rsd.Add(eur));
    }

        [Fact]
    public void Round_koristi_skalu_valute_za_EUR()
    {
        var money = Money.Of(10.126m, "EUR");

        Assert.Equal(10.13m, money.Round().Amount);
    }

    [Fact]
    public void Round_koristi_nula_decimala_za_JPY()
    {
        var money = Money.Of(15400.50m, "JPY");

        Assert.Equal(15401m, money.Round().Amount);
    }

    [Fact]
    public void Round_ne_menja_valutu()
    {
        var money = Money.Of(10.129m, "EUR");

        Assert.Equal(CurrencyCode.Parse("EUR"), money.Round().Currency);
    }
}