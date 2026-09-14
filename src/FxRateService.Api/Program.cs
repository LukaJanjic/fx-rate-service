using FxRateService.Api.BackgroundJobs;
using FxRateService.Api.Endpoints;
using FxRateService.Core.Rates;
using FxRateService.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddInfrastructure(
    postgresConnectionString: builder.Configuration.GetConnectionString("Postgres"),
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"));

builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("Postgres")!,
        name: "postgres")
    .AddRedis(
        builder.Configuration.GetConnectionString("Redis")!,
        name: "redis");
builder.Services.AddScoped<RateRefresher>();
builder.Services.AddHostedService<RateRefreshService>();
var app = builder.Build();
app.UseSerilogRequestLogging();
app.MapHealthChecks("/health");
app.MapRatesEndpoints();
app.MapConversionEndpoints();
app.Run();