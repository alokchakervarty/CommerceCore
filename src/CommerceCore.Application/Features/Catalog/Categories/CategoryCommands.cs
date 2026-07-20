using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Contracts.Catalog;
using CommerceCore.Domain.Entities.Catalog;
using CommerceCore.Shared.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Features.Catalog.Categories;

public record CreateCategoryCommand(string Name, string? Description, string? ImageUrl, Guid? ParentCategoryId)
    : IRequest<CategoryDto>;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public CreateCategoryCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var storeId = _tenant.StoreId;

        if (request.ParentCategoryId is { } parentId)
        {
            var parentExists = await _db.Categories.AnyAsync(c => c.Id == parentId && c.StoreId == storeId, cancellationToken);
            if (!parentExists)
                throw new ValidationAppException(new Dictionary<string, string[]>
                {
                    [nameof(request.ParentCategoryId)] = new[] { "The specified parent category does not exist." }
                });
        }

        var category = new Category
        {
            StoreId = storeId,
            Name = request.Name.Trim(),
            Slug = SlugHelper.Generate(request.Name),
            Description = request.Description,
            ImageUrl = request.ImageUrl,
            ParentCategoryId = request.ParentCategoryId,
            IsActive = true
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);

        return ToDto(category);
    }

    internal static CategoryDto ToDto(Category c) =>
        new(c.Id, c.Name, c.Slug, c.Description, c.ImageUrl, c.ParentCategoryId, c.IsActive);
}

public record UpdateCategoryCommand(Guid Id, string Name, string? Description, string? ImageUrl, Guid? ParentCategoryId, bool IsActive)
    : IRequest<CategoryDto>;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public UpdateCategoryCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(
            c => c.Id == request.Id && c.StoreId == _tenant.StoreId, cancellationToken)
            ?? throw new NotFoundException("Category", request.Id);

        if (request.ParentCategoryId == category.Id)
            throw new BusinessRuleException("A category cannot be its own parent.");

        category.Name = request.Name.Trim();
        category.Description = request.Description;
        category.ImageUrl = request.ImageUrl;
        category.ParentCategoryId = request.ParentCategoryId;
        category.IsActive = request.IsActive;

        await _db.SaveChangesAsync(cancellationToken);

        return CreateCategoryCommandHandler.ToDto(category);
    }
}

public record DeleteCategoryCommand(Guid Id) : IRequest;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentTenantService _tenant;

    public DeleteCategoryCommandHandler(IApplicationDbContext db, ICurrentTenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(
            c => c.Id == request.Id && c.StoreId == _tenant.StoreId, cancellationToken)
            ?? throw new NotFoundException("Category", request.Id);

        var hasChildren = await _db.Categories.AnyAsync(c => c.ParentCategoryId == category.Id, cancellationToken);
        if (hasChildren)
            throw new BusinessRuleException("This category has subcategories and cannot be deleted. Reassign or delete them first.");

        var hasProducts = await _db.Products.AnyAsync(p => p.CategoryId == category.Id, cancellationToken);
        if (hasProducts)
            throw new BusinessRuleException("This category has products assigned and cannot be deleted.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Shared slug-generation helper for every catalog entity with a Slug column.</summary>
internal static class SlugHelper
{
    public static string Generate(string input)
    {
        var slug = System.Text.RegularExpressions.Regex.Replace(input.Trim().ToLowerInvariant(), @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');
        return $"{slug}-{Guid.NewGuid().ToString()[..6]}";
    }
}
