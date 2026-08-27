using System.Net;
using FxRateService.Infrastructure.Providers.Ecb;
using FxRateService.UnitTests.Fixtures;

namespace FxRateService.UnitTests.Providers;

public class EcbRateProviderTests
{
    private static string DailyXml => FixtureLoader.Read("ecb-daily-2026-08-26.xml");

    [Fact]
    public async Task GetLatestAsync_vraca_kurseve_sa_feeda()
    {
        var stub = StubHttpMessageHandler.ReturnsOk(DailyXml);
        var provider = new EcbRateProvider(stub.CreateClient());

        var snapshot = await provider.GetLatestAsync(CancellationToken.None);

        Assert.Equal(29, snapshot.Rates.Count);
        Assert.Equal(new DateOnly(2026, 8, 26), snapshot.AsOf);
    }

    [Fact]
    public async Task GetLatestAsync_gadja_ecb_daily_endpoint()
    {
        var stub = StubHttpMessageHandler.ReturnsOk(DailyXml);
        var provider = new EcbRateProvider(stub.CreateClient());

        await provider.GetLatestAsync(CancellationToken.None);

        Assert.Equal(
            "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml",
            stub.LastRequestUri!.ToString());
        Assert.Equal(1, stub.CallCount);
    }

    [Fact]
    public async Task GetLatestAsync_puca_na_gresci_servera()
    {
        var stub = StubHttpMessageHandler.ReturnsStatus(HttpStatusCode.InternalServerError);
        var provider = new EcbRateProvider(stub.CreateClient());

        await Assert.ThrowsAsync<HttpRequestException>(
            () => provider.GetLatestAsync(CancellationToken.None));
    }

    [Fact]
    public void SourceName_je_ECB()
    {
        var provider = new EcbRateProvider(new HttpClient());

        Assert.Equal("ECB", provider.SourceName);
    }
}