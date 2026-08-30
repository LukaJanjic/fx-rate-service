using FxRateService.Api.Contracts;
using FxRateService.Core.Abstractions;

namespace FxRateService.Api.Endpoints;

public static class RatesEndpoints
{
    private const string DefaultSource = "ECB";

    public static void MapRatesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/rates", GetLatestAsync);
    }

    private static async Task<IResult> GetLatestAsync(
        IRateCache cache,
        IRateRepository repository,
        CancellationToken cancellationToken,
        string? currency = null)
    {
        var snapshot = await cache.GetAsync(DefaultSource, cancellationToken);

        if (snapshot is null)
        {
            snapshot = await repository.GetLatestAsync(DefaultSource, cancellationToken);

            if (snapshot is null)
            {
                return Results.Problem(
                    detail: "Kursevi jos nisu ucitani.",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            await cache.SetAsync(DefaultSource, snapshot, cancellationToken);
        }

        var rates = snapshot.Rates
            .Where(r => currency is null
                        || r.Quote.Value.Equals(currency, StringComparison.OrdinalIgnoreCase))
            .Select(r => new RateResponse(r.Base.Value, r.Quote.Value, r.Value))
            .ToList();

        return Results.Ok(new RatesResponse(snapshot.AsOf, DefaultSource, rates));
    }
}