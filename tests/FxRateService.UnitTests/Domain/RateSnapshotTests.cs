using FxRateService.Core.Domain;

namespace FxRateService.UnitTests.Domain;

public class RateSnapshotTests
{
    [Fact]
    public void Of_cuva_datum_i_kurseve()
    {
        var date = new DateOnly(2026, 8, 26);
        var rates = new[]
        {
            ExchangeRate.Of("EUR", "USD", 1.1669m),
            ExchangeRate.Of("EUR", "JPY", 185.62m),
        };

        var snapshot = RateSnapshot.Of(date, rates);

        Assert.Equal(date, snapshot.AsOf);
        Assert.Equal(2, snapshot.Rates.Count);
    }

    [Fact]
    public void Of_odbija_prazan_skup()
    {
        Assert.Throws<ArgumentException>(
            () => RateSnapshot.Of(new DateOnly(2026, 8, 26), []));
    }

    [Fact]
    public void Of_odbija_duplirane_parove()
    {
        var rates = new[]
        {
            ExchangeRate.Of("EUR", "USD", 1.1669m),
            ExchangeRate.Of("EUR", "USD", 1.1670m),
        };

        Assert.Throws<ArgumentException>(
            () => RateSnapshot.Of(new DateOnly(2026, 8, 26), rates));
    }
}