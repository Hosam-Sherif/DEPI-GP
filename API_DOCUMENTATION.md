# Mazaad API — Complete Technical Documentation

> **Version:** 2.0 · **Base URL:** `http://localhost:5245` · **Format:** JSON (UTF-8) · **Last Updated:** 2026-06-22

---

## Table of Contents

1. [Overview & Architecture](#1-overview--architecture)
2. [Authentication & Authorization](#2-authentication--authorization)
3. [Global Response Conventions](#3-global-response-conventions)
4. [Auth Endpoints](#4-auth-endpoints)
5. [Two-Factor Authentication 2FA](#5-two-factor-authentication-2fa)
6. [Company Registration](#6-company-registration)
7. [Companies](#7-companies)
8. [Company Users](#8-company-users)
9. [Listings](#9-listings)
10. [Bidding](#10-bidding)
11. [Chat](#11-chat)
12. [Orders](#12-orders)
13. [Notifications](#13-notifications)
14. [Material Categories](#14-material-categories)
15. [Industries](#15-industries)
16. [Inventory](#16-inventory)
17. [Sales Statistics](#17-sales-statistics)
18. [Operations Dashboard](#18-operations-dashboard)
19. [Analytics](#19-analytics)
20. [Security Logs](#20-security-logs)
21. [SignalR Real-Time Hubs](#21-signalr-real-time-hubs)
22. [Enumerations Reference](#22-enumerations-reference)
23. [Error Reference](#23-error-reference)

---

## 1. Overview & Architecture

Mazaad is a **B2B auction marketplace** for industrial raw materials. Companies list materials for timed auctions, competing companies place real-time bids, and orders are finalized from winning bids.

### Technology Stack

| Layer | Technology |
|---|---|
| API Framework | ASP.NET Core 8 Web API |
| Database | SQL Server via Entity Framework Core 8 |
| Authentication | JWT Bearer + HttpOnly Refresh Token Cookie |
| Real-Time | ASP.NET Core SignalR (WebSockets) |
| Identity | ASP.NET Core Identity |

### Domain Model

```
Industries  --< Companies --< AspNetUsers
Companies   --< Listings  --< Bids --> Orders
Listings    --< Chat_Channels --< Chat_Messages
Companies   --< InventoryItems
Material_Categories --< Listings / InventoryItems
```

---

## 2. Authentication & Authorization

### Bearer Token

Every protected endpoint requires:
```
Authorization: Bearer <accessToken>
```

### JWT Claims

| Claim Key | Type | Description |
|---|---|---|
| `uid` | string (int) | Authenticated user ID |
| `email` | string | User email address |
| `companyId` | string (int) | Company ID — EMPTY for SuperAdmin |
| `role` | string | `SuperAdmin`, `CompanyAdmin`, or `CompanyUser` |

### Roles

| Role | Description |
|---|---|
| `SuperAdmin` | Platform administrator. Manages company verification and analytics. No company. |
| `CompanyAdmin` | Admin of a verified company. Full company access. |
| `CompanyUser` | Employee of a verified company. Can create listings and bid. |

### Token Lifecycle

- **Access Token (JWT):** Short-lived (~60 min). Sent in `Authorization: Bearer` header.
- **Refresh Token (opaque):** Long-lived (7-30 days). Stored in `HttpOnly; Secure; SameSite=Strict` cookie `refreshToken`.
- After `change-password`: refresh token cookie deleted — user must re-login.

### Authorization Summary

| Action | Required |
|---|---|
| GET listings, bids, categories, companies (public) | None |
| Create / Update / Delete listing | CompanyAdmin OR CompanyUser (owning company) |
| Place bid / Quick bid | CompanyAdmin OR CompanyUser (any verified company) |
| Cancel bid | CompanyAdmin OR CompanyUser (bid's company) |
| Manage company users | CompanyAdmin (own company) or SuperAdmin |
| Approve/reject companies | SuperAdmin |
| Platform analytics / security logs (all) | SuperAdmin |

---

## 3. Global Response Conventions

### Success Codes

| Code | Meaning |
|---|---|
| `200 OK` | Successful request with body |
| `201 Created` | Resource created; body contains new resource |
| `204 No Content` | Successful, no body |

### Error Codes

| Code | Meaning |
|---|---|
| `400 Bad Request` | Validation or business rule failure |
| `401 Unauthorized` | Missing/invalid/expired token |
| `403 Forbidden` | Authenticated but not permitted |
| `404 Not Found` | Resource does not exist |
| `500 Internal Server Error` | Unhandled exception |

### Error Body

```json
{
  "success": false,
  "message": "Human-readable error"
}
```

Some endpoints return: `{ "errors": ["error1", "error2"] }` for validation arrays.

### Paginated Response (PagedResultDto)

```json
{
  "items": [ ... ],
  "totalCount": 42,
  "pageNumber": 1,
  "pageSize": 9,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

## 4. Auth Endpoints

Base route: `/api/auth`

---

### 4.1 Register User

**`POST /api/auth/register`** | Auth: None

**Request Body:**
```json
{
  "fullName": "John Doe",
  "email": "john@example.com",
  "password": "Password@123",
  "confirmPassword": "Password@123",
  "jobTitle": "Procurement Manager",
  "companyId": null
}
```

| Field | Type | Required | Validation |
|---|---|---|---|
| `fullName` | string | YES | Max 200 chars |
| `email` | string | YES | Valid email, globally unique |
| `password` | string | YES | Min 8 chars, uppercase, digit, symbol |
| `confirmPassword` | string | YES | Must match password |
| `jobTitle` | string | NO | Max 100 chars |
| `companyId` | int? | NO | FK to Companies |

**Response `200 OK`:**
```json
{
  "accessToken": "eyJhbGci...",
  "accessTokenExpiry": "2026-06-22T21:35:00Z",
  "user": {
    "id": 10,
    "fullName": "John Doe",
    "email": "john@example.com",
    "companyId": null,
    "roles": [],
    "twoFactorEnabled": false
  }
}
```

Sets cookie: `refreshToken` (HttpOnly, Secure, SameSite=Strict)

**Errors:** `400` — Email taken / passwords mismatch / weak password

---

### 4.2 Login

**`POST /api/auth/login`** | Auth: None

**Request Body:**
```json
{
  "email": "testadmin@amino.com",
  "password": "Test@12345",
  "rememberMe": false
}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| `email` | string | YES | |
| `password` | string | YES | |
| `rememberMe` | boolean | NO | true = 30-day refresh token (default 7 days) |

**Response `200 OK` — Success:**
```json
{
  "accessToken": "eyJhbGci...",
  "accessTokenExpiry": "2026-06-22T21:35:00Z",
  "user": {
    "id": 8,
    "fullName": "Test Company Admin",
    "email": "testadmin@amino.com",
    "companyId": 4,
    "roles": ["CompanyAdmin"],
    "twoFactorEnabled": false
  }
}
```

**Response `200 OK` — 2FA Required (no token issued):**
```json
{
  "requiresTwoFactor": true,
  "email": "testadmin@amino.com"
}
```
Proceed to `POST /api/2fa/verify`.

**Errors:** `400` — Wrong password / user not found / account deactivated

---

### 4.3 Refresh Token

**`POST /api/auth/refresh-token`** | Auth: `refreshToken` cookie

**Response `200 OK`:** `{ "accessToken": "...", "accessTokenExpiry": "...", "user": { ... } }`

Sets new cookie; old refresh token is revoked.

**Errors:** `401` — Cookie missing / expired / revoked

---

### 4.4 Logout

**`POST /api/auth/logout`** | Auth: None required

**Response `204 No Content`**

Revokes refresh token + clears cookie.

---

### 4.5 Change Password

**`POST /api/auth/change-password`** | Auth: Bearer

**Request Body:**
```json
{
  "currentPassword": "OldPass@123",
  "newPassword": "NewPass@456",
  "confirmNewPassword": "NewPass@456"
}
```

**Response `204 No Content`** — Refresh token cookie deleted after success.

**Errors:** `400` — Wrong current password / mismatch / weak new password | `401` — No token

---

## 5. Two-Factor Authentication 2FA

Base route: `/api/2fa` — All require Bearer unless noted.

---

### 5.1 Get Setup Info (QR Code)

**`GET /api/2fa/setup`** | Auth: Bearer

**Response `200 OK`:**
```json
{
  "qrCodeBase64": "data:image/png;base64,iVBORw0...",
  "manualEntryKey": "JBSWY3DPEHPK3PXP"
}
```

Scan QR with Google Authenticator or Authy.

---

### 5.2 Enable 2FA

**`POST /api/2fa/enable`** | Auth: Bearer

**Request Body:** `{ "code": "123456" }`

**Response `200 OK`:** `{ "message": "Two-factor authentication enabled successfully." }`

**Errors:** `400` — Invalid/expired TOTP code

---

### 5.3 Disable 2FA

**`POST /api/2fa/disable`** | Auth: Bearer

**Request Body:** `{ "code": "123456" }`

**Response `200 OK`:** `{ "message": "Two-factor authentication disabled." }`

---

### 5.4 Verify 2FA (Step 2 Login)

**`POST /api/2fa/verify`** | Auth: **None** (AllowAnonymous)

**Request Body:**
```json
{
  "email": "user@example.com",
  "code": "123456"
}
```

**Response `200 OK`:** Same as Login success — returns accessToken + user.

**Errors:** `400` — Invalid code / 2FA not enabled / user not found

---

## 6. Company Registration

Base route: `/api/companies`

---

### 6.1 Register Company

**`POST /api/companies/register`** | Auth: None | Content-Type: `multipart/form-data`

**Form Fields:**

| Field | Type | Required | Description |
|---|---|---|---|
| `IndustryId` | int | YES | FK to Industries |
| `CompanyName` | string | YES | Legal company name |
| `CommercialRegNum` | string | YES | Registration number |
| `TaxRegistrationNum` | string | YES | Tax ID |
| `City` | string | YES | City |
| `AddressDetails` | string | YES | Full address |
| `CommercialRegisterDocument` | file | YES | PDF/image, max 10 MB |
| `TaxCardDocument` | file | YES | PDF/image, max 10 MB |
| `AdminFullName` | string | YES | Admin full name |
| `AdminEmail` | string | YES | Must be unique |
| `AdminPassword` | string | YES | Min 8 chars, uppercase, digit, symbol |
| `ConfirmPassword` | string | YES | Must match AdminPassword |
| `AdminJobTitle` | string | NO | Admin job title |

**Response `201 Created`:**
```json
{
  "message": "Company registered successfully. Pending admin verification.",
  "accessToken": "eyJhbGci...",
  "accessTokenExpiry": "...",
  "user": {
    "id": 11, "fullName": "Khalid Hassan", "email": "khalid@delta.com",
    "companyId": 6, "roles": ["CompanyAdmin"], "twoFactorEnabled": false
  }
}
```

Company has `isVerified = false` until SuperAdmin approves.

**Errors:** `400` — Email taken / weak password | `400` — Invalid IndustryId

---

### 6.2 Get Pending Companies

**`GET /api/companies/pending`** | Auth: SuperAdmin

**Response `200 OK`:** Array of `CompanyResponseDto`
```json
[
  {
    "id": 5, "industryId": 1, "industryName": "Manufacturing",
    "companyName": "Beta Copper LLC", "commercialRegNum": "CR-BETA-001",
    "taxRegistrationNum": "TAX-BETA-001", "city": "Alexandria",
    "addressDetails": "12 Industrial District", "isVerified": false,
    "createdAt": "2026-06-21T10:05:23Z"
  }
]
```

---

### 6.3 Verify or Reject Company

**`PATCH /api/companies/{id}/verify`** | Auth: SuperAdmin

**Request Body:**
```json
{
  "isApproved": true,
  "rejectionReason": ""
}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| `isApproved` | boolean | YES | true=approve, false=reject |
| `rejectionReason` | string | Conditional | Required when isApproved=false |

**Response `204 No Content`**

**Errors:** `400` — Missing rejection reason | `404` — Company not found

---

### 6.4 Get Company Documents

**`GET /api/companies/{id}/documents`** | Auth: SuperAdmin

**Response `200 OK`:** Array of document metadata
```json
[
  {
    "id": 1, "documentType": "CommercialRegisterDocument",
    "fileName": "commercial_reg.pdf", "uploadedAt": "2026-06-21T10:10:00Z"
  }
]
```

---

### 6.5 Download Document

**`GET /api/companies/documents/{documentId}/download`** | Auth: SuperAdmin

**Response `200 OK`:** Raw file stream (`Content-Disposition: attachment`)

**Errors:** `404` — Document not found

---

## 7. Companies

Base route: `/api/Companies`

---

### 7.1 Get All Companies

**`GET /api/Companies`** | Auth: None

**Response `200 OK`:** Array of CompanyResponseDto
```json
[
  {
    "id": 4, "industryId": 1, "industryName": "Manufacturing",
    "companyName": "Amino", "commercialRegNum": "Amino",
    "taxRegistrationNum": "Amino", "city": "Girga",
    "addressDetails": "Girga", "isVerified": true,
    "createdAt": "2026-06-21T22:14:40Z"
  }
]
```

---

### 7.2 Get Company by ID

**`GET /api/Companies/{id}`** | Auth: None

**Response `200 OK`:** Single CompanyResponseDto

**Errors:** `404` — Not found

---

### 7.3 Quick Verify Company

**`PATCH /api/Companies/{id}/verifyy`** | Auth: Bearer

No request body. Sets `isVerified = true` immediately.

**Response `204 No Content`**

---

## 8. Company Users

Base route: `/api/companies/{companyId}/users`

CompanyAdmin can only access their own company. SuperAdmin can access any.

---

### 8.1 Get Company Users

**`GET /api/companies/{companyId}/users`** | Auth: Bearer

**Response `200 OK`:**
```json
[
  {
    "id": 8, "fullName": "Test Company Admin", "email": "testadmin@amino.com",
    "jobTitle": "Test Admin", "roles": ["CompanyAdmin"], "isActive": true,
    "twoFactorEnabled": false, "lastLoginDate": "2026-06-22T16:53:24Z",
    "createdAt": "2026-06-22T16:44:57Z"
  },
  {
    "id": 9, "fullName": "Bidder User", "email": "bidder@amino.com",
    "jobTitle": "Procurement Officer", "roles": ["CompanyUser"], "isActive": true,
    "twoFactorEnabled": false, "lastLoginDate": null,
    "createdAt": "2026-06-22T20:31:00Z"
  }
]
```

**Errors:** `403` — Caller not in this company (not SuperAdmin)

---

### 8.2 Get Company User by ID

**`GET /api/companies/{companyId}/users/{userId}`** | Auth: Bearer

**Response `200 OK`:** Single user object

**Errors:** `404` — `{ "message": "User not found in this company." }`

---

### 8.3 Add User to Company

**`POST /api/companies/{companyId}/users`** | Auth: CompanyAdmin or SuperAdmin

**Request Body:**
```json
{
  "fullName": "Sara Ahmed",
  "email": "sara@amino.com",
  "password": "Password@123",
  "confirmPassword": "Password@123",
  "jobTitle": "Sales Manager",
  "role": "CompanyUser"
}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| `fullName` | string | YES | |
| `email` | string | YES | Globally unique |
| `password` | string | YES | Complexity rules apply |
| `confirmPassword` | string | YES | Must match |
| `jobTitle` | string | NO | |
| `role` | string | YES | "CompanyAdmin" or "CompanyUser" |

**Response `201 Created`:** New user object

**Errors:** `400` — Email taken / mismatch / weak password | `403` — Not CompanyAdmin | `404` — Company not found

---

### 8.4 Update Company User

**`PATCH /api/companies/{companyId}/users/{userId}`** | Auth: CompanyAdmin or SuperAdmin

**Request Body (optional fields):**
```json
{ "role": "CompanyAdmin", "isActive": true }
```

**Response `204 No Content`**

---

### 8.5 Remove User from Company

**`DELETE /api/companies/{companyId}/users/{userId}`** | Auth: CompanyAdmin or SuperAdmin

**Response `204 No Content`**

---

## 9. Listings

Base route: `/api/Listing`

> **UnitOfMeasure Rule:** Auto-inherited from MaterialCategory. CategoryId 1 (Steel) => "Ton". Any value sent in body is IGNORED.

---

### 9.1 Get All Listings (Marketplace Grid)

**`GET /api/Listing`** | Auth: None

**Query Parameters:**

| Parameter | Type | Default | Description |
|---|---|---|---|
| `PageNumber` | int | 1 | |
| `PageSize` | int | 9 | |
| `CategoryId` | int? | — | Filter by category |
| `Condition` | int? | — | 0=New, 1=Used, 2=Refurbished |
| `Status` | int? | — | 0=Upcoming, 1=Active, 2=Closed, 3=Sold |
| `MinPrice` | decimal? | — | Min current highest bid |
| `MaxPrice` | decimal? | — | Max current highest bid |
| `SearchTerm` | string? | — | Search title and description |

**Response `200 OK` — PagedResultDto of ListingCardDto:**
```json
{
  "items": [
    {
      "id": 7, "title": "Premium Steel Coils — Grade A",
      "description": "High-purity cold-rolled steel coils...",
      "imageUrl": "", "categoryName": "Steel", "companyName": "Amino",
      "currentHighestBid": 310.00, "bidCount": 3, "status": 1, "condition": 0,
      "baseCurrency": "USD", "unitOfMeasure": "Ton",
      "endDate": "2026-12-31T00:00:00Z", "secondsRemaining": 15724800.0
    }
  ],
  "totalCount": 7, "pageNumber": 1, "pageSize": 9,
  "totalPages": 1, "hasNextPage": false, "hasPreviousPage": false
}
```

---

### 9.2 Get Listing by ID

**`GET /api/Listing/{id}`** | Auth: None

**Response `200 OK` — ListingResponseDto:**
```json
{
  "id": 7, "companyId": 4, "categoryId": 1,
  "title": "Premium Steel Coils — Grade A",
  "description": "High-purity cold-rolled steel coils...",
  "minOrderQuantity": 5.0, "availableQuantity": 200.0,
  "unitOfMeasure": "Ton", "purityPercentage": 99.5,
  "baseCurrency": "USD",
  "startDate": "2026-07-01T10:00:00Z", "endDate": "2026-09-30T18:00:00Z",
  "currentHighestBid": 310.00
}
```

**Errors:** `404` — Not found or soft-deleted

---

### 9.3 Get Listing Detail (Full Bidding Room)

**`GET /api/Listing/{id}/detail`** | Auth: None

**Response `200 OK` — ListingDetailDto:**
```json
{
  "id": 7, "companyId": 4, "companyName": "Amino",
  "categoryId": 1, "categoryName": "Steel",
  "title": "Premium Steel Coils — Grade A",
  "description": "High-purity cold-rolled steel coils...",
  "technicalSpecs": "", "minOrderQuantity": 5.0, "availableQuantity": 200.0,
  "unitOfMeasure": "Ton", "purityPercentage": 99.5, "baseCurrency": "USD",
  "currentHighestBid": 310.00, "bidCount": 3, "status": 1, "condition": 0,
  "imageUrl": "", "location": "", "dueDiligenceUrls": "",
  "startDate": "2026-07-01T10:00:00Z", "endDate": "2026-12-31T00:00:00Z",
  "topBids": [
    {
      "id": 8, "listingId": 7, "buyerCompanyId": 4,
      "displayBidderName": "Amino", "bidAmountPerUnit": 310.00,
      "totalBidAmount": 3100.00, "quantity": 10.0,
      "isAnonymous": false, "status": 1, "createdAt": "2026-06-22T20:40:35Z"
    }
  ]
}
```

---

### 9.4 Create Listing

**`POST /api/Listing`** | Auth: Bearer — CompanyAdmin or CompanyUser

**Request Body:**
```json
{
  "categoryId": 1,
  "title": "Premium Steel Coils — Grade A",
  "description": "High-purity cold-rolled steel coils. 200 Ton available.",
  "minOrderQuantity": 5,
  "availableQuantity": 200,
  "purityPercentage": 99.5,
  "baseCurrency": "USD",
  "startDate": "2026-07-01T10:00:00Z",
  "endDate": "2026-09-30T18:00:00Z",
  "startingPrice": 750.00
}
```

| Field | Type | Required | Notes |
|---|---|---|---|
| `categoryId` | int | YES | Determines unitOfMeasure automatically |
| `title` | string | YES | Max 200 chars |
| `description` | string | YES | |
| `minOrderQuantity` | decimal | YES | Min units per bid |
| `availableQuantity` | decimal | YES | Total units offered |
| `purityPercentage` | decimal | NO | Grade/purity 0-100 |
| `baseCurrency` | string | YES | ISO code e.g. "USD" |
| `startDate` | datetime | YES | ISO 8601 UTC |
| `endDate` | datetime | YES | ISO 8601 UTC |
| `startingPrice` | decimal | YES | Floor bid price per unit |
| `unitOfMeasure` | string? | NO | IGNORED — auto-set from category |

**Response `201 Created`:** ListingResponseDto (includes auto-assigned unitOfMeasure)

**Errors:** `401` — No companyId in JWT | `404` — Invalid categoryId

---

### 9.5 Update Listing

**`PUT /api/Listing/{id}`** | Auth: Bearer — CompanyAdmin or CompanyUser (owning company)

**Request Body:** Same as Create Listing

**Response `200 OK`:** Updated ListingResponseDto

**Errors:** `401` — No companyId | `404` — Not found or not owned by caller's company

---

### 9.6 Delete Listing (Soft Delete)

**`DELETE /api/Listing/{id}`** | Auth: Bearer — CompanyAdmin or CompanyUser (owning company)

Sets IsDeleted=true. Does not appear in GET responses.

**Response `204 No Content`**

**Errors:** `400` — Not found or not owned | `401` — No companyId

---
