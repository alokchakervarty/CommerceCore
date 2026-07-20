using CommerceCore.Shared.Entities;

namespace CommerceCore.Application.Common.Interfaces;

/// <summary>
/// Generic Repository Pattern abstraction over a single entity type. Used by the
/// reusable CQRS CRUD pipeline (Common/Generic) so the ~40 reference, CMS, and
/// marketing entities get full REST CRUD without hand-written handlers per entity.
/// Bespoke, business-logic-heavy modules (Auth, Catalog checkout, Orders) instead
/// inject IApplicationDbContext directly for full LINQ control — this repository
/// is deliberately the "simple case" abstraction, not a replacement for it.
/// </summary>
public interface IGenericRepository<TEntity> where TEntity : BaseEntity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Composable queryable for filtering/sorting/paging in the handler
    /// that calls it. The soft-delete global query filter is already applied.</summary>
    IQueryable<TEntity> Query();

    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);

    /// <summary>Marks the entity for removal. Infrastructure's DbContext converts
    /// this into a soft delete (IsDeleted = true) on SaveChanges, never a hard DELETE.</summary>
    void Remove(TEntity entity);
}

/// <summary>
/// Unit of Work Pattern: coordinates one or more IGenericRepository instances against
/// a single database transaction/SaveChanges call, so a handler that touches several
/// entities commits them all atomically with one round trip.
/// </summary>
public interface IUnitOfWork
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
