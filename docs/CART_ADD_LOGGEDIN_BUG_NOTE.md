# Cart Add API Bug Note (Logged-In User Specific)

Date: 2026-08-04
Environment: Production API (`https://commercecore.onrender.com`)

## Summary
`POST /api/v1/cart` fails for a specific existing user account (admin user), while the same endpoint succeeds for a freshly registered user with the same product variant.

This indicates an account/cart data issue on backend side, not a frontend request formatting issue.

## User Under Test
- Email: `alokchakarverty002@gmail.com`

## Reproduction
1. Login user via OTP (`/api/v1/auth/otp/login`) -> `200`
2. Confirm cart (`GET /api/v1/cart`) -> `200`, empty cart
3. Attempt add to cart (`POST /api/v1/cart`) with valid variant ids

## Results for Existing User
### Variant A
- Product: `Smoked Citrus`
- `productVariantId`: `9b12877d-fb06-4350-84b8-077c771bb6bf`
- Result: `500`
- Message: `An unexpected error occurred.`
- TraceId: `0HNNI1K4Q92DS:0000002D`

### Variant B
- Product: `Essance Of Elegance`
- `productVariantId`: `a8af632b-d807-4bec-851b-c86df1757e5f`
- Result: `422`
- Message: `Only 0 unit(s) of this product are available.`
- TraceId: `0HNNI1K4Q92DS:0000002E`

### Variant C
- Product: `AFTER HOUR`
- `productVariantId`: `9a5e7114-86c0-4b8f-930c-11a5baa062ae`
- Result: `500`
- Message: `An unexpected error occurred.`
- TraceId: `0HNNI1K4Q92DS:0000002F`

## Control Test (Fresh User)
- Create new user via `POST /api/v1/auth/register` -> `200`
- Add same variant (`9b12877d-fb06-4350-84b8-077c771bb6bf`) -> `200`
- Cart response returns line item successfully.

## Why This Points to Backend Data/Domain Issue
- Same endpoint + same payload works for a new account.
- Existing account reproduces `500` on multiple variants.
- Frontend request body is valid and already accepted for another user.

## Suspected Root Cause
Likely user-specific cart/customer data inconsistency (e.g., stale/soft-deleted cart item relation, cart aggregate corruption, or invariant break during cart mutation) causing unhandled exception in add-to-cart command handler.

## Recommended Backend Actions
1. Inspect logs for traceIds:
   - `0HNNI1K4Q92DS:0000002D`
   - `0HNNI1K4Q92DS:0000002F`
2. Query cart + cart items for user `db760e73-b160-4914-a04d-dde1ef74a410` (including soft-deleted rows).
3. Validate cart aggregate rebuild path during add operation.
4. Ensure handler returns domain/validation error instead of generic `500`.
5. Add regression test for existing-user cart mutation with potentially stale historical rows.
