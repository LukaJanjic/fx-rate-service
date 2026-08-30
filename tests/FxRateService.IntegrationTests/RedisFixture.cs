using StackExchange.Redis;
using Testcontainers.Redis;

namespace FxRateService.IntegrationTests;

/// <summary>
/// Dize pravi Redis kontejner za trajanje test run-a.
/// </summary>
public sealed class RedisFixture : IAsyncLifetime
{
    private readonly RedisContainer _container = new RedisBuilder("redis:7-alpine").Build();

    private IConnectionMultiplexer? _connection;

    public IConnectionMultiplexer Connection =>
        _connection ?? throw new InvalidOperationException("Fixture nije inicijalizovan.");

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();

        _connection = await ConnectionMultiplexer.ConnectAsync(_container.GetConnectionString());
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.DisposeAsync();
        }

        await _container.DisposeAsync();
    }
}