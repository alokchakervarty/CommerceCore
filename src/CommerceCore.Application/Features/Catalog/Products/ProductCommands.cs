using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Application.Features.Catalog.Categories;
using CommerceCore.Contracts.Catalog;
using CommerceCore.Domain.Entities.Catalog;
using CommerceCore.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Catalog.Products;

public record CreateProductCommand(
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
    IReadOnlyList<string>? ImageUrls) : IRequest<ProductDto>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.BasePrice).GreaterThan(0);
        RuleFor(x => x.CompareAtPrice).GreaterThan(0).When(x => x.CompareAtPrice.HasValue);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public CreateProductCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var storeId = _tenant.StoreId;

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId && c.StoreId == storeId, cancellationToken);
        if (!categoryExists)
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.CategoryId)] = new[] { "The specified category does not exist." }
            });

        if (request.BrandId is { } brandId)
        {
            var brandExists = await _db.Brands.AnyAsync(b => b.Id == brandId && b.StoreId == storeId, cancellationToken);
            if (!brandExists)
                throw new ValidationAppException(new Dictionary<string, string[]>
                {
                    [nameof(request.BrandId)] = new[] { "The specified brand does not exist." }
                });
        }

        var product = new Product
        {
            StoreId = storeId,
            Name = request.Name.Trim(),
            Slug = SlugHelper.Generate(request.Name),
            ShortDescription = request.ShortDescription,
            Description = request.Description,
            Sku = request.Sku,
            BasePrice = request.BasePrice,
            CompareAtPrice = request.CompareAtPrice,
            CostPrice = request.CostPrice,
            TrackInventory = request.TrackInventory,
            CategoryId = request.CategoryId,
            BrandId = request.BrandId,
            HasVariants = false,
            IsActive = true
        };

        // Every product gets an implicit default variant so pricing/stock always
        // live at the variant level uniformly, even for products without real options.
        var defaultVariant = new ProductVariant
        {
            Product = product,
            Sku = request.Sku ?? $"SKU-{Guid.NewGuid().ToString()[..8].ToUpperInvariant()}",
            IsDefault = true,
            IsActive = true,
            DisplayName = product.Name
        };
        product.Variants.Add(defaultVariant);

        if (request.ImageUrls != null)
        {
            var order = 0;
            foreach (var url in request.ImageUrls)
            {
                product.Images.Add(new ProductImage
                {
                    Product = product,
                    Url = url,
                    DisplayOrder = order,
                    IsPrimary = order == 0
                });
                order++;
            }
        }

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        return await ProductMapper.ToDtoAsync(_db, product.Id, cancellationToken)
            ?? throw new NotFoundException("Product", product.Id);
    }
}

public record UpdateProductCommand(
    Guid Id,
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
    Guid? BrandId) : IRequest<ProductDto>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.BasePrice).GreaterThan(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public UpdateProductCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FirstOrDefaultAsync(
            p => p.Id == request.Id && p.StoreId == _tenant.StoreId, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
            throw new ValidationAppException(new Dictionary<string, string[]>
            {
                [nameof(request.CategoryId)] = new[] { "The specified category does not exist." }
            });

        product.Name = request.Name.Trim();
        product.ShortDescription = request.ShortDescription;
        product.Description = request.Description;
        product.BasePrice = request.BasePrice;
        product.CompareAtPrice = request.CompareAtPrice;
        product.CostPrice = request.CostPrice;
        product.TrackInventory = request.TrackInventory;
        product.IsActive = request.IsActive;
        product.IsFeatured = request.IsFeatured;
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return await ProductMapper.ToDtoAsync(_db, product.Id, cancellationToken)
            ?? throw new NotFoundException("Product", product.Id);
    }
}

public record DeleteProductCommand(Guid Id) : IRequest;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public DeleteProductCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FirstOrDefaultAsync(
            p => p.Id == request.Id && p.StoreId == _tenant.StoreId, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);

        // Soft delete only (handled automatically by AppDbContext.SaveChangesAsync) —
        // a product that has ever appeared on an Order must never disappear from history.
        product.IsActive = false;
        _db.Products.Remove(product);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
