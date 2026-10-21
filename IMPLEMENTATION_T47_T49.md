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

## T49: unpaid online payment timeout

- `TimeoutOrderRollbackWorker` uses PeriodicTimer, one scan scope and a fresh dispatch scope per payment. Each command receives its own transaction. Exceptions/failed Results are logged per payment and do not interrupt subsequent candidates; scan exceptions retry on the next tick. Cancellation is propagated. Structured logs expose expired pending candidate count, failures, and actual expired payments/cancelled SubOrders.
- `OrderTimeoutOptions` is shared in Application so the worker and command use the same timeout without reversing the dependency rule. Infrastructure binds/validates it from `OrderTimeout`; appsettings defaults: PollInterval 00:01:00, PaymentTimeout 00:15:00, BatchSize 100.
- Scan and claim use the same predicate: Pending, method MoMo/VietQR, and explicit expiry at/before now OR null expiry with creation at/before now minus timeout. Explicit future expiry never uses the creation fallback. COD is excluded at selection, handler, and conditional UPDATE boundaries.
- The handler reads the payment, locks the parent, then claims using conditional UPDATE Pending -> Expired (also rechecking method/deadline). A lost race changes nothing. Parent-first minimizes contention with existing cancellation/VietQR paths; the pre-existing reverse MoMo lock order remains a documented gap. Successful payments cannot match the claim predicate.
- Only PendingPayment SubOrders are locked/rechecked and cancelled by the new domain method, with PaymentTimeout, null actor and System history. Inventory rows are locked in global ascending SKU order, released by the shared atomic repository operation, and guarded by `(Order, sub_order_id, sku_id, ReserveRelease)` ledger entries. Everything rolls back together on failed Results or exceptions.
- Reuse T27 `ReleaseReservedAsync` in the same transaction. Existing OrderItem linkage and persisted Flash Sale state remove the missing-link concern in the request. Flash Sale failures propagate to rollback the entire payment operation.
- Reuse the existing anonymous `OrderCancelledEvent` Outbox payload and event name, with changedByRole System and reason PaymentTimeout. Its dispatch/notification gap described above remains. No new event type, endpoint, wallet action or migration was introduced.
- Discount used_count/DiscountUsages rollback is unspecified and remains unchanged. Another existing gap: the MoMo failure branch marks Payment Failed and cancels the parent without releasing reservations; the requested Pending-only worker intentionally does not expand into Failed payment recovery.
- All new types use separate matching files. Existing `OrderErrors` was extracted from OrderSupport into its required matching file without changing error definitions; other pre-existing bundled types remain untouched. InventoryStockResult was likewise extracted from IInventoryRepository in T47 to keep the edited interface file compliant.
- Transaction rollback tests live in Infrastructure.Tests and use the real SQLite transaction manager/pipeline rather than mocked rollback assertions in Application.Tests. Application.Tests verify business selection, lost claims, ordering, existing ledger guard and error propagation.

## Files by commit

T47 — `4d3f9f3`, `feat(inventory): implement inventory sync consumer to deduct stock after payment (T47)`:

```text
IMPLEMENTATION_T47_T49.md (added)
src/NovaLive.Application/Abstractions/Persistence/Repositories/IInventoryHistoryRepository.cs
src/NovaLive.Application/Abstractions/Persistence/Repositories/IInventoryRepository.cs
src/NovaLive.Application/Abstractions/Persistence/Repositories/IPaymentRepository.cs
src/NovaLive.Application/Abstractions/Persistence/Repositories/InventoryStockResult.cs (added)
src/NovaLive.Application/UseCases/Inventory/Commands/ConfirmInventorySale/ConfirmInventorySaleCommand.cs (added)
src/NovaLive.Application/UseCases/Inventory/Commands/ConfirmInventorySale/ConfirmInventorySaleCommandHandler.cs (added)
src/NovaLive.Application/UseCases/Inventory/Commands/ConfirmInventorySale/ConfirmInventorySaleCommandValidator.cs (added)
src/NovaLive.Application/UseCases/Inventory/InventoryErrors.cs
src/NovaLive.Application/UseCases/Inventory/InventoryLedgerOperation.cs (added)
src/NovaLive.Domain/Inventory/Inventory.cs
src/NovaLive.Domain/Inventory/InventoryReservationErrors.cs
src/NovaLive.Infrastructure/DependencyInjection.cs
src/NovaLive.Infrastructure/Persistence/Repositories/InventoryHistoryRepository.cs
src/NovaLive.Infrastructure/Persistence/Repositories/InventoryRepository.cs
src/NovaLive.Infrastructure/Persistence/Repositories/PaymentRepository.cs
src/NovaLive.Infrastructure/System/InventorySyncConsumer.cs (added)
src/NovaLive.Infrastructure/System/InventorySyncConsumerDefinition.cs (added)
tests/NovaLive.Application.Tests/Inventory/ConfirmInventorySaleCommandHandlerTests.cs (added)
tests/NovaLive.Application.Tests/Inventory/InventorySaleTestContext.cs (added)
tests/NovaLive.Domain.Tests/Inventory/ReservedStockTests.cs (added)
tests/NovaLive.Infrastructure.Tests/Inventory/InventoryLifecycleFixture.cs (added)
tests/NovaLive.Infrastructure.Tests/Inventory/InventorySalePersistenceTests.cs (added)
tests/NovaLive.Infrastructure.Tests/System/InventorySyncConsumerTests.cs (added)
```

