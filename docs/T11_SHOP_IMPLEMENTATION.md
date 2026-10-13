# T11 — Shop registration, KYC references and warehouses

Implemented on 2026-10-13. Scope: FR-SHOP-001/002/003/008/009 and the shop registration part of AC-002.

## Behavior and decisions

- Registration requires `shops:register` and an Active user. Shop, Pending verification and a sanitized `ShopRegisteredEvent` outbox record are saved in one `ITransactionalCommand`. No Seller role, wallet or `Users.is_seller` update occurs.
- The owner check includes soft-deleted shops. PostgreSQL 23505 for the owner unique index becomes HTTP 400 with exactly `Mỗi tài khoản chỉ được sở hữu tối đa 1 gian hàng`. If both owner and slug collide, owner wins even when PostgreSQL reports the slug index first. Other slug collisions return 409.
- Slugs use the existing `SlugHelper`; optional `slug` falls back to `shopName`. Both checks include soft-deleted rows because both indexes are globally unique.
- GET `/shops/me` includes Pending shops, latest verification status/reviewer note and a masked bank account. Full KYC URLs and account numbers are never returned. Short account numbers (4 characters or fewer) are completely masked.
- Pending and Active shops may update their profile/warehouses. Suspended, Banned and Closed shops cannot mutate either. Owner routes use `[Authorize]`, `RequireAuthenticated` and database ownership rather than `shops:manage_own`, which is seeded only for Seller. This deliberately allows a Pending Buyer with no `shop_id` claim. No RBAC grants were broadened.
- Profile updates only bind `shopName`, `description`, `logoUrl`, `bannerUrl`, `phone`, `email`. Additional JSON fields cannot update owner, status, slug, KYC or bank data.
- Warehouses require and persist carrier `provinceId`, `districtId`, `wardCode`; positive IDs and bounded ward code are validated, but live carrier directory membership is outside T11.
- First warehouse becomes primary and, absent a return selection, the default return warehouse. Selecting a new primary/return clears the previous selection atomically. A current default remains selected when its request flag is false; select another warehouse to replace it. Deleting the only primary is rejected; otherwise the oldest remaining warehouse (ID tie-break) becomes primary. Deleted default returns fall back to primary.
- PostgreSQL transaction advisory locks keyed by owner serialize warehouse/profile mutations, including concurrent creation of the first warehouse. Partial unique indexes provide database enforcement. Defaults are cleared and flushed before promotion inside the same transaction to avoid per-statement unique conflicts.
- Cross-shop address IDs return 404. `/me` handlers derive the shop from the authenticated owner, never from a client-supplied shop ID.
- Follow/unfollow require `shops:follow`, accept only Active, non-deleted shops and reject self-follow. Repeated calls succeed. Follow 23505 is absorbed after EF's transaction savepoint rollback; the losing insert is detached. Unfollow uses an atomic set-based delete, so concurrent deletes remain idempotent.
- Public detail counts actual Active, non-deleted SPUs and followers and returns `ratingCount`, `productsCount`, `isFollowing`. It never loads verification data. Phone, email and warehouses remain public because the existing `ShopDetailResponse` explicitly contains those fields.

## Contracts and routes

Existing names were preserved: `RegisterShopRequest`, `UpdateShopProfileRequest`, `CreateShopAddressRequest`, `ShopResponse`, `ShopDetailResponse`, `ShopAddressResponse`.

- Registration keeps `kycFrontUrl`/`kycBackUrl` (the actual DTO names), and adds optional `slug`, `taxCode`, `bankBranch`, `businessLicense`.
- Legacy `bankHolder` remains optional for compatibility, but is not persisted: there is no bank-holder column in `ShopVerification`. It is not interpreted as a bank branch. Clients should send `bankBranch` separately.
- Address request/response now expose carrier IDs/code. `CreateShopAddressRequest` is reused for PUT, since the fields are identical.
- Canonical routes are `/api/v1/shops/register`, `/shops/me`, `/shops/me/addresses[/{id}]`, `/shops/{shopId}`, `/shops/{shopId}/follow`.
- GET/PUT `/api/v1/seller/shop/me` and GET/POST/PUT/DELETE `/api/v1/seller/shop/addresses[/{id}]` are aliases to the same handlers.
- Create endpoints return 201; other successful operations return 200. OpenAPI declares typed ApiResponse envelopes and 400/401/403/404/409 outcomes, with public GET marked AllowAnonymous.

## KYC URL configuration

T11 accepts references to objects already uploaded by T12. It does not upload, fetch, re-sign or modify URLs. Configure complete, trusted bucket prefixes:

```json
{
  "KycUrls": {
    "AllowedPrefixes": [
      "https://cdn.example.com/kyc-docs/",
      "http://localhost:9000/kyc-docs/"
    ]
  }
}
```

Use the real deployment hostname. Environment equivalent: `KycUrls__AllowedPrefixes__0=https://cdn.example.com/kyc-docs/`. The default list is empty and rejects all KYC URLs until configured. Host, scheme, port and bucket path boundary must match; userinfo, fragments and ambiguous encoded separators are rejected. Presigned query strings are accepted and stored verbatim. Legitimate encoded filenames are supported. The prefix form assumes path-style CDN/MinIO bucket URLs; configure the reverse proxy accordingly.

