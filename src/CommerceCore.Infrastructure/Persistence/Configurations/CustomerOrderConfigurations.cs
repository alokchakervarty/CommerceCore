using CommerceCore.Domain.Entities.Customers;
using CommerceCore.Domain.Entities.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceCore.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.LastName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(255).IsRequired();
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.Gender).HasMaxLength(30);
        builder.Property(c => c.TotalSpent).HasColumnType("decimal(14,2)");
        builder.Property(c => c.Version).IsConcurrencyToken();

        builder.HasIndex(c => new { c.StoreId, c.Email });

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Addresses)
            .WithOne(a => a.Customer)
            .HasForeignKey(a => a.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(c => c.FullName);
    }
}

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.FullName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.PhoneNumber).HasMaxLength(30).IsRequired();
        builder.Property(a => a.AddressLine1).HasMaxLength(255).IsRequired();
        builder.Property(a => a.AddressLine2).HasMaxLength(255);
        builder.Property(a => a.City).HasMaxLength(100).IsRequired();
        builder.Property(a => a.State).HasMaxLength(100).IsRequired();
        builder.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Version).IsConcurrencyToken();
    }
}

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems");
        builder.HasKey(ci => ci.Id);
        builder.Property(ci => ci.Version).IsConcurrencyToken();

        builder.HasIndex(ci => new { ci.CustomerId, ci.ProductVariantId }).IsUnique();

        builder.HasOne(ci => ci.Customer)
            .WithMany()
            .HasForeignKey(ci => ci.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.Product)
            .WithMany()
            .HasForeignKey(ci => ci.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ci => ci.ProductVariant)
            .WithMany()
            .HasForeignKey(ci => ci.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(o => o.CurrencyCode).HasMaxLength(10).IsRequired();

        foreach (var money in new[] { nameof(Order.SubTotal), nameof(Order.DiscountAmount), nameof(Order.ShippingAmount), nameof(Order.TaxAmount), nameof(Order.TotalAmount) })
            builder.Property(money).HasColumnType("decimal(14,2)");

        builder.Property(o => o.CouponCode).HasMaxLength(50);
        builder.Property(o => o.ShippingMethodName).HasMaxLength(150);

        builder.Property(o => o.ShippingFullName).HasMaxLength(200);
        builder.Property(o => o.ShippingPhoneNumber).HasMaxLength(30);
        builder.Property(o => o.ShippingAddressLine1).HasMaxLength(255);
        builder.Property(o => o.ShippingAddressLine2).HasMaxLength(255);
        builder.Property(o => o.ShippingCity).HasMaxLength(100);
        builder.Property(o => o.ShippingState).HasMaxLength(100);
        builder.Property(o => o.ShippingPostalCode).HasMaxLength(20);
        builder.Property(o => o.ShippingCountry).HasMaxLength(100);

        builder.Property(o => o.BillingFullName).HasMaxLength(200);
        builder.Property(o => o.BillingPhoneNumber).HasMaxLength(30);
        builder.Property(o => o.BillingAddressLine1).HasMaxLength(255);
        builder.Property(o => o.BillingAddressLine2).HasMaxLength(255);
        builder.Property(o => o.BillingCity).HasMaxLength(100);
        builder.Property(o => o.BillingState).HasMaxLength(100);
        builder.Property(o => o.BillingPostalCode).HasMaxLength(20);
        builder.Property(o => o.BillingCountry).HasMaxLength(100);

        builder.Property(o => o.Version).IsConcurrencyToken();

        builder.HasIndex(o => new { o.StoreId, o.OrderNumber }).IsUnique();
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.Status);

        builder.HasMany(o => o.OrderItems)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.ProductNameSnapshot).HasMaxLength(300).IsRequired();
        builder.Property(oi => oi.VariantDisplayNameSnapshot).HasMaxLength(300);
        builder.Property(oi => oi.SkuSnapshot).HasMaxLength(100).IsRequired();
        builder.Property(oi => oi.ImageUrlSnapshot).HasMaxLength(1000);
        builder.Property(oi => oi.UnitPrice).HasColumnType("decimal(12,2)");
        builder.Property(oi => oi.DiscountAmount).HasColumnType("decimal(12,2)");
        builder.Property(oi => oi.TaxAmount).HasColumnType("decimal(12,2)");
        builder.Property(oi => oi.Version).IsConcurrencyToken();

        builder.HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(oi => oi.ProductVariant)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(oi => oi.Warehouse)
            .WithMany()
            .HasForeignKey(oi => oi.WarehouseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Ignore(oi => oi.LineTotal);
    }
}
