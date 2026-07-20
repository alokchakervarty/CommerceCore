namespace CommerceCore.Contracts.Catalog;

public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    Guid? ParentCategoryId,
    bool IsActive);

public record CreateCategoryRequest(
    string Name,
    string? Description,
    string? ImageUrl,
    Guid? ParentCategoryId);

public record UpdateCategoryRequest(
    string Name,
    string? Description,
    string? ImageUrl,
    Guid? ParentCategoryId,
    bool IsActive);

public record ProductAttributeDto(string AttributeName, string AttributeCode, string Value);

public record ProductVariantDto(
    Guid Id,
    string Sku,
    string DisplayName,
    decimal Price,
    decimal? CompareAtPrice,
    int AvailableStock,
    string? ImageUrl,
    bool IsActive,
    IReadOnlyList<ProductAttributeDto> Attributes);

public record ProductDto(
    Guid Id,
    string Name,
    string Slug,
    string? ShortDescription,
    string? Description,
    decimal BasePrice,
    decimal? CompareAtPrice,
    bool HasVariants,
    bool IsActive,
    bool IsFeatured,
    double AverageRating,
    int ReviewCount,
    Guid CategoryId,
    string? CategoryName,
    Guid? BrandId,
    string? BrandName,
    IReadOnlyList<string> ImageUrls,
    IReadOnlyList<ProductAttributeDto> Attributes,
    IReadOnlyList<ProductVariantDto> Variants);

public record CreateProductRequest(
    string Name,
    string? ShortDescription,
    string? Description,
    string? Sku,
    decimal BasePrice,
    decimal? CompareAtPrice,
    decimal? CostPrice,
    bool TrackInventory,
    Guid CategoryId,
    Guid? BrandId,
    IReadOnlyList<string>? ImageUrls);

public record UpdateProductRequest(
    string Name,
    string? ShortDescription,
    string? Description,
    decimal BasePrice,
    decimal? CompareAtPrice,
    decimal? CostPrice,
    bool TrackInventory,
    bool IsActive,
    bool IsFeatured,
    Guid CategoryId,
    Guid? BrandId);

public record ProductListQuery
{
    public string? Search { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? BrandId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public string? SortBy { get; init; } = "created_desc"; // created_desc, price_asc, price_desc, name_asc, rating_desc
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
