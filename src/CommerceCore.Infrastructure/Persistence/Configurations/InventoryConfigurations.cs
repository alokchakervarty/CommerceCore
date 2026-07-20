using CommerceCore.Domain.Entities.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceCore.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name).HasMaxLength(200).IsRequired();
        builder.Property(w => w.Code).HasMaxLength(50).IsRequired();
        builder.Property(w => w.Version).IsConcurrencyToken();

        builder.HasIndex(w => new { w.StoreId, w.Code }).IsUnique();
    }
}

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        // Maps to the "Inventory" table required by the spec; class is named
        // InventoryItem in C# to avoid sharing its name with the containing namespace.
        builder.ToTable("Inventory");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.BinLocation).HasMaxLength(100);
        builder.Property(i => i.Version).IsConcurrencyToken();

        builder.HasIndex(i => new { i.WarehouseId, i.ProductVariantId }).IsUnique();

        builder.HasOne(i => i.Warehouse)
            .WithMany(w => w.InventoryItems)
            .HasForeignKey(i => i.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(i => i.QuantityAvailable);

        builder.HasMany(i => i.StockMovements)
            .WithOne(m => m.InventoryItem)
            .HasForeignKey(m => m.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.MovementType).HasConversion<string>().HasMaxLength(30);
        builder.Property(m => m.ReferenceType).HasMaxLength(100);
        builder.Property(m => m.Notes).HasMaxLength(1000);
        builder.Property(m => m.Version).IsConcurrencyToken();

        builder.HasIndex(m => new { m.ReferenceType, m.ReferenceId });
    }
}
