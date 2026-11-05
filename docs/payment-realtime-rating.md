# T30, T31, T45: payment and rating flows

## Payment initiation and polling (T30)

Authenticated buyers use `POST /api/v1/payments/initiate` (alias `/payments/initiate`):

```json
{ "parentOrderId": "<parent-order UUID>", "paymentMethod": "VietQR" }
```

Methods are `MoMo`, `VietQR`, and `COD` (case insensitive). The order must belong to the caller, both order/payment must still be pending, and the requested method must match checkout. Amounts come from the database. Payment initiation reuses the existing payment; it does not create another payment or reset its deadline.

`ApiResponse<PaymentInitResponse>.data` contains `paymentId`, `parentOrderId`, `paymentMethod`, `amount`, `status`, `paymentUrl`, `qrPayload`, `qrImageBase64`, `expiredAt`, and optional `vietQr` bank/display details. Online expiry is the persisted checkout deadline (15 minutes). QR image bytes are generated locally as a PNG; clients prefix the base64 with `data:image/png;base64,`.

COD returns `Pending`, null URL/QR and null expiry without invoking online adapters. Online callbacks reject payments of another method. COD settlement remains with the existing verified shipping reconciliation flow.

`GET /api/v1/payments/{paymentId}/status` (alias `/payments/{paymentId}/status`) returns `PaymentStatusResponse` only to the buyer. It reads `Payments.status` directly from the database and sends `Cache-Control: no-store`. A pending payment whose deadline has passed remains pending until the timeout worker actually expires it; clients also use `expiredAt` to stop showing an expired QR.

The buyer QR modal uses these endpoints, polls every five seconds while waiting, displays the server deadline, and uses the generated VietQR image. Polling covers missed realtime messages. Duplicate polling/realtime success callbacks are suppressed.

## Buyer notifications (T31)

Connect to `/hubs/payment` over HTTPS/WebSocket with a valid access token using the existing cookie/header authentication or `?access_token=...`. Query tokens are accepted only under `/hubs`. JWT's `sub` claim becomes the normalized SignalR user ID through `JwtUserIdProvider`; SignalR owns the connection mapping and disconnect cleanup. The payment connection closes when authentication expires.

The `PaymentSuccessEvent` consumer calls `PaymentNotificationService`, which resolves the persisted buyer from the parent order and sends `PaymentSuccess` to that user's connections:

```json
{ "paymentId": "<UUID>", "orderId": "<parent-order UUID>", "amount": 100000, "paymentMethod": "VietQR" }
```

`JoinPayment(paymentId)` remains available and validates ownership before adding a connection to the legacy payment group. Notifications are user-targeted, so reconnects do not depend on rejoining that group. Clients filter by `paymentId` to distinguish multiple orders/tabs.

The existing `AddStackExchangeRedis` backplane uses `Redis:ConnectionString` and the `NovaLive_SignalR` channel prefix. All replicas must use the same Redis and compatible configuration. Consumer deduplication uses the existing idempotency service; failed notification attempts release the claim for retry. Polling remains the durable recovery path when a buyer was offline.

## Rating worker (T45)

`RatingCalculationWorker` runs at startup and every five minutes in the API host. It groups visible reviews by shop and calculates `round(sum(rating) / count, 2)`, then updates only changed `Shops.rating_avg` / `rating_count`. Empty aggregates become zero. Reviews from all of a shop's SKUs/SPUs contribute with equal review weight; hiding the last review clears the cached rating.

The scan compares database aggregates instead of relying on a process-local timestamp cursor, so edits, moderation, late commits and restarts cannot permanently skip a change. Aggregation runs in SQL; only summary rows are loaded. PostgreSQL's transaction-scoped advisory lock prevents overlapping replicas from writing summaries out of order. The transaction rolls back on failure and the next tick retries. No schema migration is required because the shop summary columns already exist.

## Verification

Tests cover initiation ownership/method/expiry, server amounts, COD, provider failures, retry deadlines, live DB polling, real HTTP routes, JWT query-token boundaries, real SignalR WebSockets with multiple buyer connections, notification deduplication/retry, PNG generation, weighted rating averages, edits, hidden reviews, empty aggregates and backdated reviews.

External provider calls and the production Redis backplane are isolated in automated tests. Live gateway credentials, provider callbacks and cross-container Redis delivery require environment integration testing.

Implementation references: [ASP.NET Core SignalR authentication](https://learn.microsoft.com/aspnet/core/signalr/authn-and-authz), [QRCoder PNG renderer](https://github.com/Shane32/QRCoder/blob/master/QRCoder/PngByteQRCode.cs).
