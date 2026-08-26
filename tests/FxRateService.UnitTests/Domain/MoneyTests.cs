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
}