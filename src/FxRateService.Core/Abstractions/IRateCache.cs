using FxRateService.Core.Domain;

namespace FxRateService.Core.Abstractions;

/// <summary>
/// Privremena kopija poslednjih kurseva. Mora biti obrisiva bez posledica —
/// izvor istine je uvek baza.
/// </summary>
public interface IRateCache
{
    Task<RateSnapshot?> GetAsync(string source, CancellationToken cancellationToken);

    Task SetAsync(string source, RateSnapshot snapshot, CancellationToken cancellationToken);

    /// <summary>Brise unos posle uspesnog refresh-a, da novi kursevi budu odmah vidljivi.</summary>
    Task InvalidateAsync(string source, CancellationToken cancellationToken);
}