using CommerceCore.Domain.Entities.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceCore.Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Slug).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Domain).HasMaxLength(255);
        builder.Property(s => s.Version).IsConcurrencyToken();

        builder.HasIndex(s => s.Slug).IsUnique();
        builder.HasIndex(s => s.Domain).IsUnique();

        builder.HasOne(s => s.Settings)
            .WithOne(ss => ss.Store)
            .HasForeignKey<StoreSettings>(ss => ss.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Theme)
            .WithOne(t => t.Store)
            .HasForeignKey<StoreTheme>(t => t.StoreId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StoreSettingsConfiguration : IEntityTypeConfiguration<StoreSettings>
{
    public void Configure(EntityTypeBuilder<StoreSettings> builder)
    {
        builder.ToTable("StoreSettings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Version).IsConcurrencyToken();
        builder.HasIndex(s => s.StoreId).IsUnique();
    }
}

public class StoreThemeConfiguration : IEntityTypeConfiguration<StoreTheme>
{
    public void Configure(EntityTypeBuilder<StoreTheme> builder)
    {
        builder.ToTable("StoreThemes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.CustomCss).HasColumnType("text");
        builder.Property(t => t.LayoutJson).HasColumnType("text");
        builder.Property(t => t.Version).IsConcurrencyToken();
        builder.HasIndex(t => t.StoreId).IsUnique();
    }
}
