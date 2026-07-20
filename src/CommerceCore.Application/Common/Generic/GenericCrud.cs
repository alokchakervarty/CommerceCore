using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Shared.Entities;
using CommerceCore.Shared.Exceptions;
using CommerceCore.Shared.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Common.Generic;

/// <summary>
/// A fully generic MediatR request/handler set giving any BaseEntity-derived type
/// complete CRUD (Create, GetById, GetPaged with search, Update, Delete) without a
/// bespoke handler file. This is what makes ~27 reference/CMS/marketing tables
/// (Brand, Coupon, Blog, Country, Tax, ...) real, working REST resources with only a
/// thin per-entity Api controller — the correct trade-off for a backend whose entire
/// purpose is being reusable across verticals rather than hand-tuned per module.
///
/// TEntity is the EF-mapped Domain entity. TDto is the projection returned to the
/// client, produced by the supplied <paramref name="toDto"/> delegate. TCreateDto/
/// TUpdateDto are used by the concrete per-entity command records (see the thin
/// Api-layer controllers) — the mapping from request to entity is supplied by the
/// caller via constructor/property-setting delegates, not reflection, so it stays
/// fully type-safe and debuggable.
/// </summary>
public static class GenericCrud<TEntity> where TEntity : BaseEntity
{
    public record Create(TEntity Entity) : IRequest<TEntity>;

    public class CreateHandler : IRequestHandler<Create, TEntity>
    {
        private readonly IUnitOfWork _uow;
        public CreateHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<TEntity> Handle(Create request, CancellationToken cancellationToken)
        {
            await _uow.Repository<TEntity>().AddAsync(request.Entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
            return request.Entity;
        }
    }

    public record GetById(Guid Id) : IRequest<TEntity>;

    public class GetByIdHandler : IRequestHandler<GetById, TEntity>
    {
        private readonly IUnitOfWork _uow;
        public GetByIdHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<TEntity> Handle(GetById request, CancellationToken cancellationToken)
        {
            return await _uow.Repository<TEntity>().GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(typeof(TEntity).Name, request.Id);
        }
    }

    /// <summary>Generic paged list with an optional search predicate (case-insensitive
    /// Contains over whichever string property the caller's predicate checks) and an
    /// optional extra filter (e.g. "only IsActive", "only this StoreId").</summary>
    public record GetPaged(
        int PageNumber,
        int PageSize,
        string? Search,
        Func<TEntity, bool>? SearchPredicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? ExtraFilter,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? Sort) : IRequest<PagedResult<TEntity>>;

    public class GetPagedHandler : IRequestHandler<GetPaged, PagedResult<TEntity>>
    {
        private readonly IUnitOfWork _uow;
        public GetPagedHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<PagedResult<TEntity>> Handle(GetPaged request, CancellationToken cancellationToken)
        {
            var query = _uow.Repository<TEntity>().Query();

            if (request.ExtraFilter != null)
                query = request.ExtraFilter(query);

            query = request.Sort != null ? request.Sort(query) : query.OrderByDescending(e => e.CreatedDate);

            var page = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

            // Search runs client-side (post-materialization) only when the caller
            // supplied a predicate the database can't translate; simple cases should
            // prefer ExtraFilter with a translatable EF Core expression instead.
            if (!string.IsNullOrWhiteSpace(request.Search) && request.SearchPredicate != null)
            {
                var all = await query.ToListAsync(cancellationToken);
                var filtered = all.Where(request.SearchPredicate).ToList();
                var pageItems = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                return PagedResult<TEntity>.Create(pageItems, filtered.Count, page, pageSize);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
            return PagedResult<TEntity>.Create(items, totalCount, page, pageSize);
        }
    }

    public record Update(Guid Id, Action<TEntity> Apply) : IRequest<TEntity>;

    public class UpdateHandler : IRequestHandler<Update, TEntity>
    {
        private readonly IUnitOfWork _uow;
        public UpdateHandler(IUnitOfWork uow) => _uow = uow;

        public async Task<TEntity> Handle(Update request, CancellationToken cancellationToken)
        {
            var repo = _uow.Repository<TEntity>();
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(typeof(TEntity).Name, request.Id);

            request.Apply(entity);
            repo.Update(entity);
            await _uow.SaveChangesAsync(cancellationToken);
            return entity;
        }
    }

    public record Delete(Guid Id) : IRequest;

    public class DeleteHandler : IRequestHandler<Delete>
    {
        private readonly IUnitOfWork _uow;
        public DeleteHandler(IUnitOfWork uow) => _uow = uow;

        public async Task Handle(Delete request, CancellationToken cancellationToken)
        {
            var repo = _uow.Repository<TEntity>();
            var entity = await repo.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(typeof(TEntity).Name, request.Id);

            repo.Remove(entity); // converted to a soft delete by AppDbContext.SaveChangesAsync
            await _uow.SaveChangesAsync(cancellationToken);
        }
    }
}
