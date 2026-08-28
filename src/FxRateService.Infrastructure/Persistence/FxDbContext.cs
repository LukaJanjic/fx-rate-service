using Microsoft.EntityFrameworkCore;

namespace FxRateService.Infrastructure.Persistence;

public sealed class FxDbContext : DbContext
{
    public FxDbContext(DbContextOptions<FxDbContext> options) : base(options)
    {
    }

    public DbSet<ExchangeRateRecord> ExchangeRates => Set<ExchangeRateRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var rate = modelBuilder.Entity<ExchangeRateRecord>();

        rate.ToTable("exchange_rates");

        rate.HasKey(r => r.Id);

        rate.Property(r => r.Id)
            .HasColumnName("id");

        rate.Property(r => r.AsOf)
            .HasColumnName("as_of")
            .HasColumnType("date")
            .IsRequired();

        rate.Property(r => r.BaseCurrency)
            .HasColumnName("base_currency")
            .HasMaxLength(3)
            .IsFixedLength()
            .IsRequired();

        rate.Property(r => r.QuoteCurrency)
            .HasColumnName("quote_currency")
            .HasMaxLength(3)
            .IsFixedLength()
            .IsRequired();

        rate.Property(r => r.Rate)
            .HasColumnName("rate")
            .HasPrecision(18, 6)
            .IsRequired();

        rate.Property(r => r.Source)
            .HasColumnName("source")
            .HasMaxLength(16)
            .IsRequired();

        rate.Property(r => r.RetrievedAt)
            .HasColumnName("retrieved_at")
            .HasColumnType("timestamptz")
            .IsRequired();

        rate.HasIndex(r => new { r.AsOf, r.BaseCurrency, r.QuoteCurrency, r.Source })
            .IsUnique()
            .HasDatabaseName("ux_exchange_rates_day_pair_source");

        rate.HasIndex(r => new { r.BaseCurrency, r.QuoteCurrency, r.Source, r.AsOf })
            .HasDatabaseName("ix_exchange_rates_history");
    }
}