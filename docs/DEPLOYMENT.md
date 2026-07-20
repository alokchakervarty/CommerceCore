# Deployment Guide

## Before deploying anywhere real

1. **Change `Jwt:Key`.** The value in `appsettings.json` is a placeholder. Generate a
   real secret (32+ random characters) and set it via environment variable
   (`Jwt__Key`) or a secrets manager — never commit it.
2. **Change the seeded admin password.** `admin@example.com` / `Admin@123` is a
   development convenience. Log in once, then either add a change-password endpoint
   or update the `Users.PasswordHash` column directly with a freshly generated BCrypt hash.
3. **Set `ASPNETCORE_ENVIRONMENT=Production`** so `Program.cs` stops auto-applying
   migrations and auto-seeding on every startup — run `dotnet ef database update`
   explicitly as a deploy step instead.
4. **Set `RequireHttpsMetadata = true`** for JWT validation — this happens
   automatically outside Development, but confirm your reverse proxy/load balancer
   actually terminates TLS.
5. **Review CORS `AllowedOrigins`** in `appsettings.json` — the default includes
   localhost dev ports only.

## Docker deployment

```bash
cd docker
docker compose up --build -d
```

For a managed database (RDS, Cloud SQL, etc.) instead of the bundled Postgres
container, remove the `postgres` service from `docker-compose.yml` and point
`ConnectionStrings__DefaultConnection` at your managed instance.

## Environment variables (override any appsettings.json value)

| Variable | Purpose |
|---|---|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Jwt__Key` | JWT signing secret (32+ chars) |
| `Jwt__Issuer` / `Jwt__Audience` | JWT issuer/audience claims |
| `Jwt__AccessTokenExpiryMinutes` | Access token lifetime |
| `Cors__AllowedOrigins__0`, `__1`, ... | Allowed CORS origins |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Staging` / `Production` |

## Database migrations in production

Never rely on the automatic dev-time migration in `Program.cs` for production. Instead:

```bash
cd src/CommerceCore.Infrastructure
dotnet ef migrations bundle --startup-project ../CommerceCore.Api -o efbundle
# Run efbundle against the production connection string as an explicit deploy step,
# before the new API version starts receiving traffic.
```

## Health checks

`GET /health` reports database connectivity — point your load balancer's health
check here.

## Scaling notes

- The API is stateless (JWT auth, no server-side session) — safe to run multiple
  instances behind a load balancer.
- `ICurrentTenantService` reads `X-Store-Id` per-request — no per-tenant deployment
  needed; one instance serves unlimited stores.
- `AppDbContext` is registered scoped (standard EF Core lifetime) — no changes needed
  for horizontal scaling.
- For high write volume on `AuditLogs` (every Create/Update/Delete writes one row),
  consider partitioning that table by `OccurredAt` once volume warrants it.
