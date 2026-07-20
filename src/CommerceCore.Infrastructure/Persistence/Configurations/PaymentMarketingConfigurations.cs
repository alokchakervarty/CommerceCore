using CommerceCore.Domain.Entities.Marketing;
using CommerceCore.Domain.Entities.Payments;
using CommerceCore.Domain.Entities.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceCore.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Provider).HasMaxLength(100).IsRequired();
        builder.Property(p => p.MethodType).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(p => p.CurrencyCode).HasMaxLength(10);
        builder.Property(p => p.Amount).HasColumnType("decimal(14,2)");
        builder.Property(p => p.AmountCaptured).HasColumnType("decimal(14,2)");
        builder.Property(p => p.AmountRefunded).HasColumnType("decimal(14,2)");
        builder.Property(p => p.GatewayCustomerId).HasMaxLength(150);
        builder.Property(p => p.GatewayPaymentIntentId).HasMaxLength(150);
        builder.Property(p => p.FailureReason).HasMaxLength(500);
        builder.Property(p => p.Version).IsConcurrencyToken();

        builder.HasIndex(p => p.OrderId);

        builder.HasMany(p => p.Transactions)
            .WithOne(t => t.Payment)
            .HasForeignKey(t => t.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("PaymentTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(t => t.CurrencyCode).HasMaxLength(10);
        builder.Property(t => t.Amount).HasColumnType("decimal(14,2)");
        builder.Property(t => t.GatewayTransactionId).HasMaxLength(150);
        builder.Property(t => t.GatewayResponseRaw).HasColumnType("text");
        builder.Property(t => t.FailureReason).HasMaxLength(500);
        builder.Property(t => t.Version).IsConcurrencyToken();
    }
}

public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.ToTable("Coupons");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).HasMaxLength(50).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.DiscountType).HasConversion<string>().HasMaxLength(30);
        builder.Property(c => c.DiscountValue).HasColumnType("decimal(12,2)");
        builder.Property(c => c.MinimumOrderAmount).HasColumnType("decimal(12,2)");
        builder.Property(c => c.MaxDiscountAmount).HasColumnType("decimal(12,2)");
        builder.Property(c => c.Version).IsConcurrencyToken();

        builder.HasIndex(c => new { c.StoreId, c.Code }).IsUnique();

        builder.HasMany(c => c.Usages)
            .WithOne(u => u.Coupon)
            .HasForeignKey(u => u.CouponId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("CouponUsage");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.DiscountAmountApplied).HasColumnType("decimal(12,2)");
        builder.Property(u => u.Version).IsConcurrencyToken();

        builder.HasIndex(u => new { u.CouponId, u.OrderId }).IsUnique();
    }
}

public class WishlistConfiguration : IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("Wishlists");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Name).HasMaxLength(150).IsRequired();
        builder.Property(w => w.Version).IsConcurrencyToken();

        builder.HasMany(w => w.Items)
            .WithOne(i => i.Wishlist)
            .HasForeignKey(i => i.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Version).IsConcurrencyToken();

        builder.HasIndex(i => new { i.WishlistId, i.ProductId, i.ProductVariantId }).IsUnique();
    }
}

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Title).HasMaxLength(200);
        builder.Property(r => r.Body).HasColumnType("text");
        builder.Property(r => r.MerchantReplyBody).HasColumnType("text");
        builder.Property(r => r.Version).IsConcurrencyToken();

        builder.HasIndex(r => r.ProductId);

        builder.ToTable(t => t.HasCheckConstraint("CK_Review_Rating_Range", "\"Rating\" >= 1 AND \"Rating\" <= 5"));
    }
}