Example registration body (actual DTO field names):

```json
{
  "shopName": "Nova Shop",
  "slug": "nova-shop",
  "description": "Shop description",
  "phone": "0912345678",
  "email": "shop@example.com",
  "kycFrontUrl": "https://cdn.example.com/kyc-docs/owner/front.png",
  "kycBackUrl": "https://cdn.example.com/kyc-docs/owner/back.png",
  "bankAccount": "123456789",
  "bankName": "Bank",
  "bankBranch": "Branch"
}
```

## Migration

New migration: `20261013015207_AddShopAddressDefaultIndexes`.

- Adds unique `ix_shop_addresses_primary` on `shop_id WHERE is_primary = TRUE`.
- Adds unique `ix_shop_addresses_return` on `shop_id WHERE is_return = TRUE`.
- No old migrations were edited. No migration was applied to the user's database.
- The generator also proposed dropping `cart_items.updated_at`: the current CartItem model already lacks that historical column. The physical DROP was deliberately removed as outside T11; the generated snapshot/designer reflect the current model, while the historical database column remains untouched. This existing cart schema drift still needs a separate cart fix. A test checks that the resulting snapshot matches the PostgreSQL model and that this migration contains only the two shop indexes.
- Before applying to an existing database, identify duplicate defaults. Migration intentionally fails rather than silently choosing among existing defaults:

```sql
SELECT shop_id FROM public.shop_addresses GROUP BY shop_id
HAVING count(*) FILTER (WHERE is_primary) > 1
    OR count(*) FILTER (WHERE is_return) > 1;
```

Resolve any returned rows according to operational data ownership before migration.

## Verification

Final result: build succeeded (0 errors; only the pre-existing OpenAPI warnings on the incremental final build). Full solution tests: **327 passed, 0 failed, 5 skipped** (24 Domain, 140 Application, 109 Infrastructure passed + 5 provider skips, 54 API). Three of the skipped PostgreSQL tests are new T11 tests; two are existing auth tests. No new code warnings were introduced.

Commands (single MSBuild node is reliable in this Windows environment):

```powershell
dotnet build NovaLive.sln --no-restore -m:1 /p:UseSharedCompilation=false
dotnet test NovaLive.sln --no-restore -m:1 /p:UseSharedCompilation=false
```

The test runner needs local loopback access for VSTest/testhost; it was run outside the sandbox after the sandbox run timed out before test execution. SQLite test dependency is pinned to cached version 10.0.12. NuGet was unavailable, so the added test dependency was restored from the existing local package cache.

Coverage includes registration, unchanged KYC, no role/wallet creation, soft-deleted ownership, normalized slug conflicts, validation and URL boundary cases, owner authorization, Pending/disabled shop statuses, address default switching/deletion/carrier codes, follow idempotency, sanitized public projection, real HTTP routing/aliases/RBAC, rollback after a forced KYC insert failure, OpenAPI response metadata, actual unique indexes, and PostgreSQL exception mapping.

Three new provider-specific tests exercise simultaneous registration, follow and first-warehouse creation. They require `NOVALIVE_TEST_POSTGRES` pointing to a disposable PostgreSQL server with database-creation privileges and create/delete only their own uniquely named test databases. They are skipped when no server is configured. Docker was not running in this environment; simulated 23505 tests and SQLite transaction/index tests still execute.

## Documentation differences and remaining work

- Update API docs to make `/shops/me...` canonical, retain seller aliases, and describe Pending owner access rather than Seller-only authorization.
- Update API examples from `idCardFront`/`idCardBack` to existing contract names `kycFrontUrl`/`kycBackUrl`, and clarify legacy `bankHolder` versus stored `bankBranch`.
- `ShopVerification.Rejected` exists, but `ShopStatus.Rejected` does not. A rejected owner's unique shop prevents a second registration. Product/team must choose a resubmit-KYC flow for the existing shop or a different lifecycle; T11 does not change this rule.
- SRS AC-002 includes approval and product creation, while the task breakdown reuses AC-002 for registration and product inventory separately. T11 covers only its registration acceptance criteria; T14 approval is unchanged.
- README says 45 tables and the current IAppDbContext has 45 DbSets; no new table was added. Do not change that count to 46 based solely on the task example.
- `ShopRegisteredEvent` is stored atomically without KYC/bank values. Existing OutboxBackgroundWorker still marks records processed without publishing (pre-existing stub). Actual event delivery/notification wiring remains a separate infrastructure task; T11 does not claim notifications are delivered.
- Existing warnings: NU1903 for Microsoft.OpenApi 2.0.0 in API/API.Tests, and CS8602 in SearchProductsQueryHandlerTests. No unrelated package upgrade/test change was made.

## Changed files

