using FxRateService.Core.Abstractions;
using FxRateService.Core.Domain;

namespace FxRateService.Infrastructure.Providers.Ecb;

/// <summary>
/// Povlaci dnevne referentne kurseve sa ECB-a.
/// Otpornost (retry, timeout, circuit breaker) je na HttpClient-u,
/// ne u ovoj klasi.
/// </summary>
public sealed class EcbRateProvider : IRateProvider
{
    internal const string DailyFeedUrl =
        "https://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml";

    private readonly HttpClient _http;

    public EcbRateProvider(HttpClient http) => _http = http;

    public string SourceName => "ECB";

    public async Task<RateSnapshot> GetLatestAsync(CancellationToken cancellationToken)
    {
        using var response = await _http.GetAsync(DailyFeedUrl, cancellationToken);

        response.EnsureSuccessStatusCode();

        var xml = await response.Content.ReadAsStringAsync(cancellationToken);

        return EcbRateParser.Parse(xml);
    }
}