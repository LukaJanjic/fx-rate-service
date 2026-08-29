using FxRateService.Core.Abstractions;
using FxRateService.Infrastructure.Persistence;
using FxRateService.Infrastructure.Providers.Ecb;
using FxRateService.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace FxRateService.Infrastructure;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string? postgresConnectionString = null)
    {
        services.AddSingleton<IClock, SystemClock>();

        if (postgresConnectionString is not null)
        {
            services.AddDbContext<FxDbContext>(options =>
                options.UseNpgsql(postgresConnectionString));

            services.AddScoped<IRateRepository, PostgresRateRepository>();
        }

        services.AddHttpClient<IRateProvider, EcbRateProvider>()
            .AddResilienceHandler("ecb", builder =>
            {
                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,
                });

                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    BreakDuration = TimeSpan.FromSeconds(30),
                });

                builder.AddTimeout(TimeSpan.FromSeconds(10));
            });

        return services;
    }
}