- `docs/T11_SHOP_IMPLEMENTATION.md`
- `src/NovaLive.Api/Controllers/Shops/ShopsController.cs`
- `src/NovaLive.Api/appsettings.json`
- `src/NovaLive.Application/Abstractions/Persistence/IShopPersistence.cs`
- `src/NovaLive.Application/Common/Events/ShopRegisteredEvent.cs`
- `src/NovaLive.Application/DependencyInjection.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/AddShopAddress/AddShopAddressCommand.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/AddShopAddress/AddShopAddressCommandHandler.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/AddShopAddress/AddShopAddressCommandValidator.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/DeleteShopAddress/DeleteShopAddressCommand.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/DeleteShopAddress/DeleteShopAddressCommandHandler.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/DeleteShopAddress/DeleteShopAddressCommandValidator.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/FollowShop/FollowShopCommand.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/FollowShop/FollowShopCommandHandler.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/FollowShop/FollowShopCommandValidator.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/RegisterShop/RegisterShopCommand.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/RegisterShop/RegisterShopCommandHandler.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/RegisterShop/RegisterShopCommandValidator.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/UnfollowShop/UnfollowShopCommand.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/UnfollowShop/UnfollowShopCommandHandler.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/UnfollowShop/UnfollowShopCommandValidator.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/UpdateShopAddress/UpdateShopAddressCommand.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/UpdateShopAddress/UpdateShopAddressCommandHandler.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/UpdateShopAddress/UpdateShopAddressCommandValidator.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/UpdateShopInfo/UpdateShopInfoCommand.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/UpdateShopInfo/UpdateShopInfoCommandHandler.cs`
- `src/NovaLive.Application/UseCases/Shops/Commands/UpdateShopInfo/UpdateShopInfoCommandValidator.cs`
- `src/NovaLive.Application/UseCases/Shops/KycUrlOptions.cs`
- `src/NovaLive.Application/UseCases/Shops/Queries/GetMyShop/GetMyShopQuery.cs`
- `src/NovaLive.Application/UseCases/Shops/Queries/GetMyShop/GetMyShopQueryHandler.cs`
- `src/NovaLive.Application/UseCases/Shops/Queries/GetMyShop/GetMyShopQueryValidator.cs`
- `src/NovaLive.Application/UseCases/Shops/Queries/GetShopAddresses/GetShopAddressesQuery.cs`
- `src/NovaLive.Application/UseCases/Shops/Queries/GetShopAddresses/GetShopAddressesQueryHandler.cs`
- `src/NovaLive.Application/UseCases/Shops/Queries/GetShopAddresses/GetShopAddressesQueryValidator.cs`
- `src/NovaLive.Application/UseCases/Shops/Queries/GetShopPublic/GetShopPublicQuery.cs`
- `src/NovaLive.Application/UseCases/Shops/Queries/GetShopPublic/GetShopPublicQueryHandler.cs`
- `src/NovaLive.Application/UseCases/Shops/Queries/GetShopPublic/GetShopPublicQueryValidator.cs`
- `src/NovaLive.Application/UseCases/Shops/ShopAccess.cs`
- `src/NovaLive.Application/UseCases/Shops/ShopAddressDefaults.cs`
- `src/NovaLive.Application/UseCases/Shops/ShopAddressRequestValidator.cs`
- `src/NovaLive.Application/UseCases/Shops/ShopErrors.cs`
- `src/NovaLive.Application/UseCases/Shops/ShopMappings.cs`
- `src/NovaLive.Application/UseCases/Shops/ShopValidation.cs`
- `src/NovaLive.Contracts/V1/Shops/ShopRequests.cs`
- `src/NovaLive.Contracts/V1/Shops/ShopResponses.cs`
- `src/NovaLive.Domain/Shops/Shop.cs`
- `src/NovaLive.Domain/Shops/ShopAddress.cs`
- `src/NovaLive.Domain/Shops/ShopFollower.cs`
- `src/NovaLive.Domain/Shops/ShopVerification.cs`
- `src/NovaLive.Infrastructure/DependencyInjection.cs`
- `src/NovaLive.Infrastructure/Persistence/Configurations/ShopConfiguration.cs`
- `src/NovaLive.Infrastructure/Persistence/Migrations/20261013015207_AddShopAddressDefaultIndexes.Designer.cs`
- `src/NovaLive.Infrastructure/Persistence/Migrations/20261013015207_AddShopAddressDefaultIndexes.cs`
- `src/NovaLive.Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs`
- `src/NovaLive.Infrastructure/Persistence/ShopPersistence.cs`
- `tests/NovaLive.Api.Tests/Shops/ShopHttpTests.cs`
- `tests/NovaLive.Application.Tests/NovaLive.Application.Tests.csproj`
- `tests/NovaLive.Application.Tests/Shops/ShopTestContext.cs`
- `tests/NovaLive.Application.Tests/Shops/ShopValidationTests.cs`
- `tests/NovaLive.Application.Tests/Shops/ShopWorkflowTests.cs`
- `tests/NovaLive.Infrastructure.Tests/Shops/PostgresShopConcurrencyTests.cs`
- `tests/NovaLive.Infrastructure.Tests/Shops/ShopPersistenceTests.cs`
