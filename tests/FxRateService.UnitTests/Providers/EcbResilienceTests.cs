using System.Net;
using FxRateService.Core.Abstractions;
using FxRateService.Infrastructure;
using FxRateService.UnitTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;

namespace FxRateService.UnitTests.Providers;

public class EcbResilienceTests
{
    private static string DailyXml => FixtureLoader.Read("ecb-daily-2026-08-26.xml");

    [Fact]
    public async Task Klijent_ponavlja_zahtev_posle_greske_servera()
    {
        var callCount = 0;

        var stub = new StubHttpMessageHandler(_ =>
            ++callCount < 3
                ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                : new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(DailyXml),
                });

        var provider = BuildProvider(stub);

        var snapshot = await provider.GetLatestAsync(CancellationToken.None);

        Assert.Equal(29, snapshot.Rates.Count);
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task Klijent_ne_ponavlja_zahtev_na_gresci_klijenta()
    {
        var stub = StubHttpMessageHandler.ReturnsStatus(HttpStatusCode.NotFound);
        var provider = BuildProvider(stub);

        await Assert.ThrowsAnyAsync<Exception>(
            () => provider.GetLatestAsync(CancellationToken.None));

        Assert.Equal(1, stub.CallCount);
    }

    private static IRateProvider BuildProvider(StubHttpMessageHandler stub)
    {
        var services = new ServiceCollection();

        services.AddInfrastructure();
        services.ConfigureHttpClientDefaults(b =>
            b.ConfigurePrimaryHttpMessageHandler(() => stub));

        return services.BuildServiceProvider().GetRequiredService<IRateProvider>();
    }
}