# CommerceCore

A reusable, headless e-commerce backend built in .NET 8. The same backend — same
database schema, same API — serves a perfume store, a bedsheet store, a furniture
store, an electronics store, or any other vertical. The frontend changes per client;
this backend never does. Vertical-specific product data (Volume, Thread Count, RAM,
Fragrance Notes, ...) is expressed entirely through the dynamic Attribute engine in
the Catalog module — never as hardcoded columns.

## Tech stack

- **.NET 8** / ASP.NET Core Web API
- **PostgreSQL** via Entity Framework Core 8 (Npgsql)
- **JWT** authentication with refresh-token rotation
- **MediatR** (CQRS), **FluentValidation**, **AutoMapper**
- **Serilog** (console + rolling file), **Swagger/OpenAPI**, **API versioning**
- **Repository Pattern + Unit of Work** for the generic CRUD pipeline
- **Clean Architecture**: Domain → Application → Infrastructure → Api, plus a
  standalone Shared kernel and Contracts (DTO) project
- **Docker** / docker-compose
- **xUnit** for tests

## Solution structure

```
CommerceCore.sln
src/
  CommerceCore.Shared          # BaseEntity, ApiResponse envelope, Result<T>, exceptions — zero dependencies
  CommerceCore.Domain           # 56 entities across 10 modules, zero infrastructure dependencies
  CommerceCore.Contracts        # Request/response DTOs for the hand-written modules (Auth, Catalog, Cart, Orders)
  CommerceCore.Application      # MediatR handlers, FluentValidation, the generic CRUD pipeline
  CommerceCore.Infrastructure   # EF Core DbContext + configurations, JWT/password services, repositories
  CommerceCore.Api              # Controllers, Program.cs, middleware
tests/
  CommerceCore.Tests            # xUnit
database/                       # Standalone SQL schema + seed data (see database/README.md)
docker/                         # Dockerfile + docker-compose.yml
docs/                           # Architecture, ER overview, deployment guide
```

## Quick start (Docker — fastest path)

```bash
cd docker
docker compose up --build
```

This starts PostgreSQL and the API together. On first run in the Development
environment, the API automatically applies EF Core migrations and seeds:
- A default Store
- Admin/Customer roles
- A seeded admin user: **admin@example.com / Admin@123** (change this immediately in any real deployment)
- A handful of reference Countries

Once running: **http://localhost:8080/swagger**

## Quick start (without Docker)

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download), PostgreSQL 15+ running locally.

```bash
# 1. Restore
dotnet restore CommerceCore.sln

# 2. Configure your connection string and JWT key
#    Edit src/CommerceCore.Api/appsettings.json, or better, use user-secrets:
cd src/CommerceCore.Api
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=commercecore_db;Username=postgres;Password=postgres"
dotnet user-secrets set "Jwt:Key" "<a long random string, 32+ characters>"

# 3. Install the EF Core CLI tool (one-time, per machine)
dotnet tool install --global dotnet-ef

# 4. Create and apply the initial migration
cd ../CommerceCore.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../CommerceCore.Api
dotnet ef database update --startup-project ../CommerceCore.Api

# 5. Run
cd ../CommerceCore.Api
dotnet run
```

Swagger opens at `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`).

Alternatively, apply `database/run_all.sql` directly against a fresh PostgreSQL
database instead of running EF migrations — see `database/README.md`.

## Multi-tenancy: the X-Store-Id header

**Every request must include an `X-Store-Id` header** with the target Store's GUID.
This is a deliberate headless-backend design choice: unlike a single-tenant app that
can infer its store from a session or subdomain, this backend explicitly requires
every caller — web, mobile, or third-party integration — to state which store's data
it wants, since one deployment serves unlimited tenants simultaneously.

Get your seeded store's ID once, after first run:
```sql
SELECT "Id", "Name" FROM "Stores" WHERE "Slug" = 'default';
```
Swagger's UI includes an `X-Store-Id` field on every endpoint so you can set it once per session.

## Authentication flow

1. `POST /api/v1/auth/register` or `/login` → returns `accessToken` + `refreshToken`
2. Send `Authorization: Bearer {accessToken}` on subsequent requests
3. `POST /api/v1/auth/refresh` when the access token expires (rotates the refresh token too)
4. `POST /api/v1/auth/logout` revokes the current refresh token

Roles are store-scoped (`Admin`, `Customer` seeded by default; add more via the
`Roles`/`Permissions`/`RolePermissions` tables).

## API surface

Hand-written modules with full business logic (search, checkout, stock reservation, etc.):

| Module | Base route |
|---|---|
| Auth | `/api/v1/auth` |
| Categories | `/api/v1/categories` |
| Products | `/api/v1/products` |
| Cart | `/api/v1/cart` |
| Orders | `/api/v1/orders` |

Generic CRUD modules (full REST — GET list/by-id, POST, PUT, DELETE — via a reusable
MediatR pipeline, since these ~27 tables are simple admin-managed reference/CMS/
marketing data with no business-logic projection needed):

`brands`, `collections`, `attributes`, `attribute-values`, `warehouses`, `coupons`,
`wishlists`, `reviews`, `blogs`, `blog-categories`, `pages`, `menus`, `menu-items`,
`banners`, `faqs`, `countries`, `states`, `cities`, `currencies`, `languages`, `taxes`,
`shipping-zones`, `shipping-methods`, `media`, `notification-templates`,
`system-settings`, `email-templates` — all under `/api/v1/{resource}`.

Every response — success or failure — is wrapped in the same envelope:
```json
{ "success": true, "message": "...", "data": { ... }, "errors": [], "traceId": "...", "timestamp": "..." }
```

See `docs/API.md` for more, and Swagger (`/swagger`) for the live, authoritative
contract with request/response schemas.

## Running tests

```bash
dotnet test CommerceCore.sln
```

## Further reading

- `docs/ARCHITECTURE.md` — Clean Architecture layer breakdown and key design decisions
- `docs/ER_DIAGRAM.md` — entity relationships by module
- `docs/DEPLOYMENT.md` — production deployment checklist
- `database/README.md` — standalone SQL schema details

## Honest scope notes

This was generated as a large, single build. A few things are deliberately
out of scope or simplified, flagged here rather than silently:

- **Tax and shipping-rate calculation** are modeled (Tax, ShippingZone, ShippingMethod
  entities exist and are fully CRUD-able) but Checkout doesn't yet *apply* them —
  `Order.TaxAmount`/`ShippingAmount` are computed as 0. Wire up the calculation in
  `CheckoutCommandHandler` once you decide on tax/shipping rules for your vertical.
- **Payment gateway integration** (Stripe/Razorpay/etc.) isn't implemented — the
  `Payment`/`PaymentTransaction` schema and `PaymentMethod` field on `Order` are ready
  for it; add a webhook endpoint and gateway SDK call when you pick a provider.
- A few foreign keys (e.g. `CouponUsage.OrderId`, `Review.CustomerId`/`OrderId`) are
  omitted in the standalone SQL scripts as a deliberate simplification — see
  `database/README.md`.
- The project wasn't (and, in this environment, couldn't be) compiled with `dotnet build`
  — there's no network access to NuGet here. The code follows correct, idiomatic
  EF Core 8 / ASP.NET Core 8 / MediatR 12 patterns throughout, but run
  `dotnet restore && dotnet build` as your first step locally to catch anything.
