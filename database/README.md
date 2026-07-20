# CommerceCore Database Scripts

Hand-maintained PostgreSQL DDL mirroring the EF Core model exactly, provided per the
project's requirement for standalone SQL scripts (not just EF migrations).

**In normal development, prefer EF Core migrations** (`dotnet ef database update` from
`CommerceCore.Infrastructure`) — they're the living source of truth and stay in sync
with the C# model automatically. These SQL files exist for:
- Reviewing the entire schema in one place without a running database
- Provisioning environments where the `dotnet-ef` CLI isn't available
- Onboarding/documentation

## Files, in dependency order

| File | Contents |
|---|---|
| `01_identity_and_stores.sql` | Stores (tenant root), Users, Roles, Permissions, RefreshTokens |
| `02_catalog.sql` | Categories, Brands, Collections, Products, ProductVariants, ProductImages, the dynamic Attribute engine |
| `03_inventory.sql` | Warehouses, Inventory, StockMovements |
| `04_customers_orders.sql` | Customers, Addresses, CartItems, Orders, OrderItems |
| `05_payments_marketing.sql` | Payments, PaymentTransactions, Coupons, CouponUsage, Wishlists, Reviews |
| `06_cms.sql` | Blogs, BlogCategories, Pages, Menus, MenuItems, Banners, Faqs |
| `07_reference.sql` | Countries, States, Cities, Currencies, Languages, Taxes, ShippingZones, ShippingMethods |
| `08_media_notifications_system.sql` | Media, NotificationTemplates, Notifications, SystemSettings, EmailTemplates, AuditLogs, ActivityLogs |
| `99_seed_data.sql` | Reference countries/currencies/languages + a default Store and seeded Admin user |

## Running against a fresh database

```bash
createdb commercecore_db
psql -U postgres -d commercecore_db -f run_all.sql
```

Or apply files individually in the numbered order above.

## Seeded admin login

After `99_seed_data.sql` runs (or the API's automatic dev-time seeding — see
`DbSeeder.cs`), you can log in with:
- **Email:** `admin@example.com`
- **Password:** `Admin@123`

**Change this password before any real deployment.**

## Notes on conventions

- Every table's primary key is a UUID (`gen_random_uuid()`, requires the `pgcrypto`
  extension, enabled at the top of `01_identity_and_stores.sql`).
- Every table except `AuditLogs`/`ActivityLogs` carries the standard audit columns:
  `CreatedDate`, `CreatedBy`, `ModifiedDate`, `ModifiedBy`, `DeletedDate`, `DeletedBy`,
  `IsDeleted`, `Version`. `AuditLogs`/`ActivityLogs` are intentionally write-once logs
  and never derive from that pattern — an audit log must never itself be audited.
- **Soft delete everywhere.** Nothing in this schema is ever physically `DELETE`d by
  the application; `IsDeleted` is set instead. Application code always filters
  `WHERE "IsDeleted" = false` (EF Core does this automatically via a global query filter).
- A few intentionally-omitted foreign keys (e.g. `CouponUsage.OrderId`,
  `Reviews.CustomerId`/`OrderId`) are a deliberate simplification to avoid tight
  coupling across module boundaries in this generated schema — add them if your
  deployment needs strict referential integrity there.
