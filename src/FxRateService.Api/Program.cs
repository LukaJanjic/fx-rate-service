using FxRateService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(
    postgresConnectionString: builder.Configuration.GetConnectionString("Postgres"),
    redisConnectionString: builder.Configuration.GetConnectionString("Redis"));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();