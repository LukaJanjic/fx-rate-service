using System.Net;
using System.Net.Http.Json;
using FxRateService.Api.Contracts;

namespace FxRateService.IntegrationTests;

public class RatesApiTests : IClassFixture<FxApiFactory>
{
    private readonly HttpClient _client;

    public RatesApiTests(FxApiFactory factory) => _client = factory.CreateClient();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Health_vraca_200()
    {
        var response = await _client.GetAsync("/health", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRates_vraca_kurseve_iz_provajdera()
    {
        var response = await _client.GetAsync("/api/rates", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<RatesResponse>(Ct);

        Assert.NotNull(body);
        Assert.Equal(new DateOnly(2026, 8, 26), body.AsOf);
        Assert.Equal(3, body.Rates.Count);
        Assert.Equal(0.85613m, body.Rates.Single(r => r.Quote == "GBP").Rate);
    }

    [Fact]
    public async Task GetRates_filtrira_po_valuti()
    {
        var response = await _client.GetAsync("/api/rates?currency=usd", Ct);

        var body = await response.Content.ReadFromJsonAsync<RatesResponse>(Ct);

        Assert.NotNull(body);
        Assert.Single(body.Rates);
        Assert.Equal("USD", body.Rates[0].Quote);
    }

    [Fact]
    public async Task Convert_racuna_cross_rate()
    {
        var response = await _client.PostAsJsonAsync("/api/convert",
            new ConvertRequest(100m, "USD", "JPY"), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ConvertResponse>(Ct);

        Assert.NotNull(body);
        Assert.Equal(15907m, body.Result);
    }

    [Fact]
    public async Task Convert_vraca_400_na_neispravnu_valutu()
    {
        var response = await _client.PostAsJsonAsync("/api/convert",
            new ConvertRequest(100m, "xy", "JPY"), Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}