namespace FxRateService.Api.Contracts;

public sealed record HistoryResponse(
    string Base,
    string Quote,
    string Source,
    IReadOnlyList<HistoryPoint> Points);

public sealed record HistoryPoint(DateOnly AsOf, decimal Rate);