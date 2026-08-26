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

    [Fact]
    public void Convert_koristi_obrnut_kurs()
    {
        var eurRsd = ExchangeRate.Of("EUR", "RSD", 120m);
        var converter = new CurrencyConverter([eurRsd]);

        var result = converter.Convert(Money.Of(1200m, "RSD"), CurrencyCode.Parse("EUR"));

        Assert.Equal(10m, result.Amount);
        Assert.Equal("EUR", result.Currency.Value);
    }
        [Fact]
    public void Convert_koristi_cross_rate_preko_evra()
    {
        var eurUsd = ExchangeRate.Of("EUR", "USD", 2m);
        var eurRsd = ExchangeRate.Of("EUR", "RSD", 120m);
        var converter = new CurrencyConverter([eurUsd, eurRsd]);

        var result = converter.Convert(Money.Of(10m, "USD"), CurrencyCode.Parse("RSD"));

        Assert.Equal(600m, result.Amount);
        Assert.Equal("RSD", result.Currency.Value);
    }
}