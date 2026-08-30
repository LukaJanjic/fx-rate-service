namespace FxRateService.Api.Contracts;

public sealed record ConvertRequest(decimal Amount, string From, string To);

public sealed record ConvertResponse(
    decimal Amount,
    string From,
    decimal Result,
    string To,
    DateOnly AsOf);