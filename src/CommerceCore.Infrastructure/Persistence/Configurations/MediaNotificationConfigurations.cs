using CommerceCore.Domain.Entities.Media;
using CommerceCore.Domain.Entities.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceCore.Infrastructure.Persistence.Configurations;

public class MediaAssetConfiguration : IEntityTypeConfiguration<MediaAsset>
{
    public void Configure(EntityTypeBuilder<MediaAsset> builder)
    {
        // Maps to the "Media" table required by the spec.
        builder.ToTable("Media");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FileName).HasMaxLength(300).IsRequired();
        builder.Property(m => m.Url).HasMaxLength(1000).IsRequired();
        builder.Property(m => m.ThumbnailUrl).HasMaxLength(1000);
        builder.Property(m => m.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(m => m.AltText).HasMaxLength(300);
        builder.Property(m => m.Folder).HasMaxLength(150);
        builder.Property(m => m.Version).IsConcurrencyToken();
    }
}

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("NotificationTemplates");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Code).HasMaxLength(150).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(150).IsRequired();
        builder.Property(t => t.Channel).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.SubjectTemplate).HasMaxLength(300);
        builder.Property(t => t.BodyTemplate).HasColumnType("text").IsRequired();
        builder.Property(t => t.Version).IsConcurrencyToken();

        builder.HasIndex(t => new { t.StoreId, t.Code, t.Channel }).IsUnique();
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Channel).HasConversion<string>().HasMaxLength(20);
        builder.Property(n => n.RecipientAddress).HasMaxLength(255).IsRequired();
        builder.Property(n => n.Subject).HasMaxLength(300);
        builder.Property(n => n.Body).HasColumnType("text").IsRequired();
        builder.Property(n => n.FailureReason).HasMaxLength(500);
        builder.Property(n => n.Version).IsConcurrencyToken();

        builder.HasOne(n => n.NotificationTemplate)
            .WithMany()
            .HasForeignKey(n => n.NotificationTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(n => n.RecipientId);
    }
}
