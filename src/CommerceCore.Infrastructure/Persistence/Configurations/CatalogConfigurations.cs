using CommerceCore.Domain.Entities.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceCore.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Version).IsConcurrencyToken();

        builder.HasIndex(c => new { c.StoreId, c.Slug }).IsUnique();

        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.ChildCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict); // prevent a delete cascading through the whole tree
    }
}

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.ToTable("Brands");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Slug).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Version).IsConcurrencyToken();

        builder.HasIndex(b => new { b.StoreId, b.Slug }).IsUnique();
    }
}

public class CollectionConfiguration : IEntityTypeConfiguration<Collection>
{
    public void Configure(EntityTypeBuilder<Collection> builder)
    {
        builder.ToTable("Collections");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Version).IsConcurrencyToken();

        builder.HasIndex(c => new { c.StoreId, c.Slug }).IsUnique();
    }
}

public class CollectionProductConfiguration : IEntityTypeConfiguration<CollectionProduct>
{
    public void Configure(EntityTypeBuilder<CollectionProduct> builder)
    {
        builder.ToTable("CollectionProducts");
        builder.HasKey(cp => cp.Id);
        builder.Property(cp => cp.Version).IsConcurrencyToken();

        builder.HasIndex(cp => new { cp.CollectionId, cp.ProductId }).IsUnique();

        builder.HasOne(cp => cp.Collection)
            .WithMany(c => c.CollectionProducts)
            .HasForeignKey(cp => cp.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.Product)
            .WithMany(p => p.CollectionProducts)
            .HasForeignKey(cp => cp.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Sku).HasMaxLength(100);
        builder.Property(p => p.ShortDescription).HasMaxLength(500);
        builder.Property(p => p.Description).HasColumnType("text");

        builder.Property(p => p.BasePrice).HasColumnType("decimal(12,2)");
        builder.Property(p => p.CompareAtPrice).HasColumnType("decimal(12,2)");
        builder.Property(p => p.CostPrice).HasColumnType("decimal(12,2)");
        builder.Property(p => p.WeightKg).HasColumnType("decimal(10,3)");
        builder.Property(p => p.LengthCm).HasColumnType("decimal(10,2)");
        builder.Property(p => p.WidthCm).HasColumnType("decimal(10,2)");
        builder.Property(p => p.HeightCm).HasColumnType("decimal(10,2)");

        builder.Property(p => p.Version).IsConcurrencyToken();

        builder.HasIndex(p => new { p.StoreId, p.Slug }).IsUnique();
        builder.HasIndex(p => new { p.StoreId, p.Sku });
        builder.HasIndex(p => p.CategoryId);

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(p => p.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Variants)
            .WithOne(v => v.Product)
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Images)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Sku).HasMaxLength(100).IsRequired();
        builder.Property(v => v.Barcode).HasMaxLength(100);
        builder.Property(v => v.DisplayName).HasMaxLength(300);
        builder.Property(v => v.Price).HasColumnType("decimal(12,2)");
        builder.Property(v => v.CompareAtPrice).HasColumnType("decimal(12,2)");
        builder.Property(v => v.WeightKg).HasColumnType("decimal(10,3)");

        builder.Property(v => v.Version).IsConcurrencyToken();

        builder.HasIndex(v => v.Sku).IsUnique();
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Url).HasMaxLength(1000).IsRequired();
        builder.Property(i => i.AltText).HasMaxLength(300);
        builder.Property(i => i.Version).IsConcurrencyToken();

        builder.HasOne(i => i.ProductVariant)
            .WithMany()
            .HasForeignKey(i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class AttributeDefinitionConfiguration : IEntityTypeConfiguration<AttributeDefinition>
{
    public void Configure(EntityTypeBuilder<AttributeDefinition> builder)
    {
        // Maps to the "Attributes" table required by the spec; class is named
        // AttributeDefinition in C# only to avoid colliding with System.Attribute.
        builder.ToTable("Attributes");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Code).HasMaxLength(150).IsRequired();
        builder.Property(a => a.InputType).HasConversion<string>().HasMaxLength(30);
        builder.Property(a => a.Version).IsConcurrencyToken();

        builder.HasIndex(a => new { a.StoreId, a.Code }).IsUnique();

        builder.HasMany(a => a.Values)
            .WithOne(v => v.Attribute)
            .HasForeignKey(v => v.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder.ToTable("AttributeValues");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Value).HasMaxLength(200).IsRequired();
        builder.Property(v => v.ColorHex).HasMaxLength(20);
        builder.Property(v => v.Version).IsConcurrencyToken();

        builder.HasIndex(v => new { v.AttributeId, v.Value }).IsUnique();
    }
}

public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.ToTable("ProductAttributes");
        builder.HasKey(pa => pa.Id);

        builder.Property(pa => pa.FreeTextValue).HasMaxLength(500);
        builder.Property(pa => pa.Version).IsConcurrencyToken();

        builder.HasOne(pa => pa.Product)
            .WithMany(p => p.AttributeAssignments)
            .HasForeignKey(pa => pa.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.ProductVariant)
            .WithMany(v => v.AttributeAssignments)
            .HasForeignKey(pa => pa.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pa => pa.Attribute)
            .WithMany()
            .HasForeignKey(pa => pa.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pa => pa.AttributeValueRef)
            .WithMany()
            .HasForeignKey(pa => pa.AttributeValueId)
            .OnDelete(DeleteBehavior.Restrict);

        // Exactly one of ProductId / ProductVariantId must be set.
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_ProductAttribute_ExactlyOneOwner",
            "(ProductId IS NOT NULL AND ProductVariantId IS NULL) OR (ProductId IS NULL AND ProductVariantId IS NOT NULL)"));
    }
}
