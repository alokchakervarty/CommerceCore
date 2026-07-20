using CommerceCore.Shared.Entities;

namespace CommerceCore.Domain.Entities.Catalog;

/// <summary>Hierarchical product category. Self-referencing via ParentCategoryId
/// to support unlimited nesting (e.g. Fashion > Men > Shirts > Formal).</summary>
public class Category : BaseEntity, IStoreScoped
{
    public Guid StoreId { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> ChildCategories { get; set; } = new List<Category>();

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
