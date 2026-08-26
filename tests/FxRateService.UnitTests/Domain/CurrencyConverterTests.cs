using FxRateService.Core.Domain;

namespace FxRateService.UnitTests.Domain;

public class CurrencyConverterTests
{
    [Fact]
    public void Convert_direktan_kurs()
    {
        var eurRsd = ExchangeRate.Of("EUR", "RSD", 120m);
        var converter = new CurrencyConverter([eurRsd]);

        var result = converter.Convert(Money.Of(10m, "EUR"), CurrencyCode.Parse("RSD"));

        Assert.Equal(1200m, result.Amount);
        Assert.Equal("RSD", result.Currency.Value);
    }

    [Fact]
    public void Convert_ista_valuta_vraca_isti_iznos()
    {
        var converter = new CurrencyConverter([]);

        var result = converter.Convert(Money.Of(10m, "EUR"), CurrencyCode.Parse("EUR"));

        Assert.Equal(10m, result.Amount);
    }

    [Fact]
    public void Convert_puca_kad_nema_kursa()
    {
        var converter = new CurrencyConverter([]);

        Assert.Throws<InvalidOperationException>(
            () => converter.Convert(Money.Of(10m, "EUR"), CurrencyCode.Parse("RSD")));
    }
}