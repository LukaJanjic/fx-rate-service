using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FxRateService.Infrastructure.Persistence;

/// <summary>
/// Koristi je SAMO 'dotnet ef' alat pri generisanju migracija.
/// Connection string je lazan — alat cita model, ne povezuje se na bazu.
/// U produkciji kontekst dolazi iz DI-ja sa pravom konfiguracijom.
/// </summary>
public sealed class FxDbContextDesignTimeFactory : IDesignTimeDbContextFactory<FxDbContext>
{
    public FxDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FxDbContext>()
            .UseNpgsql("Host=localhost;Port=5433;Database=fxrates;Username=postgres;Password=postgres")
            .Options;

        return new FxDbContext(options);
    }
}