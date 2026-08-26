using FxRateService.Core.Domain;

namespace FxRateService.UnitTests.Domain;

public class CurrencyCodeTests
{
    [Fact]
    public void Parse_normalizuje_na_velika_slova()
    {
        var currency = CurrencyCode.Parse("eur");

        Assert.Equal("EUR", currency.Value);
    }

    [Fact]
    public void Parse_odbija_kod_pogresne_duzine()
    {
        Assert.Throws<ArgumentException>(() => CurrencyCode.Parse("EURO"));
    }
    [Fact]
    public void Parse_odbija_kod_koji_nije_od_slova()
    {
        Assert.Throws<ArgumentException>(() => CurrencyCode.Parse("E1R"));
    }

    [Fact]
    public void Parse_odbija_null()
    {
        Assert.Throws<ArgumentException>(() => CurrencyCode.Parse(null!));
    }

    [Fact]
    public void Dve_iste_valute_su_jednake()
    {
        Assert.Equal(CurrencyCode.Parse("eur"), CurrencyCode.Parse("EUR"));
    }

    [Fact]
    public void Neinicijalizovan_kod_puca_umesto_da_vrati_null()
    {
        var uninitialized = default(CurrencyCode);

        Assert.Throws<InvalidOperationException>(() => uninitialized.Value);
    }
}