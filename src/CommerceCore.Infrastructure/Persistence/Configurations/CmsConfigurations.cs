using CommerceCore.Domain.Entities.Cms;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CommerceCore.Infrastructure.Persistence.Configurations;

public class BlogCategoryConfiguration : IEntityTypeConfiguration<BlogCategory>
{
    public void Configure(EntityTypeBuilder<BlogCategory> builder)
    {
        builder.ToTable("BlogCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Version).IsConcurrencyToken();

        builder.HasIndex(c => new { c.StoreId, c.Slug }).IsUnique();
    }
}

public class BlogConfiguration : IEntityTypeConfiguration<Blog>
{
    public void Configure(EntityTypeBuilder<Blog> builder)
    {
        builder.ToTable("Blogs");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title).HasMaxLength(300).IsRequired();
        builder.Property(b => b.Slug).HasMaxLength(300).IsRequired();
        builder.Property(b => b.Excerpt).HasMaxLength(500);
        builder.Property(b => b.Body).HasColumnType("text").IsRequired();
        builder.Property(b => b.Version).IsConcurrencyToken();

        builder.HasIndex(b => new { b.StoreId, b.Slug }).IsUnique();

        builder.HasOne(b => b.BlogCategory)
            .WithMany(c => c.Blogs)
            .HasForeignKey(b => b.BlogCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(b => b.AuthorUser)
            .WithMany()
            .HasForeignKey(b => b.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable("Pages");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Slug).HasMaxLength(300).IsRequired();
        builder.Property(p => p.Body).HasColumnType("text").IsRequired();
        builder.Property(p => p.Version).IsConcurrencyToken();

        builder.HasIndex(p => new { p.StoreId, p.Slug }).IsUnique();
    }
}

public class MenuConfiguration : IEntityTypeConfiguration<Menu>
{
    public void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable("Menus");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).HasMaxLength(150).IsRequired();
        builder.Property(m => m.Location).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Version).IsConcurrencyToken();

        builder.HasMany(m => m.Items)
            .WithOne(i => i.Menu)
            .HasForeignKey(i => i.MenuId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.ToTable("MenuItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Label).HasMaxLength(150).IsRequired();
        builder.Property(i => i.LinkType).HasMaxLength(50).IsRequired();
        builder.Property(i => i.LinkTarget).HasMaxLength(500).IsRequired();
        builder.Property(i => i.Version).IsConcurrencyToken();

        builder.HasOne(i => i.ParentMenuItem)
            .WithMany(i => i.Children)
            .HasForeignKey(i => i.ParentMenuItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class BannerConfiguration : IEntityTypeConfiguration<Banner>
{
    public void Configure(EntityTypeBuilder<Banner> builder)
    {
        builder.ToTable("Banners");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Subtitle).HasMaxLength(300);
        builder.Property(b => b.ImageUrl).HasMaxLength(1000).IsRequired();
        builder.Property(b => b.MobileImageUrl).HasMaxLength(1000);
        builder.Property(b => b.LinkType).HasMaxLength(50);
        builder.Property(b => b.LinkTarget).HasMaxLength(500);
        builder.Property(b => b.Placement).HasMaxLength(100).IsRequired();
        builder.Property(b => b.Version).IsConcurrencyToken();
    }
}

public class FaqConfiguration : IEntityTypeConfiguration<Faq>
{
    public void Configure(EntityTypeBuilder<Faq> builder)
    {
        builder.ToTable("Faqs");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Question).HasMaxLength(500).IsRequired();
        builder.Property(f => f.Answer).HasColumnType("text").IsRequired();
        builder.Property(f => f.Category).HasMaxLength(100);
        builder.Property(f => f.Version).IsConcurrencyToken();
    }
}
