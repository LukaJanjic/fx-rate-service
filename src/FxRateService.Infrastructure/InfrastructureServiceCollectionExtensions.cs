using FxRateService.Core.Abstractions;
using FxRateService.Infrastructure.Providers.Ecb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace FxRateService.Infrastructure;

/// <summary>
/// Jedina tacka kroz koju Api projekat zna za Infrastructure.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddHttpClient<IRateProvider, EcbRateProvider>()
            .AddResilienceHandler("ecb", builder =>
            {
                // Spoljasnji sloj: odlucuje da li se ceo pokusaj ponavlja.
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                });

                // Srednji sloj: prestaje da pokusava kad izvor ocigledno ne radi.
                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    BreakDuration = TimeSpan.FromSeconds(30),
                });

                // Unutrasnji sloj: ogranicava JEDAN pokusaj, ne ceo lanac.
                builder.AddTimeout(TimeSpan.FromSeconds(10));
            });

        return services;
    }
}