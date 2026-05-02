# Mazaad Institutional Marketplace – API Documentation

**Base URL:** `http://localhost:5245`  
**Swagger UI:** `http://localhost:5245/index.html`  
**Real-Time:** SignalR at `/hubs/bidding` and `/hubs/chat`

---

## Table of Contents
1. [Listing](#1-listing)
2. [Bidding](#2-bidding)
3. [Orders](#3-orders)
4. [Notifications](#4-notifications)
5. [Companies](#5-companies)
6. [Industry](#6-industry)
7. [Material Category](#7-material-category)
8. [Chat (REST)](#8-chat-rest)
9. [SignalR – BiddingHub](#9-signalr--biddinghub)
10. [SignalR – ChatHub](#10-signalr--chathub)

---

## 1. Listing

### GET `/api/listing`
**Description:** Returns a paginated, filtered list of marketplace listing cards.  
Used to power the **Institutional Marketplace** grid screen.

**Query Parameters:**
| Param | Type | Required | Description |
|-------|------|----------|-------------|
| `categoryId` | int | No | Filter by material category |
| `condition` | int | No | 0=New, 1=Certified, 2=OpenBox, 3=FixedIt |
| `status` | int | No | 0=Upcoming, 1=Active, 2=Ended |
| `minPrice` | decimal | No | Minimum current highest bid |
| `maxPrice` | decimal | No | Maximum current highest bid |
| `searchTerm` | string | No | Free-text search on Title/Description |
| `pageNumber` | int | No | Default: 1 |
| `pageSize` | int | No | Default: 9 |

**Response `200 OK`:**
```json
{
  "items": [
    {
      "id": 1,
      "title": "Siemens Healthineers Lumina 3T MRI Suite",
      "description": "Certified/Refurbished. Full-body 3 Tesla MRI system.",
      "imageUrl": "https://images.unsplash.com/...",
      "categoryName": "Medical Equipment",
      "companyName": "Siemens Healthineers MENA",
      "currentHighestBid": 3420000.00,
      "bidCount": 12,
      "status": 1,
      "condition": 1,
      "baseCurrency": "USD",
      "endDate": "2026-12-31T23:59:59Z",
      "secondsRemaining": 18234000.0
    }
  ],
  "totalCount": 6,
  "pageNumber": 1,
  "pageSize": 9,
  "totalPages": 1,
  "hasNextPage": false,
  "hasPreviousPage": false
}
```

---

### GET `/api/listing/{id}`
**Description:** Returns a summary of a single listing by ID.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Listing ID |

**Response `200 OK`:**
```json
{
  "id": 1,
  "companyId": 1,
  "categoryId": 1,
  "title": "Siemens Healthineers Lumina 3T MRI Suite",
  "description": "...",
  "minOrderQuantity": 1.0,
  "availableQuantity": 1.0,
  "purityPercentage": 100.0,
  "baseCurrency": "USD",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2026-12-31T23:59:59Z",
  "currentHighestBid": 3420000.00
}
```

**Response `404 Not Found`:** Listing does not exist or is deleted.

---

### GET `/api/listing/{id}/detail`
**Description:** Returns full listing detail for the **Live Bidding Room** screen.  
Includes company name, category name, technical specs, location, due diligence docs, and the top 5 bids.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Listing ID |

**Response `200 OK`:**
```json
{
  "id": 1,
  "companyId": 1,
  "companyName": "Siemens Healthineers MENA",
  "categoryId": 1,
  "categoryName": "Medical Equipment",
  "title": "Siemens Healthineers Lumina 3T MRI Suite",
  "description": "Deutsche Klinik Regional Distribution Center...",
  "technicalSpecs": "{\"FieldStrength\":\"3 Tesla\",\"BoreSize\":\"70 cm\",\"ManufactureDate\":\"October 2021\"}",
  "minOrderQuantity": 1.0,
  "availableQuantity": 1.0,
  "purityPercentage": 100.0,
  "baseCurrency": "USD",
  "currentHighestBid": 3420000.00,
  "bidCount": 12,
  "status": 1,
  "condition": 1,
  "imageUrl": "https://images.unsplash.com/...",
  "location": "Industrial Park West, Gate 12, Munich 80331, DE",
  "dueDiligenceUrls": "inspection_report.pdf,maintenance_log.pdf,terms_of_sale.pdf",
  "startDate": "2025-01-01T00:00:00Z",
  "endDate": "2026-12-31T23:59:59Z",
  "topBids": [
    {
      "id": 3,
      "listingId": 1,
      "buyerCompanyId": 2,
      "displayBidderName": "Gulf Heavy Equipment LLC",
      "bidAmountPerUnit": 3420000.00,
      "totalBidAmount": 3420000.00,
      "quantity": 1.0,
      "isAnonymous": false,
      "status": 2,
      "createdAt": "2026-05-02T14:00:00Z"
    }
  ]
}
```

**Response `404 Not Found`:** Listing does not exist.

---

### POST `/api/listing`
**Description:** Creates a new auction listing. The seller company ID is taken from the authenticated user (currently hardcoded to `1` for testing).

**Request Body:**
```json
{
  "categoryId": 1,
  "title": "New Industrial Asset",
  "description": "Detailed description of the asset",
  "minOrderQuantity": 1,
  "availableQuantity": 5,
  "purityPercentage": 99.5,
  "baseCurrency": "USD",
  "startDate": "2026-06-01T00:00:00Z",
  "endDate": "2026-07-01T00:00:00Z",
  "startingPrice": 50000
}
```

**Response `201 Created`:** Returns the created listing summary.

---

### PUT `/api/listing/{id}`
**Description:** Updates a listing's mutable fields. Only the owning company can update.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Listing ID |

**Request Body:** Same as `POST /api/listing`

**Response `200 OK`:** Returns updated listing summary.  
**Response `404 Not Found`:** Listing not found or caller doesn't own it.

---

### DELETE `/api/listing/{id}`
**Description:** Soft-deletes a listing (sets `IsDeleted = true`). Only the owning company can delete.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Listing ID |

**Response `204 No Content`:** Deleted successfully.  
**Response `400 Bad Request`:** Listing not found or not owned by caller.

---

## 2. Bidding

### POST `/api/bidding/place-bid`
**Description:** Places a full bid from the **Live Bidding Room** execution panel.  
Validates auction is active, quantity is within bounds, and bid exceeds current price.  
Marks all previous active bids as `Outbid`, increments `BidCount` on the listing.  
Sends "You've been outbid" notifications to displaced bidders.  
Broadcasts `BidPlaced` event via SignalR to all clients watching that listing.

**Request Body:**
```json
{
  "listingId": 2,
  "bidAmountPerUnit": 290000,
  "totalBidAmount": 290000,
  "quantity": 1,
  "isAnonymous": false
}
```

**Response `200 OK`:**
```json
{
  "success": true,
  "message": "Bid placed successfully.",
  "displayBiddersName": "Gulf Heavy Equipment LLC",
  "newPrice": 290000.00,
  "newBidId": 7,
  "newBidCount": 9
}
```

**Response `400 Bad Request`:**
```json
{
  "success": false,
  "message": "Your bid must exceed the current highest bid.",
  "displayBiddersName": "",
  "newPrice": 0,
  "newBidId": 0,
  "newBidCount": 0
}
```

**Validation Rules:**
- Auction `EndDate` must be in the future
- `Quantity` ≥ `MinOrderQuantity` and ≤ `AvailableQuantity`
- `BidAmountPerUnit` > `CurrentHighestBid`

---

### POST `/api/bidding/quick-bid`
**Description:** One-click bid from a **Marketplace card** "Quick Bid" button.  
Automatically uses the listing's `MinOrderQuantity` — caller only provides the price.  
Also broadcasts `BidPlaced` via SignalR.

**Request Body:**
```json
{
  "listingId": 3,
  "bidAmountPerUnit": 1250000,
  "isAnonymous": false
}
```

**Response `200 OK`:** Same format as `place-bid`.  
**Response `400 Bad Request`:** Same validation errors as `place-bid`.

---

### GET `/api/bidding/listing/{listingId}`
**Description:** Returns all bids for a listing ordered by amount descending (basic summary view).

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `listingId` | int | Listing ID |

**Response `200 OK`:**
```json
[
  {
    "success": true,
    "message": "Bid retrieved",
    "displayBiddersName": "Gulf Heavy Equipment LLC",
    "newPrice": 290000.00,
    "newBidId": 7,
    "newBidCount": 0
  }
]
```

---

### GET `/api/bidding/listing/{listingId}/live`
**Description:** Returns the top 10 bids with full detail for the **Live Bidding Feed** panel.  
Used to initialize the SignalR client state when a user opens the bidding room.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `listingId` | int | Listing ID |

**Response `200 OK`:**
```json
[
  {
    "id": 7,
    "listingId": 2,
    "buyerCompanyId": 2,
    "displayBidderName": "Gulf Heavy Equipment LLC",
    "bidAmountPerUnit": 290000.00,
    "totalBidAmount": 290000.00,
    "quantity": 1.0,
    "isAnonymous": false,
    "status": 2,
    "createdAt": "2026-05-02T15:30:00Z"
  }
]
```

**Bid Status Values:**
| Value | Name | Description |
|-------|------|-------------|
| 0 | Active | Currently the leading bid |
| 1 | Outbid | A higher bid was placed |
| 2 | Winning | Auction ended, this bid won |
| 3 | Won | Converted to an order |
| 4 | Cancelled | Cancelled by the bidder |

---

### GET `/api/bidding/{id}`
**Description:** Returns the full detail of a single bid.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Bid ID |

**Response `200 OK`:** Same format as single item in `/live` response.  
**Response `404 Not Found`:** Bid does not exist.

---

### DELETE `/api/bidding/{id}`
**Description:** Cancels a bid (sets `Status = Cancelled`). Only the bidding company can cancel their own bid.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Bid ID |

**Response `204 No Content`:** Cancelled successfully.  
**Response `400 Bad Request`:** Bid not found or not owned by caller.

---

## 3. Orders

### GET `/api/orders`
**Description:** Returns all orders where the current company is the buyer **or** the seller. Ordered by most recent first.

**Response `200 OK`:**
```json
[
  {
    "id": 1,
    "bidId": 5,
    "sellerCompanyId": 1,
    "sellerCompanyName": "Siemens Healthineers MENA",
    "buyerCompanyId": 2,
    "buyerCompanyName": "Gulf Heavy Equipment LLC",
    "agreedQuantity": 1.0,
    "agreedUnitPrice": 3420000.00,
    "platformFee": 85500.00,
    "totalAmount": 3420000.00,
    "status": 1,
    "notes": "",
    "orderDate": "2026-05-02T16:00:00Z"
  }
]
```

**Order Status Values:**
| Value | Name |
|-------|------|
| 0 | Pending |
| 1 | Confirmed |
| 2 | Completed |
| 3 | Cancelled |

---

### GET `/api/orders/{id}`
**Description:** Returns a single order by ID.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Order ID |

**Response `200 OK`:** Same format as single item above.  
**Response `404 Not Found`:** Order does not exist.

---

### POST `/api/orders/finalize`
**Description:** Converts a winning bid into a formal order.  
Automatically applies the active commission policy to calculate `PlatformFee`.  
Marks the bid as `Won` and sends a "Congratulations" notification to the buyer.  
Can only be called after the auction `EndDate` has passed.

**Request Body:**
```json
{
  "bidId": 5,
  "notes": "Delivery to be arranged within 30 days."
}
```

**Response `201 Created`:** Returns the created order (same format as GET).

**Response `400 Bad Request`:**
```json
"Auction has not ended yet."
```
or
```json
"No active commission policy found."
```

**Response `403 Forbidden`:** Caller does not own the listing.

---

### PATCH `/api/orders/{id}/status`
**Description:** Updates the status of an order. Both buyer and seller can update.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Order ID |

**Request Body:**
```json
2
```
*(Integer representing `OrderStatus` enum: 0=Pending, 1=Confirmed, 2=Completed, 3=Cancelled)*

**Response `204 No Content`:** Updated successfully.  
**Response `400 Bad Request`:** Order not found or caller is not buyer/seller.

---

## 4. Notifications

### GET `/api/notifications`
**Description:** Returns all notifications for the current user, ordered by most recent first.  
Notifications are created automatically when: a bid is outbid, or an order is confirmed.

**Response `200 OK`:**
```json
[
  {
    "id": 1,
    "userId": 1,
    "title": "You've been outbid",
    "message": "Your bid on 'Siemens Healthineers Lumina 3T MRI Suite' has been outbid. New price: $3,420,000.00",
    "isRead": false,
    "referenceType": "Listing",
    "referenceId": 1,
    "createdAt": "2026-05-02T15:00:00Z"
  }
]
```

---

### GET `/api/notifications/unread-count`
**Description:** Returns the count of unread notifications. Used to display the badge on the notification bell icon.

**Response `200 OK`:**
```json
{
  "unreadCount": 3
}
```

---

### PUT `/api/notifications/{id}/read`
**Description:** Marks a single notification as read.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Notification ID |

**Response `204 No Content`:** Marked as read.  
**Response `404 Not Found`:** Notification not found or doesn't belong to current user.

---

### PUT `/api/notifications/read-all`
**Description:** Marks all of the current user's notifications as read (bulk operation).

**Response `204 No Content`:** All marked as read.

---

## 5. Companies

### GET `/api/companies`
**Description:** Returns all registered companies with their industry name.

**Response `200 OK`:**
```json
[
  {
    "id": 1,
    "industryId": 1,
    "industryName": "Medical Devices",
    "companyName": "Siemens Healthineers MENA",
    "commercialRegNum": "CR-001-2025",
    "taxRegistrationNum": "TRN-001-2025",
    "city": "Munich",
    "addressDetails": "Industrial Park West, Gate 12, Munich 80331, DE",
    "isVerified": true,
    "createdAt": "2025-01-01T00:00:00Z"
  }
]
```

---

### GET `/api/companies/{id}`
**Description:** Returns a single company by ID.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Company ID |

**Response `200 OK`:** Same format as single item above.  
**Response `404 Not Found`:** Company does not exist.

---

### POST `/api/companies`
**Description:** Registers a new company. The company starts as unverified (`isVerified = false`).

**Request Body:**
```json
{
  "industryId": 2,
  "companyName": "New Machinery Co.",
  "commercialRegNum": "CR-010-2026",
  "taxRegistrationNum": "TRN-010-2026",
  "city": "Cairo",
  "addressDetails": "10 Industrial Zone, Cairo, Egypt"
}
```

**Response `201 Created`:** Returns the created company with `isVerified = false`.

---

### PATCH `/api/companies/{id}/verify`
**Description:** Marks a company as verified (admin action). Sets `IsVerified = true`.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Company ID |

**Response `204 No Content`:** Verified successfully.  
**Response `404 Not Found`:** Company does not exist.

---

## 6. Industry

### GET `/api/industry`
**Description:** Returns all active industry types. Used to populate the **Industrial Sectors** filter panel in the marketplace.

**Response `200 OK`:**
```json
[
  { "id": 1, "industryName": "Medical Devices",  "createdAt": "2025-01-01T00:00:00Z" },
  { "id": 2, "industryName": "Heavy Machinery",  "createdAt": "2025-01-01T00:00:00Z" },
  { "id": 3, "industryName": "Data Centers",     "createdAt": "2025-01-01T00:00:00Z" },
  { "id": 4, "industryName": "Real Estate",      "createdAt": "2025-01-01T00:00:00Z" },
  { "id": 5, "industryName": "Robotics",         "createdAt": "2025-01-01T00:00:00Z" }
]
```

---

### GET `/api/industry/{id}`
**Description:** Returns a single industry type by ID.

**Response `200 OK`:** Single item same format as above.  
**Response `404 Not Found`:** Industry does not exist or is deleted.

---

### POST `/api/industry`
**Description:** Creates a new industry type.

**Request Body:**
```json
{
  "industryName": "Aerospace"
}
```

**Response `201 Created`:** Returns the created industry type.

---

### DELETE `/api/industry/{id}`
**Description:** Soft-deletes an industry type (`IsDeleted = true`). It will no longer appear in GET results.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `id` | int | Industry ID |

**Response `204 No Content`:** Deleted successfully.  
**Response `404 Not Found`:** Industry does not exist.

---

## 7. Material Category

### GET `/api/materialcategory`
**Description:** Returns all material categories. Used as filter options and when creating a listing.

**Response `200 OK`:**
```json
[
  { "id": 1, "categoryName": "Medical Equipment",      "description": "Diagnostic and therapeutic medical devices", "unitOfMeasure": "Unit" },
  { "id": 2, "categoryName": "Construction Machinery", "description": "Heavy construction equipment",               "unitOfMeasure": "Unit" },
  { "id": 3, "categoryName": "IT Infrastructure",      "description": "Servers, networking and cooling",            "unitOfMeasure": "Unit" },
  { "id": 4, "categoryName": "Fleet Vehicles",         "description": "Commercial fleet bundles",                   "unitOfMeasure": "Fleet" },
  { "id": 5, "categoryName": "Real Estate",            "description": "Commercial and industrial real estate",      "unitOfMeasure": "sqm" },
  { "id": 6, "categoryName": "Industrial Robots",      "description": "Robotic arms and production lines",         "unitOfMeasure": "Unit" }
]
```

---

### GET `/api/materialcategory/{id}`
**Description:** Returns a single category by ID.

**Response `200 OK`:** Single item same format.  
**Response `404 Not Found`:** Category does not exist.

---

### POST `/api/materialcategory`
**Description:** Creates a new material category.

**Request Body:**
```json
{
  "categoryName": "Renewable Energy",
  "description": "Solar panels, wind turbines and energy storage",
  "unitOfMeasure": "MW"
}
```

**Response `201 Created`:** Returns the created category.

---

### DELETE `/api/materialcategory/{id}`
**Description:** Permanently deletes a category.

**Response `204 No Content`:** Deleted successfully.  
**Response `400 Bad Request`:** Category not found.

---

## 8. Chat (REST)

### POST `/api/chat/start`
**Description:** Creates or retrieves an existing chat channel between a buyer and seller for a specific listing.  
If a channel already exists for this listing + buyer combination, it returns the existing channel ID.

**Query Parameters:**
| Param | Type | Required | Description |
|-------|------|----------|-------------|
| `listingId` | int | Yes | The listing being discussed |
| `buyerCompanyId` | int | Yes | The buyer's company ID |
| `sellerCompanyId` | int | Yes | The seller's company ID |

**Response `200 OK`:**
```json
{
  "channelId": 3
}
```

---

### GET `/api/chat/{channelId}/history`
**Description:** Returns the full message history for a channel, ordered by oldest first.

**Path Parameters:**
| Param | Type | Description |
|-------|------|-------------|
| `channelId` | int | Chat channel ID |

**Response `200 OK`:**
```json
[
  {
    "id": 1,
    "channelId": 3,
    "senderUserId": 1,
    "messageText": "Is the MRI unit still available for Q3 delivery?",
    "sentAt": "2026-05-02T10:00:00Z"
  },
  {
    "id": 2,
    "channelId": 3,
    "senderUserId": 2,
    "messageText": "Yes, we can arrange Q3 delivery. Please confirm the bid.",
    "sentAt": "2026-05-02T10:05:00Z"
  }
]
```

---

## 9. SignalR – BiddingHub

**Connection URL:** `ws://localhost:5245/hubs/bidding`

The BiddingHub enables real-time bid updates for the **Live Bidding Room** screen.  
Connect using the SignalR client library (`@microsoft/signalr`).

### Client → Server Methods

#### `JoinAuction(listingId: number)`
**Description:** Subscribe to live updates for a specific listing. Call this when the user opens the bidding room.  
The server immediately pushes the current top bids back to the caller via `InitialBidState`.

```javascript
connection.invoke("JoinAuction", 1);
```

---

#### `LeaveAuction(listingId: number)`
**Description:** Unsubscribe from a listing's updates. Call this when the user navigates away.

```javascript
connection.invoke("LeaveAuction", 1);
```

---

#### `PlaceBid(userId, companyId, request)`
**Description:** Place a bid directly via SignalR (alternative to the REST endpoint).  
On success, broadcasts `BidPlaced` to the entire auction room group.  
On failure, sends `BidFailed` only to the caller.

```javascript
connection.invoke("PlaceBid", 1, 2, {
  listingId: 1,
  bidAmountPerUnit: 3500000,
  totalBidAmount: 3500000,
  quantity: 1,
  isAnonymous: false
});
```

---

### Server → Client Events

#### `InitialBidState` → `BidDetailDto[]`
**Triggered:** Immediately after a client calls `JoinAuction`.  
**Payload:** Array of top 10 bids (full detail) for the listing.

```javascript
connection.on("InitialBidState", (bids) => {
  // Populate the Live Bidding Feed with current bids
  renderBidFeed(bids);
});
```

---

#### `BidPlaced` → `LiveBidUpdateDto`
**Triggered:** When any client (REST or SignalR) successfully places a bid.  
**Payload:**

```javascript
connection.on("BidPlaced", (update) => {
  // update shape:
  // {
  //   listingId: 1,
  //   bidId: 14,
  //   displayBidderName: "Gulf Heavy Equipment LLC",
  //   newHighestBid: 3500000.00,
  //   totalBidCount: 13,
  //   timestamp: "2026-05-02T15:00:00Z"
  // }
  updatePriceDisplay(update.newHighestBid);
  addBidToFeed(update);
});
```

---

#### `BidFailed` → `string`
**Triggered:** When a `PlaceBid` call from this client fails (only sent to the caller).

```javascript
connection.on("BidFailed", (errorMessage) => {
  showErrorToast(errorMessage);
});
```

---

## 10. SignalR – ChatHub

**Connection URL:** `ws://localhost:5245/hubs/chat`

### Client → Server Methods

#### `JoinChannel(channelId: string)`
Subscribe to messages in a chat channel.

```javascript
connection.invoke("JoinChannel", "3");
```

#### `SendMessage(channelId, senderUserId, messageText)`
Send a message. Saved to the database, then broadcast to all channel members.

```javascript
connection.invoke("SendMessage", 3, 1, "Hello, is the asset still available?");
```

---

### Server → Client Events

#### `ReceiveMessage` → `MessageResponseDto`
```javascript
connection.on("ReceiveMessage", (message) => {
  // {
  //   id: 5,
  //   channelId: 3,
  //   senderUserId: 1,
  //   messageText: "Hello, is the asset still available?",
  //   sentAt: "2026-05-02T15:00:00Z"
  // }
  appendMessageToChat(message);
});
```

---

## Enum Reference

### `ListingStatus`
| Value | Name | Description |
|-------|------|-------------|
| 0 | Upcoming | Auction not started yet |
| 1 | Active | Auction is live |
| 2 | Ended | Auction has finished |
| 3 | Cancelled | Auction was cancelled |

### `ListingCondition`
| Value | Name | UI Label |
|-------|------|----------|
| 0 | New | New |
| 1 | Certified | Certified |
| 2 | OpenBox | Open Box |
| 3 | FixedIt | Fixed-It |

### `BidStatus`
| Value | Name | Description |
|-------|------|-------------|
| 0 | Active | Currently leading bid |
| 1 | Outbid | Superseded by higher bid |
| 2 | Winning | Auction ended, awaiting finalization |
| 3 | Won | Converted to an order |
| 4 | Cancelled | Cancelled by bidder |

### `OrderStatus`
| Value | Name |
|-------|------|
| 0 | Pending |
| 1 | Confirmed |
| 2 | Completed |
| 3 | Cancelled |

---

## Common HTTP Status Codes

| Code | Meaning |
|------|---------|
| `200 OK` | Request succeeded, body contains data |
| `201 Created` | Resource created, body contains created resource |
| `204 No Content` | Success, no body |
| `400 Bad Request` | Invalid input or business rule violation |
| `403 Forbidden` | Caller is not authorized to perform this action |
| `404 Not Found` | Resource does not exist |
| `500 Internal Server Error` | Unexpected server error |
