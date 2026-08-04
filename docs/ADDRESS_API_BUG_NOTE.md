# Address API Bug Note

Date: 2026-08-04
Environment: Production API (`https://commercecore.onrender.com`)
Reporter: Frontend integration test from TwoSoul app

## Summary
Address endpoints are not usable from the current frontend contract.

- `GET /api/v1/addresses` returns `500` for an authenticated admin user.
- `POST /api/v1/addresses` has inconsistent request contract behavior:
  - Flat payload with `type: 2` returns `400` (expects string, also says request wrapper required).
  - Flat payload with `type: "Both"` returns `500`.
  - Wrapped payload `{ request: { ... } }` returns `400` saying required fields are missing, implying binding mismatch.

## Impact
- Users cannot load saved addresses in account/checkout flow.
- Users cannot create shipping addresses.
- Checkout readiness stays blocked due to missing address creation.

## Tested Authentication Flow
1. `POST /api/v1/auth/otp/request` with:
   ```json
   {
     "identifier": "alokchakarverty002@gmail.com",
     "channel": "Email"
   }
   ```
   Result: `200 OK`

2. `POST /api/v1/auth/otp/login` with:
   ```json
   {
     "identifier": "alokchakarverty002@gmail.com",
     "channel": "Email",
     "code": "123456"
   }
   ```
   Result: `200 OK`, bearer token issued.

3. `GET /api/v1/auth/me` with bearer token
   Result: `200 OK`

This confirms auth is valid for subsequent address tests.

## Reproduction Details
### Case A: Address list
Request:
- `GET /api/v1/addresses`
- Header: `Authorization: Bearer <token>`

Actual:
- Status: `500`
- Body:
  - `success: false`
  - `message: "An unexpected error occurred."`
  - `traceId: "0HNNHST8RG2T1:00000016"`
  - `timestamp: "2026-08-04T05:55:05.19751Z"`

Expected:
- `200` with address list (or empty list)

### Case B: Create address (flat payload, type numeric)
Request:
- `POST /api/v1/addresses`
- Header: `Authorization: Bearer <token>`, `Content-Type: application/json`
- Payload:
  ```json
  {
    "fullName": "API Test User",
    "phoneNumber": "9876543210",
    "addressLine1": "Tower 9, Address Test <timestamp>",
    "addressLine2": "Near Central Plaza",
    "city": "Kolkata",
    "state": "West Bengal",
    "postalCode": "700001",
    "isDefaultShipping": false,
    "type": 2
  }
  ```

Actual:
- Status: `400`
- Body includes:
  - `request: ["The request field is required."]`
  - `$.type: ["The JSON value could not be converted to System.String..."]`
- Trace:
  - `traceId: "00-0373812712edcc29d90d2eab7f94d411-99168ae88dcc2408-00"`

Expected:
- Clear schema contract and successful create (`201/200`) for valid payload.

### Case C: Create address (flat payload, type string)
Request payload same as above, except:
```json
"type": "Both"
```

Actual:
- Status: `500`
- Body:
  - `success: false`
  - `message: "An unexpected error occurred."`
  - `traceId: "0HNNHST8RG2T1:0000001E"`
  - `timestamp: "2026-08-04T05:55:22.7443072Z"`

Expected:
- Successful create for valid enum/string or explicit validation error, not `500`.

### Case D: Create address (wrapped payload)
Request payload:
```json
{
  "request": {
    "fullName": "API Test User",
    "phoneNumber": "9876543210",
    "addressLine1": "Tower 9, Variant Test <timestamp>",
    "addressLine2": "Near Central Plaza",
    "city": "Kolkata",
    "state": "West Bengal",
    "postalCode": "700001",
    "isDefaultShipping": false,
    "type": "Both"
  }
}
```

Actual:
- Status: `400`
- Body:
  - `message: "Validation failed."`
  - errors: `FullName required`, `PhoneNumber required`, `AddressLine1 required`, `City required`, `State required`, `PostalCode required`
  - `traceId: "0HNNHST8RG2T1:0000001F"` (similar for Shipping variant `...20`)

Expected:
- If wrapper is required, server should bind wrapper and validate nested fields correctly.

## Suspected Root Cause
Likely request model binding mismatch in `POST /api/v1/addresses` and unhandled exception path in both list/create handlers.

Potential issues:
- DTO expects `request` wrapper while controller reads flat body (or vice versa).
- `type` contract mismatch between frontend (`number` originally) and backend (`string` expected), plus missing enum validation.
- Exception path not mapped to domain validation response, causing generic `500`.

## Frontend Contract References
- Frontend API caller: [src/api.js](../../TwosoulPerfume/TwoSoulPerfume/src/api.js)
- Address form payload builder: [src/components/AddressForm.jsx](../../TwosoulPerfume/TwoSoulPerfume/src/components/AddressForm.jsx)

## Recommended Backend Fix
1. Publish/confirm canonical request schema for `POST /api/v1/addresses` (flat vs wrapped).
2. Align DTO and validator with that schema.
3. Normalize `type` handling:
   - Accept explicit enum values with clear validation errors.
   - Avoid runtime conversion exceptions.
4. Fix unhandled exceptions in address handlers and return actionable error payloads.
5. Add integration tests for:
   - authenticated `GET /api/v1/addresses`
   - `POST /api/v1/addresses` with valid payload
   - invalid `type` payload
   - wrapper mismatch payload
