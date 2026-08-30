namespace FxRateService.Api.Contracts;

public sealed record RatesResponse(
    DateOnly AsOf,
    string Source,
    IReadOnlyList<RateResponse> Rates);

public sealed record RateResponse(
    string Base,
    string Quote,
    decimal Rate);