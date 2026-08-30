using FxRateService.Core.Abstractions;

namespace FxRateService.Core.Rates;

/// <summary>
/// Jedan ciklus osvezavanja: povuci sa izvora, upisi u bazu, invalidiraj cache.
/// Odvojeno od BackgroundService-a da bi moglo da se testira bez hosta.
/// </summary>
public sealed class RateRefresher
{
    private readonly IRateProvider _provider;
    private readonly IRateRepository _repository;
    private readonly IRateCache _cache;

    public RateRefresher(IRateProvider provider, IRateRepository repository, IRateCache cache)
    {
        _provider = provider;
        _repository = repository;
        _cache = cache;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var snapshot = await _provider.GetLatestAsync(cancellationToken);

        await _repository.SaveAsync(snapshot, _provider.SourceName, cancellationToken);

        await _cache.InvalidateAsync(_provider.SourceName, cancellationToken);
    }
}