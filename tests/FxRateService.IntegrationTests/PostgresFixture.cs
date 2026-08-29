using FxRateService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace FxRateService.IntegrationTests;

/// <summary>
/// Dize pravi Postgres kontejner za trajanje test run-a.
/// Port bira Docker — nema sukoba sa lokalnim Postgresom ni izmedju testova.
/// </summary>
public sealed class PostgresFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("fxrates")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public FxDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FxDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new FxDbContext(options);
    }
}