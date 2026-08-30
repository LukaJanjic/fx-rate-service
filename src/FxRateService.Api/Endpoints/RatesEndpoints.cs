using FxRateService.Api.Contracts;
using FxRateService.Core.Abstractions;
using FxRateService.Core.Domain;

namespace FxRateService.Api.Endpoints;

public static class RatesEndpoints
{
    private const string DefaultSource = "ECB";

    public static void MapRatesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/rates", GetLatestAsync);
        app.MapGet("/api/rates/history", GetHistoryAsync);
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
        private static async Task<IResult> GetHistoryAsync(
        IRateRepository repository,
        CancellationToken cancellationToken,
        string baseCurrency,
        string quote,
        DateOnly from,
        DateOnly to)
    {
        if (!CurrencyCode.TryParse(baseCurrency, out var parsedBase)
            || !CurrencyCode.TryParse(quote, out var parsedQuote))
        {
            return Results.Problem(
                detail: "Kod valute mora biti ISO 4217 alfa-3.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (from > to)
        {
            return Results.Problem(
                detail: "Pocetak perioda ne moze biti posle kraja.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var history = await repository.GetHistoryAsync(
            parsedBase, parsedQuote, from, to, DefaultSource, cancellationToken);

        var points = history
            .Select(h => new HistoryPoint(h.AsOf, h.Rate.Value))
            .ToList();

        return Results.Ok(new HistoryResponse(
            parsedBase.Value, parsedQuote.Value, DefaultSource, points));
    }
}