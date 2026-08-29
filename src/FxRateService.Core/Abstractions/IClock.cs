namespace FxRateService.Core.Abstractions;

/// <summary>
/// Izvor trenutnog vremena. Postoji da bi kod koji zavisi od vremena
/// bio deterministicki testabilan.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}