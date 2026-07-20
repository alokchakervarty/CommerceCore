using CommerceCore.Domain.Entities.SystemAudit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceCore.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserDisplayName).HasMaxLength(200);
        builder.Property(a => a.EntityName).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(30).IsRequired();
        builder.Property(a => a.OldValuesJson).HasColumnType("text");
        builder.Property(a => a.NewValuesJson).HasColumnType("text");
        builder.Property(a => a.ChangedPropertiesJson).HasColumnType("text");
        builder.Property(a => a.IpAddress).HasMaxLength(50);
        builder.Property(a => a.UserAgent).HasMaxLength(500);

        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.OccurredAt);
    }
}

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("ActivityLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.ActorDisplayName).HasMaxLength(200);
        builder.Property(a => a.ActivityType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.RelatedEntityName).HasMaxLength(150);

        builder.HasIndex(a => a.StoreId);
        builder.HasIndex(a => a.OccurredAt);
    }
}

public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
{
    public void Configure(EntityTypeBuilder<SystemSetting> builder)
    {
        builder.ToTable("SystemSettings");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Key).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Value).HasColumnType("text").IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.Version).IsConcurrencyToken();

        builder.HasIndex(s => new { s.StoreId, s.Key }).IsUnique();
    }
}

public class EmailTemplateConfiguration : IEntityTypeConfiguration<EmailTemplate>
{
    public void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable("EmailTemplates");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Subject).HasMaxLength(300).IsRequired();
        builder.Property(e => e.HtmlBody).HasColumnType("text").IsRequired();
        builder.Property(e => e.PlainTextBody).HasColumnType("text");
        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.HasIndex(e => new { e.StoreId, e.Code }).IsUnique();
    }
}
