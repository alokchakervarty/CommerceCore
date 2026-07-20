namespace CommerceCore.Shared.Entities;

/// <summary>
/// Base class for every entity in the system. Provides a UUID primary key,
/// full audit trail (created/modified/deleted by + when), soft-delete support,
/// and an optimistic-concurrency row version token.
/// Every table in the schema maps to a class deriving from this.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }
    public Guid? ModifiedBy { get; set; }

    public DateTime? DeletedDate { get; set; }
    public Guid? DeletedBy { get; set; }

    public bool IsDeleted { get; set; }

    /// <summary>
    /// Optimistic concurrency token, incremented manually by Infrastructure's DbContext
    /// on every SaveChanges() call and configured as a Fluent API ConcurrencyToken.
    /// Deliberately application-managed rather than a database-native mechanism (e.g.
    /// PostgreSQL's xmin or SQL Server's ROWVERSION) so the same Domain model keeps
    /// working unmodified if the underlying database provider ever changes — EF Core
    /// still detects and throws DbUpdateConcurrencyException on a stale write exactly
    /// as it would with a database-native token.
    /// </summary>
    public int Version { get; set; }
}

/// <summary>
/// Marker interface applied to every entity that belongs to a specific store,
/// enabling multi-tenant query filtering at the DbContext level.
/// </summary>
public interface IStoreScoped
{
    Guid StoreId { get; set; }
}
