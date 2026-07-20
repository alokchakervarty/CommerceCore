using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Catalog;
using CommerceCore.Shared.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Catalog.Categories;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetCategoryByIdQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(
            c => c.Id == request.Id && c.StoreId == _tenant.StoreId, cancellationToken)
            ?? throw new NotFoundException("Category", request.Id);

        return CreateCategoryCommandHandler.ToDto(category);
    }
}

/// <summary>Returns the full category tree for the current store, flattened with
/// ParentCategoryId so the client can reconstruct hierarchy however it needs.</summary>
public record GetCategoriesQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public GetCategoriesQueryHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Categories.Where(c => c.StoreId == _tenant.StoreId);
        if (request.ActiveOnly)
            query = query.Where(c => c.IsActive);

        var categories = await query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name).ToListAsync(cancellationToken);
        return categories.Select(CreateCategoryCommandHandler.ToDto).ToList();
    }
}
