using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Catalog;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Catalog.Products;

/// <summary>Builds a fully-populated ProductDto (variants, images, attributes) with a
/// single set of queries, shared by every command/query handler that returns one so the
/// projection logic — and its N+1-avoidance — lives in exactly one place.</summary>
internal static class ProductMapper
{
    public static async Task<ProductDto?> ToDtoAsync(IApplicationDbContext db, Guid productId, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null) return null;

        var productAttributes = await db.ProductAttributes
            .Where(pa => pa.ProductId == productId)
            .Select(pa => new ProductAttributeDto(
                pa.Attribute!.Name,
                pa.Attribute.Code,
                pa.AttributeValueRef != null ? pa.AttributeValueRef.Value : pa.FreeTextValue ?? string.Empty))
            .ToListAsync(cancellationToken);

        var variantIds = product.Variants.Select(v => v.Id).ToList();

        var variantAttributes = await db.ProductAttributes
            .Where(pa => pa.ProductVariantId != null && variantIds.Contains(pa.ProductVariantId.Value))
            .Select(pa => new
            {
                pa.ProductVariantId,
                Dto = new ProductAttributeDto(
                    pa.Attribute!.Name,
                    pa.Attribute.Code,
                    pa.AttributeValueRef != null ? pa.AttributeValueRef.Value : pa.FreeTextValue ?? string.Empty)
            })
            .ToListAsync(cancellationToken);

        var stockByVariant = await db.InventoryItems
            .Where(i => variantIds.Contains(i.ProductVariantId))
            .GroupBy(i => i.ProductVariantId)
            .Select(g => new { ProductVariantId = g.Key, Available = g.Sum(i => i.QuantityOnHand - i.QuantityReserved) })
            .ToDictionaryAsync(x => x.ProductVariantId, x => x.Available, cancellationToken);

        var variantDtos = product.Variants
            .OrderBy(v => v.DisplayName)
            .Select(v => new ProductVariantDto(
                v.Id,
                v.Sku,
                v.DisplayName,
                v.Price ?? product.BasePrice,
                v.CompareAtPrice ?? product.CompareAtPrice,
                stockByVariant.TryGetValue(v.Id, out var stock) ? stock : 0,
                v.ImageUrl,
                v.IsActive,
                variantAttributes.Where(va => va.ProductVariantId == v.Id).Select(va => va.Dto).ToList()))
            .ToList();

        return new ProductDto(
            product.Id,
            product.Name,
            product.Slug,
            product.ShortDescription,
            product.Description,
            product.BasePrice,
            product.CompareAtPrice,
            product.HasVariants,
            product.IsActive,
            product.IsFeatured,
            product.AverageRating,
            product.ReviewCount,
            product.CategoryId,
            product.Category?.Name,
            product.BrandId,
            product.Brand?.Name,
            product.Images.OrderBy(i => i.DisplayOrder).Select(i => i.Url).ToList(),
            productAttributes,
            variantDtos);
    }
}