T49 — `feat(orders): add timeout order rollback worker to release reserved stock (T49)`:

```text
IMPLEMENTATION_T47_T49.md
src/NovaLive.Api/appsettings.json
src/NovaLive.Application/Abstractions/Persistence/Repositories/IPaymentRepository.cs
src/NovaLive.Application/UseCases/Orders/OrderSupport.cs (extract existing OrderErrors only)
src/NovaLive.Application/UseCases/Orders/OrderErrors.cs (added)
src/NovaLive.Application/UseCases/Orders/OrderTimeoutOptions.cs (added)
src/NovaLive.Application/UseCases/Orders/Commands/ExpireUnpaidPayment/ExpireUnpaidPaymentCommand.cs (added)
src/NovaLive.Application/UseCases/Orders/Commands/ExpireUnpaidPayment/ExpireUnpaidPaymentCommandHandler.cs (added)
src/NovaLive.Application/UseCases/Orders/Commands/ExpireUnpaidPayment/ExpireUnpaidPaymentCommandValidator.cs (added)
src/NovaLive.Domain/Orders/SubOrder.cs
src/NovaLive.Domain/Payments/Payment.cs
src/NovaLive.Infrastructure/DependencyInjection.cs
src/NovaLive.Infrastructure/Persistence/Repositories/PaymentRepository.cs
src/NovaLive.Infrastructure/System/TimeoutOrderRollbackWorker.cs (added)
tests/NovaLive.Application.Tests/UseCases/Orders/ExpireUnpaidPaymentCommandHandlerTests.cs (added)
tests/NovaLive.Infrastructure.Tests/Orders/OrderTimeoutOptionsTests.cs (added)
tests/NovaLive.Infrastructure.Tests/Orders/TimeoutOrderRollbackWorkerTests.cs (added)
tests/NovaLive.Infrastructure.Tests/Orders/TimeoutPaymentRepositoryTests.cs (added)
tests/NovaLive.Infrastructure.Tests/Orders/TimeoutRollbackPersistenceTests.cs (added)
```

The pre-existing deletion of docs/T27_FLASH_SALE_IMPLEMENTATION.md was preserved and excluded from both commits. No docs file was edited, and no changes were pushed.

## Final validation results

`dotnet test --no-restore -m:1 -p:UseSharedCompilation=false -v minimal` completed successfully for the entire solution:

| Project | Passed | Skipped | Failed |
| --- | ---: | ---: | ---: |
| Domain.Tests | 135 | 0 | 0 |
| Application.Tests | 468 | 0 | 0 |
| Infrastructure.Tests | 221 | 22 | 0 |
| Api.Tests | 144 | 0 | 0 |
| Total | 968 | 22 | 0 |

All 46 new test cases passed. The 22 skipped cases are existing optional PostgreSQL tests requiring NOVALIVE_TEST_POSTGRES, covering Auth (2), Shops (3), Carts (2), Inventory (4), FlashSales (10), Checkout (1). PostgreSQL row-lock concurrency and live RabbitMQ error-queue delivery were not exercised; SQLite validates repository conditional updates and complete transaction rollback. No TestContainers harness is available, so no new PostgreSQL concurrency tests were written.

Build succeeded with zero errors. A rebuild reports six pre-existing CS8602 warnings in UploadMediaTests, SearchProductsQueryHandlerTests and the MoMo/VietQR handler tests; there are no new warnings in this implementation. `git diff --check` passed.
