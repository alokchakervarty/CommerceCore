using CommerceCore.Domain.Entities.Catalog;
using CommerceCore.Domain.Entities.Customers;
using CommerceCore.Domain.Entities.Identity;
using CommerceCore.Domain.Entities.Inventory;
using CommerceCore.Domain.Entities.Marketing;
using CommerceCore.Domain.Entities.Orders;
using CommerceCore.Domain.Entities.Payments;
using CommerceCore.Domain.Entities.Reviews;
using CommerceCore.Domain.Entities.Stores;
using Microsoft.EntityFrameworkCore;

namespace CommerceCore.Application.Common.Interfaces;

/// <summary>
/// The Application layer depends on this abstraction, never on the concrete
/// Infrastructure DbContext — keeping the dependency arrow pointing inward as
/// Clean Architecture requires. Infrastructure's AppDbContext implements it.
/// Exposes only DbSets Application handlers actually query/mutate directly;
/// everything else is reached via navigation properties.
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Store> Stores { get; }

    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<OtpCode> OtpCodes { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }

    DbSet<Category> Categories { get; }
    DbSet<Brand> Brands { get; }
    DbSet<Collection> Collections { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<AttributeDefinition> AttributeDefinitions { get; }
    DbSet<AttributeValue> AttributeValues { get; }
    DbSet<ProductAttribute> ProductAttributes { get; }

    DbSet<Warehouse> Warehouses { get; }
    DbSet<InventoryItem> InventoryItems { get; }
    DbSet<StockMovement> StockMovements { get; }

    DbSet<Customer> Customers { get; }
    DbSet<Address> Addresses { get; }
    DbSet<CartItem> CartItems { get; }

    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }

    DbSet<Payment> Payments { get; }
    DbSet<PaymentTransaction> PaymentTransactions { get; }

    DbSet<Coupon> Coupons { get; }
    DbSet<CouponUsage> CouponUsages { get; }
    DbSet<Wishlist> Wishlists { get; }
    DbSet<WishlistItem> WishlistItems { get; }

    DbSet<Review> Reviews { get; }

    /// <summary>Generic accessor used by the reusable CQRS CRUD pipeline (see
    /// Common/Generic) to reach any of the ~40 reference/CMS/marketing entities
    /// that don't warrant a dedicated bespoke DbSet property above.</summary>
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction BeginTransaction();
}
