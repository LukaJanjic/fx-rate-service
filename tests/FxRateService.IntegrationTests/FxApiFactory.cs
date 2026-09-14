using FxRateService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using FxRateService.Core.Abstractions;
using FxRateService.Core.Domain;
using Microsoft.Extensions.DependencyInjection.Extensions;
using FxRateService.Core.Rates;
namespace FxRateService.IntegrationTests;
using Microsoft.Extensions.Hosting;
/// <summary>
/// Dize ceo servis u memoriji, sa pravim Postgresom i Redisom iz kontejnera.
/// Jedina razlika u odnosu na produkciju su connection stringovi.
/// </summary>
public sealed class FxApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres =
        new PostgreSqlBuilder("postgres:17").WithDatabase("fxrates").Build();

    private readonly RedisContainer _redis =
        new RedisBuilder("redis:7-alpine").Build();

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();
        await _redis.StartAsync();

        using var scope = Services.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<FxDbContext>();
        await db.Database.MigrateAsync();

        var refresher = scope.ServiceProvider.GetRequiredService<RateRefresher>();
        await refresher.RefreshAsync(CancellationToken.None);
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Postgres", _postgres.GetConnectionString());
        builder.UseSetting("ConnectionStrings:Redis", _redis.GetConnectionString());
        builder.UseSetting("RateRefresh:IntervalMinutes", "1440");

                builder.ConfigureServices(services =>
        {
            services.RemoveAll<IRateProvider>();
            services.AddSingleton<IRateProvider, FakeRateProvider>();

            services.RemoveAll<IHostedService>();
        });
    }

        private sealed class FakeRateProvider : IRateProvider
    {
        public string SourceName => "ECB";

        public Task<RateSnapshot> GetLatestAsync(CancellationToken cancellationToken) =>
            Task.FromResult(RateSnapshot.Of(new DateOnly(2026, 8, 26),
            [
                ExchangeRate.Of("EUR", "USD", 1.1669m),
                ExchangeRate.Of("EUR", "JPY", 185.62m),
                ExchangeRate.Of("EUR", "GBP", 0.85613m),
            ]));
    }
}