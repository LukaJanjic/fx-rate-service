using FxRateService.Core.Rates;

namespace FxRateService.Api.BackgroundJobs;

/// <summary>
/// Poziva RateRefresher na zadati interval. Sam ne zna sta znaci
/// "osveziti kurseve" — to je u Core-u.
/// </summary>
public sealed class RateRefreshService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RateRefreshService> _logger;

    public RateRefreshService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<RateRefreshService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(
            _configuration.GetValue("RateRefresh:IntervalMinutes", 240));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var refresher = scope.ServiceProvider.GetRequiredService<RateRefresher>();

                await refresher.RefreshAsync(stoppingToken);

                _logger.LogInformation("Kursevi osvezeni.");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Osvezavanje kurseva nije uspelo.");
            }

            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}