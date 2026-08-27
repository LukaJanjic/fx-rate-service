using FxRateService.Core.Domain;

namespace FxRateService.Core.Abstractions;

/// <summary>
/// Izvor kurseva. Core definise sta mu treba; implementacija zivi u Infrastructure.
/// </summary>
public interface IRateProvider
{
    /// <summary>Ime izvora, za logovanje i za kolonu u bazi.</summary>
    string SourceName { get; }

    /// <summary>Povlaci najnovije objavljene kurseve.</summary>
    Task<RateSnapshot> GetLatestAsync(CancellationToken cancellationToken);
}