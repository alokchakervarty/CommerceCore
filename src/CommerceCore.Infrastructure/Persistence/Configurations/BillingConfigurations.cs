using CommerceCore.Domain.Entities.Billing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceCore.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.InvoiceNumber).HasMaxLength(50).IsRequired();
        builder.Property(i => i.FinancialYear).HasMaxLength(10).IsRequired();

        builder.Property(i => i.SellerLegalName).HasMaxLength(300).IsRequired();
        builder.Property(i => i.SellerGstNumber).HasMaxLength(15);
        builder.Property(i => i.SellerPanNumber).HasMaxLength(10);
        builder.Property(i => i.SellerAddressLine1).HasMaxLength(255);
        builder.Property(i => i.SellerAddressLine2).HasMaxLength(255);
        builder.Property(i => i.SellerCity).HasMaxLength(100);
        builder.Property(i => i.SellerState).HasMaxLength(100);
        builder.Property(i => i.SellerPostalCode).HasMaxLength(20);

        builder.Property(i => i.BuyerName).HasMaxLength(200).IsRequired();
        builder.Property(i => i.BuyerAddressLine1).HasMaxLength(255);
        builder.Property(i => i.BuyerAddressLine2).HasMaxLength(255);
        builder.Property(i => i.BuyerCity).HasMaxLength(100);
        builder.Property(i => i.BuyerState).HasMaxLength(100);
        builder.Property(i => i.BuyerPostalCode).HasMaxLength(20);
        builder.Property(i => i.BuyerPhoneNumber).HasMaxLength(30);

        builder.Property(i => i.PlaceOfSupplyState).HasMaxLength(100).IsRequired();

        foreach (var money in new[] { nameof(Invoice.TaxableValue), nameof(Invoice.TotalCgstAmount), nameof(Invoice.TotalSgstAmount), nameof(Invoice.TotalIgstAmount), nameof(Invoice.TotalDiscountAmount), nameof(Invoice.TotalAmount) })
            builder.Property(money).HasColumnType("decimal(14,2)");

        builder.Property(i => i.Version).IsConcurrencyToken();

        builder.HasIndex(i => new { i.StoreId, i.InvoiceNumber }).IsUnique();
        builder.HasIndex(i => i.OrderId).IsUnique(); // exactly one invoice per order

        builder.HasOne(i => i.Order)
            .WithMany()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class InvoiceSequenceConfiguration : IEntityTypeConfiguration<InvoiceSequence>
{
    public void Configure(EntityTypeBuilder<InvoiceSequence> builder)
    {
        builder.ToTable("InvoiceSequences");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FinancialYear).HasMaxLength(10).IsRequired();
        builder.Property(s => s.Version).IsConcurrencyToken();

        builder.HasIndex(s => new { s.StoreId, s.FinancialYear }).IsUnique();
    }
}
