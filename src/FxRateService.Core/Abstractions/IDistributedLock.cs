namespace FxRateService.Core.Abstractions;

/// <summary>
/// Koordinacija izmedju instanci servisa. Onaj ko dobije lock radi posao,
/// ostali preskacu ciklus.
/// </summary>
public interface IDistributedLock
{
    /// <summary>
    /// Pokusava da zauzme lock. Vraca null ako ga neko drugi vec drzi.
    /// Rezultat se MORA osloboditi (using), inace lock ostaje do isteka TTL-a.
    /// </summary>
    Task<IAsyncDisposable?> TryAcquireAsync(
        string key, TimeSpan holdFor, CancellationToken cancellationToken);
}