using System.Reflection;
using CommerceCore.Application.Common.Interfaces;
using CommerceCore.Domain.Entities.Catalog;
using CommerceCore.Domain.Entities.Cms;
using CommerceCore.Domain.Entities.Customers;
using CommerceCore.Domain.Entities.Identity;
using CommerceCore.Domain.Entities.Inventory;
using CommerceCore.Domain.Entities.Marketing;
using CommerceCore.Domain.Entities.Media;
using CommerceCore.Domain.Entities.Notifications;
using CommerceCore.Domain.Entities.Orders;
using CommerceCore.Domain.Entities.Payments;
using CommerceCore.Domain.Entities.Reference;
using CommerceCore.Domain.Entities.Reviews;
using CommerceCore.Domain.Entities.Stores;
using CommerceCore.Domain.Entities.SystemAudit;
using CommerceCore.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;

namespace CommerceCore.Infrastructure.Persistence;

/// <summary>
/// The single EF Core DbContext for the whole platform. Implements
/// IApplicationDbContext so the Application layer depends only on that
/// abstraction, never on this concrete class (Clean Architecture's dependency
/// rule). Every module's entities are registered here; per-entity Fluent API
/// configuration lives in Persistence/Configurations and is picked up
/// automatically via ApplyConfigurationsFromAssembly below.
/// </summary>
public class AppDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    // ---- Identity ----
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    // ---- Stores ----
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<StoreSettings> StoreSettings => Set<StoreSettings>();
    public DbSet<StoreTheme> StoreThemes => Set<StoreTheme>();

    // ---- Catalog ----
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Collection> Collections => Set<Collection>();
    public DbSet<CollectionProduct> CollectionProducts => Set<CollectionProduct>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<AttributeDefinition> AttributeDefinitions => Set<AttributeDefinition>();
    public DbSet<AttributeValue> AttributeValues => Set<AttributeValue>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();

    // ---- Inventory ----
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    // ---- Customers / Orders ----
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    // ---- Payments / Marketing / Reviews ----
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentTransaction> PaymentTransactions => Set<PaymentTransaction>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<CouponUsage> CouponUsages => Set<CouponUsage>();
    public DbSet<Wishlist> Wishlists => Set<Wishlist>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Review> Reviews => Set<Review>();

    // ---- CMS ----
    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Banner> Banners => Set<Banner>();
    public DbSet<Faq> Faqs => Set<Faq>();

    // ---- Reference data ----
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Language> Languages => Set<Language>();
    public DbSet<Tax> Taxes => Set<Tax>();
    public DbSet<ShippingZone> ShippingZones => Set<ShippingZone>();
    public DbSet<ShippingMethod> ShippingMethods => Set<ShippingMethod>();

    // ---- Media / Notifications ----
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // ---- System / Audit (not BaseEntity — write-once logs) ----
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Applies every IEntityTypeConfiguration<T> class found in this assembly
        // (Persistence/Configurations/*.cs) — adding a new entity's configuration
        // file is all that's needed; nothing to register manually here.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global soft-delete filter: every query against a BaseEntity-derived type
        // automatically excludes IsDeleted rows unless explicitly IgnoreQueryFilters().
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var condition = System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false));
                var lambda = System.Linq.Expressions.Expression.Lambda(condition, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    /// <summary>
    /// Central place where every save is intercepted to: stamp audit fields
    /// (CreatedDate/By, ModifiedDate/By), convert hard deletes into soft deletes,
    /// increment the provider-agnostic optimistic-concurrency Version column, and
    /// write an AuditLog row for every changed entity — so no individual handler
    /// has to remember to do any of this itself.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService?.UserId;
        var now = DateTime.UtcNow;
        var auditEntries = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = now;
                    entry.Entity.CreatedBy = userId;
                    entry.Entity.Version = 1;
                    auditEntries.Add(BuildAuditLog(entry, "Created", userId));
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedDate = now;
                    entry.Entity.ModifiedBy = userId;
                    entry.Entity.Version += 1;
                    auditEntries.Add(BuildAuditLog(entry, "Updated", userId));
                    break;

                case EntityState.Deleted:
                    // Convert every hard delete into a soft delete. Callers that truly need
                    // a hard delete (e.g. GDPR erasure) must bypass this via raw SQL/ExecuteDelete.
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedDate = now;
                    entry.Entity.DeletedBy = userId;
                    entry.Entity.Version += 1;
                    auditEntries.Add(BuildAuditLog(entry, "Deleted", userId));
                    break;
            }
        }

        if (auditEntries.Count > 0)
            AuditLogs.AddRange(auditEntries);

        return await base.SaveChangesAsync(cancellationToken);
    }

    private static AuditLog BuildAuditLog(EntityEntry<BaseEntity> entry, string action, Guid? userId)
    {
        string? oldValues = null;
        string? newValues = null;
        var changedProps = new List<string>();

        if (action != "Created")
        {
            var originalValues = new Dictionary<string, object?>();
            foreach (var prop in entry.OriginalValues.Properties)
                originalValues[prop.Name] = entry.OriginalValues[prop];
            oldValues = JsonSerializer.Serialize(originalValues);
        }

        if (action != "Deleted")
        {
            var currentValues = new Dictionary<string, object?>();
            foreach (var prop in entry.CurrentValues.Properties)
                currentValues[prop.Name] = entry.CurrentValues[prop];
            newValues = JsonSerializer.Serialize(currentValues);
        }

        if (action == "Updated")
        {
            foreach (var prop in entry.Properties)
            {
                if (prop.IsModified)
                    changedProps.Add(prop.Metadata.Name);
            }
        }

        return new AuditLog
        {
            UserId = userId,
            EntityName = entry.Entity.GetType().Name,
            EntityId = entry.Entity.Id,
            Action = action,
            OldValuesJson = oldValues,
            NewValuesJson = newValues,
            ChangedPropertiesJson = changedProps.Count > 0 ? JsonSerializer.Serialize(changedProps) : null,
            OccurredAt = DateTime.UtcNow
        };
    }

    public IDbContextTransaction BeginTransaction() => Database.BeginTransaction();
}
