using FxRateService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();