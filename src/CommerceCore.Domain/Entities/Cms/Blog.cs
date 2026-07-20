using CommerceCore.Domain.Entities.Identity;
using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Cms;

public class BlogCategory : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}

/// <summary>A store's blog/content post — used for SEO landing pages, buying guides,
/// announcements, etc. Deliberately generic so any vertical can reuse it (perfume
/// "fragrance guides", electronics "buying guides", fashion "lookbooks").</summary>
public class Blog : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public Guid? BlogCategoryId { get; set; }
    public BlogCategory? BlogCategory { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string Body { get; set; } = string.Empty;    // rich HTML/Markdown content
    public string? FeaturedImageUrl { get; set; }

    public Guid AuthorUserId { get; set; }
    public User? AuthorUser { get; set; }

    public bool IsPublished { get; set; }
    public DateTime? PublishedAt { get; set; }

    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    public int ViewCount { get; set; }
}
