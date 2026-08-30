namespace FxRateService.Infrastructure.Caching;

/// <summary>
/// Ravan oblik snapshot-a za JSON. Postoji jer domenski tipovi imaju
/// privatne konstruktore i validaciju koju deserijalizator ne moze da zaobidje.
/// </summary>
internal sealed class RateSnapshotDto
{
    public DateOnly AsOf { get; set; }

    public List<RateDto> Rates { get; set; } = [];
}

internal sealed class RateDto
{
    public string Base { get; set; } = string.Empty;

    public string Quote { get; set; } = string.Empty;

    public decimal Value { get; set; }
}