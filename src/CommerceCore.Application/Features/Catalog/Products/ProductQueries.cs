using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Catalog;
using CommerceCore.Shared.Exceptions;
using CommerceCore.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Catalog.Products;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto>;

public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    private readonly IApplicationDbContext _db;

    public GetProductByIdQueryHandler(IApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        => await ProductMapper.ToDtoAsync(_db, request.Id, cancellationToken)
            ?? throw new NotFoundException("Product", request.Id);
}

public record GetProductsQuery(ProductListQuery Query) : IRequest<PagedResult<ProductDto>>;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetProductsQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var q = request.Query;
        var storeId = _tenant.StoreId;

        var products = _db.Products.Where(p => p.StoreId == storeId && p.IsActive);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var search = q.Search.Trim().ToLower();
            products = products.Where(p => p.Name.ToLower().Contains(search)
                || (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(search)));
        }

        if (q.CategoryId.HasValue)
            products = products.Where(p => p.CategoryId == q.CategoryId.Value);

        if (q.BrandId.HasValue)
            products = products.Where(p => p.BrandId == q.BrandId.Value);

        if (q.MinPrice.HasValue)
            products = products.Where(p => p.BasePrice >= q.MinPrice.Value);

        if (q.MaxPrice.HasValue)
            products = products.Where(p => p.BasePrice <= q.MaxPrice.Value);

        products = q.SortBy switch
        {
            "price_asc" => products.OrderBy(p => p.BasePrice),
            "price_desc" => products.OrderByDescending(p => p.BasePrice),
            "name_asc" => products.OrderBy(p => p.Name),
            "rating_desc" => products.OrderByDescending(p => p.AverageRating),
            _ => products.OrderByDescending(p => p.CreatedDate)
        };

        var totalCount = await products.CountAsync(cancellationToken);

        var page = q.PageNumber < 1 ? 1 : q.PageNumber;
        var pageSize = q.PageSize is < 1 or > 100 ? 20 : q.PageSize;

        var pageIds = await products
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        // Reuse the shared mapper per-item so the paged list carries exactly the same
        // shape (variants, images, attributes) as GetProductByIdQuery.
        var items = new List<ProductDto>(pageIds.Count);
        foreach (var id in pageIds)
        {
            var dto = await ProductMapper.ToDtoAsync(_db, id, cancellationToken);
            if (dto != null) items.Add(dto);
        }

        return PagedResult<ProductDto>.Create(items, totalCount, page, pageSize);
    }
}
