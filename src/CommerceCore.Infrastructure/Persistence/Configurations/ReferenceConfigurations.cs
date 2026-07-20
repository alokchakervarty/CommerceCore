using CommerceCore.Domain.Entities.Reference;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceCore.Infrastructure.Persistence.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Iso2Code).HasMaxLength(2).IsRequired();
        builder.Property(c => c.Iso3Code).HasMaxLength(3).IsRequired();
        builder.Property(c => c.PhoneCode).HasMaxLength(10);
        builder.Property(c => c.Version).IsConcurrencyToken();

        builder.HasIndex(c => c.Iso2Code).IsUnique();

        builder.HasMany(c => c.States)
            .WithOne(s => s.Country)
            .HasForeignKey(s => s.CountryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class StateConfiguration : IEntityTypeConfiguration<State>
{
    public void Configure(EntityTypeBuilder<State> builder)
    {
        builder.ToTable("States");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Code).HasMaxLength(20);
        builder.Property(s => s.Version).IsConcurrencyToken();

        builder.HasIndex(s => new { s.CountryId, s.Name });

        builder.HasMany(s => s.Cities)
            .WithOne(c => c.State)
            .HasForeignKey(c => c.StateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Version).IsConcurrencyToken();

        builder.HasIndex(c => new { c.StateId, c.Name });
    }
}

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).HasMaxLength(3).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Symbol).HasMaxLength(10).IsRequired();
        builder.Property(c => c.ExchangeRateToBase).HasColumnType("decimal(18,6)");
        builder.Property(c => c.Version).IsConcurrencyToken();

        builder.HasIndex(c => c.Code).IsUnique();
    }
}

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Code).HasMaxLength(10).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(100).IsRequired();
        builder.Property(l => l.NativeName).HasMaxLength(100).IsRequired();
        builder.Property(l => l.Version).IsConcurrencyToken();

        builder.HasIndex(l => l.Code).IsUnique();
    }
}

public class TaxConfiguration : IEntityTypeConfiguration<Tax>
{
    public void Configure(EntityTypeBuilder<Tax> builder)
    {
        builder.ToTable("Taxes");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).HasMaxLength(150).IsRequired();
        builder.Property(t => t.RatePercentage).HasColumnType("decimal(6,3)");
        builder.Property(t => t.Version).IsConcurrencyToken();

        builder.HasIndex(t => t.StoreId);
    }
}

public class ShippingZoneConfiguration : IEntityTypeConfiguration<ShippingZone>
{
    public void Configure(EntityTypeBuilder<ShippingZone> builder)
    {
        builder.ToTable("ShippingZones");
        builder.HasKey(z => z.Id);
        builder.Property(z => z.Name).HasMaxLength(150).IsRequired();
        builder.Property(z => z.Version).IsConcurrencyToken();

        builder.HasMany(z => z.ShippingMethods)
            .WithOne(m => m.ShippingZone)
            .HasForeignKey(m => m.ShippingZoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ShippingMethodConfiguration : IEntityTypeConfiguration<ShippingMethod>
{
    public void Configure(EntityTypeBuilder<ShippingMethod> builder)
    {
        builder.ToTable("ShippingMethods");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).HasMaxLength(150).IsRequired();
        builder.Property(m => m.Description).HasMaxLength(500);
        builder.Property(m => m.RateType).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.FlatRate).HasColumnType("decimal(10,2)");
        builder.Property(m => m.RatePerKg).HasColumnType("decimal(10,2)");
        builder.Property(m => m.FreeShippingThreshold).HasColumnType("decimal(10,2)");
        builder.Property(m => m.Version).IsConcurrencyToken();
    }
}
