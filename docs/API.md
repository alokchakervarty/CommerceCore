# API Reference

Swagger (`/swagger` in Development) is the authoritative, always-current contract —
every request/response schema, every route, generated directly from the code. This
page is a quick-start with runnable examples.

**Every request needs an `X-Store-Id` header.** See the main README's "Multi-tenancy"
section for why.

## Example flow (curl)

```bash
BASE=http://localhost:8080/api/v1
STORE_ID=<your seeded store's GUID — see README>

# 1. Log in as the seeded admin
curl -s -X POST $BASE/auth/login \
  -H "X-Store-Id: $STORE_ID" -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin@123"}'
# → { "data": { "accessToken": "...", "refreshToken": "...", ... } }

TOKEN=<accessToken from above>

# 2. Create a category
curl -s -X POST $BASE/categories \
  -H "X-Store-Id: $STORE_ID" -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"Fragrances","description":"All perfumes and colognes"}'

CATEGORY_ID=<id from response>

# 3. Create a product
curl -s -X POST $BASE/products \
  -H "X-Store-Id: $STORE_ID" -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d "{\"name\":\"Midnight Oud\",\"basePrice\":49.99,\"categoryId\":\"$CATEGORY_ID\",\"trackInventory\":true}"

# 4. Browse products (public, no auth needed)
curl -s "$BASE/products?search=oud&pageNumber=1&pageSize=10" -H "X-Store-Id: $STORE_ID"

# 5. Register a shopper and add to cart
curl -s -X POST $BASE/auth/register \
  -H "X-Store-Id: $STORE_ID" -H "Content-Type: application/json" \
  -d '{"firstName":"Jane","lastName":"Doe","email":"jane@example.com","password":"Password1"}'

SHOPPER_TOKEN=<accessToken from above>
VARIANT_ID=<a productVariant id from the product's GetById response>

curl -s -X POST $BASE/cart \
  -H "X-Store-Id: $STORE_ID" -H "Authorization: Bearer $SHOPPER_TOKEN" -H "Content-Type: application/json" \
  -d "{\"productVariantId\":\"$VARIANT_ID\",\"quantity\":2}"

# 6. Checkout (needs a saved address first — see AddressesController-equivalent;
#    addresses in this build are managed via the Customer's Addresses, added during
#    checkout flows in your frontend, or directly for now via the database)
```

## Response envelope

Every response, success or failure, has this shape:

```json
{
  "success": true,
  "message": "Request completed successfully.",
  "data": { },
  "errors": [],
  "traceId": "0HN...",
  "timestamp": "2026-07-18T12:00:00Z"
}
```

Failures set `"success": false`, populate `"errors"`, and use the appropriate HTTP
status code (400 for validation, 401/403 for auth, 404 for not found, 409 for
conflicts, 422 for business rule violations, 500 for unexpected errors).

## Pagination

List endpoints accept `pageNumber` (default 1) and `pageSize` (default 20, capped at
100), and return:

```json
{
  "items": [ ],
  "totalCount": 142,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 8,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

(wrapped inside `data`, per the envelope above).

See the main `README.md` for the full route list.
