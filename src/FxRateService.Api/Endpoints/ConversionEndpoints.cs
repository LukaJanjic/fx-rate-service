using FxRateService.Api.Contracts;
using FxRateService.Core.Abstractions;
using FxRateService.Core.Domain;

namespace FxRateService.Api.Endpoints;

public static class ConversionEndpoints
{
    private const string DefaultSource = "ECB";

    public static void MapConversionEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/convert", ConvertAsync);
    }

    private static async Task<IResult> ConvertAsync(
        ConvertRequest request,
        IRateCache cache,
        IRateRepository repository,
        CancellationToken cancellationToken)
    {
        if (!CurrencyCode.TryParse(request.From, out var from)
            || !CurrencyCode.TryParse(request.To, out var to))
        {
            return Results.Problem(
                detail: "Kod valute mora biti ISO 4217 alfa-3.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (request.Amount < 0m)
        {
            return Results.Problem(
                detail: "Iznos ne moze biti negativan.",
                statusCode: StatusCodes.Status400BadRequest);
        }

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

        var converter = new CurrencyConverter(snapshot.Rates);

        try
        {
            var result = converter.Convert(new Money(request.Amount, from), to);

            return Results.Ok(new ConvertResponse(
                request.Amount, from.Value, result.Amount, to.Value, snapshot.AsOf));
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
    }
}