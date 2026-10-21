# Inventory lifecycle implementation

## T47: payment inventory confirmation

- `InventorySyncConsumer` dispatches only the payment ID; the handler reads authoritative payment/order/line data. Failed Results become transport exceptions, with three endpoint retries (1s/5s/15s), followed by MassTransit's default `_error` queue.
- `ConfirmInventorySaleCommand` uses the existing idempotency pipeline. Execution order is IdempotencyBehavior outside TransactionBehavior: successful Result markers are cached for 24h only after commit. Redis key: `idempotency:response:inventory-sale:{paymentId:N}`. Failures remain retryable.
- DB guard: lock the parent and inventory rows in ascending SKU order, then query `InventoryHistories` by `(Order, sub_order_id, sku_id, SaleConfirmed)`. Group repeated SKU lines within each SubOrder. Conditional atomic UPDATE changes both counters and increments the existing concurrency version. Ledger snapshots come from the updated stock while row locks are held.
- `ref_type = Order`, `ref_id = sub_order_id`; deterministic `operation_id` is the first 16 SHA-256 bytes of `inventory:Order:{subOrderId:N}:{skuId:N}:{changeType}`. The existing unique operation-ID index is reused; no migration is necessary. Ledger insertion and all shops/SKUs share the command transaction.
- Reusable inventory repository methods `ConfirmReservedSaleAsync` and `ReleaseReservedAsync` share one conditional transition. They only access Inventories. The handler writes the separate history repository. Existing unrelated stock methods are preserved.
- T27 already exists: `OrderItem.FlashSaleItemId` and persisted reservation state link to Flash Sale. Reuse `IFlashSaleStockService.ConfirmSoldAsync`; it is idempotent even though online webhooks already call it.

## Mandatory code survey and boundaries

- `PaymentSuccessEvent` currently lives in `Contracts/V1/Payments/PaymentResponses.cs`: PaymentId, ParentOrderId, OrderCode, Amount, PaymentMethod, TransactionRef, PaidAt. MoMo and VietQR handlers write it to Outbox. T48 `CODReconciliationConsumer` marks COD paid after delivery/revenue reconciliation but does **not** publish PaymentSuccessEvent; consequently COD stock confirmation is still missing. No trigger was added.
- `IIdempotencyService`/`RedisIdempotencyService` support acquisition markers and cached responses with prefix `idempotency:`. Acquisition is distinct from completion. The new command uses the existing response cache instead of introducing a new protocol.
- `TransactionManager` saves and commits successful Results, rolls back failed Results/exceptions and clears tracking. `IUnitOfWork` exposes SaveChanges; handlers do not inject DbContext.
- Existing MassTransit consumers: OrderPlaced, PaymentSuccess notification, COD reconciliation. Existing RabbitMQ registration uses ConfigureEndpoints without a global retry policy. T47 adds only its own consumer definition.
- Online checkout already sets `expired_at = created_at + 15 minutes` in `Payment.Create`; COD has null expiry. T49 must retain the legacy null-expiry fallback.
- Online webhooks lock Payment rows (`FOR UPDATE`) and call `MarkSuccess`, which rejects Expired/Failed/Refunded. They do **not** issue a symmetric conditional SQL `Pending -> Success`. Lock order is inconsistent: VietQR and the order-code MoMo path lock parent then payment; the Guid MoMo path locks payment then parent. This is an existing potential deadlock/retry gap, outside scope.
- Cancelled SubOrders are skipped with a warning on late payment; actual collected funds need refund/reconciliation review. Inventory processing does not change Payment/SubOrder state or wallets.
- Domain enums used here match the documented CHECK lists. Existing `InventoryChangeType.SaleCancelled` is absent from the database-design document's CHECK list; unchanged outside this implementation.
- Tests use xUnit/FluentAssertions/Moq. A SQLite transaction fixture and optional real PostgreSQL fixture (`NOVALIVE_TEST_POSTGRES`) exist; no TestContainers harness/package exists. No new PostgreSQL concurrency tests are added under the requested restriction. Existing PostgreSQL tests skip when that variable is absent.
- Existing `OrderCancelledEvent` is an anonymous Outbox payload from buyer cancellation. OutboxBackgroundWorker currently dispatches only OrderPlacedEvent, PaymentSuccessEvent and ShipmentDeliveredEvent, and marks other types processed without publishing. This notification gap is recorded, not changed here.
- Documents label T22/T27/T28/T29/T30/T32 as missing despite corresponding code being present. Task T27 suggests throwing on sold-out; mandatory coding standards require Results. Business logic allows stock confirmation at payment/delivery, whereas SRS FR-CAT-010 requires confirmation at successful payment; T47 follows SRS. Commission/revenue formulas differ between business logic and existing settlement code, which this implementation does not alter.

## Validation

- Build uses `dotnet build --no-restore -m:1 -p:UseSharedCompilation=false`; disabling shared compilation avoids sandbox compiler cancellation.
- Tests must run outside the restricted process sandbox because testhost communication hangs inside it. No application behavior or security settings were changed for this.
- T47 coverage: valid/invalid domain transitions and stock invariants; command validator; multiple shops/SKUs with aggregation and ordering; replay without Redis; cancelled SubOrder/late payment; reservation mismatch; consumer mapping and failure propagation; real SQLite rollback and cache-eviction replay through MediatR and transaction/idempotency behaviors.
