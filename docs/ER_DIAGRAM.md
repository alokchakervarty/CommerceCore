# Entity Relationships

Full column-level detail lives in `database/*.sql`. This is the core commerce flow —
the relationships that matter most for understanding how a checkout actually works.

```mermaid
erDiagram
    STORE ||--o{ CATEGORY : owns
    STORE ||--o{ PRODUCT : owns
    STORE ||--o{ CUSTOMER : owns
    STORE ||--o{ ORDER : owns

    CATEGORY ||--o{ PRODUCT : contains
    BRAND ||--o{ PRODUCT : contains
    PRODUCT ||--o{ PRODUCT_VARIANT : has
    PRODUCT ||--o{ PRODUCT_IMAGE : has
    PRODUCT ||--o{ PRODUCT_ATTRIBUTE : "describes via"
    PRODUCT_VARIANT ||--o{ PRODUCT_ATTRIBUTE : "distinguished by"
    ATTRIBUTE_DEFINITION ||--o{ ATTRIBUTE_VALUE : defines
    ATTRIBUTE_DEFINITION ||--o{ PRODUCT_ATTRIBUTE : "assigned via"

    WAREHOUSE ||--o{ INVENTORY_ITEM : stocks
    PRODUCT_VARIANT ||--o{ INVENTORY_ITEM : "stock tracked by"
    INVENTORY_ITEM ||--o{ STOCK_MOVEMENT : "audited by"

    CUSTOMER ||--o{ ADDRESS : has
    CUSTOMER ||--o{ CART_ITEM : has
    CUSTOMER ||--o{ ORDER : places
    PRODUCT_VARIANT ||--o{ CART_ITEM : "referenced by"

    ORDER ||--o{ ORDER_ITEM : contains
    ORDER ||--o{ PAYMENT : "paid via"
    PAYMENT ||--o{ PAYMENT_TRANSACTION : "ledgered by"
    COUPON ||--o{ COUPON_USAGE : "redeemed via"

    CUSTOMER ||--o{ WISHLIST : has
    WISHLIST ||--o{ WISHLIST_ITEM : contains
    PRODUCT ||--o{ REVIEW : receives

    USER ||--o{ USER_ROLE : has
    ROLE ||--o{ USER_ROLE : "assigned via"
    ROLE ||--o{ ROLE_PERMISSION : has
    PERMISSION ||--o{ ROLE_PERMISSION : "granted via"
    USER ||--o{ REFRESH_TOKEN : has
    USER ||--o| CUSTOMER : "optionally becomes"
```

## Full table list by module

| Module | Tables |
|---|---|
| Identity | Users, Roles, Permissions, RolePermissions, UserRoles, RefreshTokens |
| Stores | Stores, StoreSettings, StoreThemes |
| Catalog | Categories, Brands, Collections, CollectionProducts, Products, ProductVariants, ProductImages, Attributes, AttributeValues, ProductAttributes |
| Inventory | Warehouses, Inventory, StockMovements |
| Customers/Orders | Customers, Addresses, CartItems, Orders, OrderItems |
| Payments/Marketing | Payments, PaymentTransactions, Coupons, CouponUsage, Wishlists, WishlistItems, Reviews |
| CMS | Blogs, BlogCategories, Pages, Menus, MenuItems, Banners, Faqs |
| Reference | Countries, States, Cities, Currencies, Languages, Taxes, ShippingZones, ShippingMethods |
| Media/Notifications | Media, NotificationTemplates, Notifications |
| System/Audit | SystemSettings, EmailTemplates, AuditLogs, ActivityLogs |

**56 tables total.**

## The dynamic attribute engine, concretely

The same three tables — `Attributes`, `AttributeValues`, `ProductAttributes` — serve
every vertical without a schema change:

| Vertical | Example `Attributes` rows | Variant dimension? |
|---|---|---|
| Perfume | Volume, Fragrance Notes | Volume: yes · Notes: no (descriptive) |
| Bedsheets | Material, Thread Count, Size, Color | Size/Color: yes · Material/Thread Count: no |
| Electronics | RAM, Storage, Processor | RAM/Storage: yes · Processor: no |
| Jewelry | Metal Type, Gemstone, Ring Size | Ring Size: yes · Metal/Gemstone: no |

A store admin creates `AttributeDefinition` rows for their vertical (via
`/api/v1/attributes`), predefined choices as `AttributeValue` rows (e.g. "50ml",
"Red"), then assigns them to a `Product` (descriptive) or `ProductVariant` (a
purchasing dimension) via `ProductAttribute` rows. No code changes, ever.
