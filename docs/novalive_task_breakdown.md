# 📋 BẢNG PHÂN RÃ CÔNG VIỆC & KẾ HOẠCH SPRINT (NOVALIVE)

> **Tài liệu**: Task Breakdown & Sprint Recommendation chuẩn Agile/Scrum  
> **Dự án**: NovaLive — E-Commerce Multi-vendor Marketplace + Livestream Commerce  
> **Tech Stack**: .NET 10 Web API + PostgreSQL 17 + Redis 7 + RabbitMQ + MinIO + SignalR + Agora RTC  
> **Kiến trúc**: Clean Architecture + DDD + CQRS (MediatR) + Outbox Pattern  
> **Team**: 3 thành viên — **Dev A** (Auth/Orders/Payment), **Dev B** (Product/Shop/Discount), **Dev C** (Realtime/Infra/Workers)  
> **Quy chuẩn mã Task**: Đánh số tuần tự từ **T01** đến **T61** (T01–T07: Đã hoàn thành; T08–T55: Backend/Infra/DevOps; T56–T61: Frontend Applications)  
> **SRS Reference**: [`software_requirements_specification.md`](./software_requirements_specification.md) v1.0  
> **Contracts Reference**: `src/NovaLive.Contracts/V1/*` (Đã hoàn thiện 100% Request/Response DTOs)

---

# PART 1 — SYSTEM ANALYSIS & SRS MAPPING

### 1.1 Project Overview & Baseline Status

Hệ thống **NovaLive** đã được hoàn thiện hạ tầng cốt lõi (Baseline Completed):
- **Contracts**: Đã định nghĩa đầy đủ 100% Request & Response DTOs tại `NovaLive.Contracts/V1/*` cho các domains (Auth, Users, Shops, Products, Carts, Orders, Payments, Shipping, Returns, Discounts, FlashSales, Livestreams, Reviews, Dashboards).
- **Domain & Database**: 46 bảng PostgreSQL 17 đầy đủ FKs, GIN index, tsvector, Composite index và Initial Migration đã tạo.
- **Infrastructure Baseline**: Implement xong `RedisCacheService`, `MassTransitMessageBus`, `RedisIdempotencyService`, `PostgresProductQueryService`, `OutboxBackgroundWorker`.
- **Pipeline Behaviors**: 6 MediatR Pipeline Behaviors (`Validation`, `Logging`, `Transaction`, `Authorization`, `Idempotency`, `Performance`).

---

# PART 2 — FEATURE / MODULE BREAKDOWN

```text
NovaLive
│
├── EPIC 00: Foundation, Infrastructure & Baseline (COMPLETED)
│   ├── T01: Clean Architecture Solution Setup (.NET 10)
│   ├── T02: Docker Compose Dev Stack (PG + Redis + RabbitMQ + MinIO)
│   ├── T03: AppDbContext & Initial Migration (46 tables)
│   ├── T04: Auto Migration Runner on API Startup
│   ├── T05: MediatR Pipeline Behaviors & Result Pattern
│   ├── T06: Global Exception Handler & OpenAPI Scalar
│   └── T07: Abstraction Interfaces & Complete V1 Contracts DTOs
│
├── EPIC 01: Auth, Identity & RBAC (GỘP GỌN 2 TASKS)
│   ├── T08: [BE/DB/API] Complete Auth Engine & RBAC Pipeline (Register, OTP, Login, Rotation, Reset, Endpoints, Blacklist Middleware)
│   └── T09: [BE/DB] Seed System Roles & Initial Permissions
│
├── EPIC 02: User Profile & Shop Management
│   ├── T10: [BE/API] User Profile & Address Book Management
│   ├── T11: [BE/API] Shop Registration, KYC Upload & Warehouse Management
│   ├── T12: [INFRA] MinIO File Storage Adapter & Multi-bucket Upload
│   ├── T13: [BE/API] Shop Wallet, Financial Ledger & Payout Request
│   └── T14: [BE/API] Admin Shop Onboarding, Ban Policy & Payout Approval
│
├── EPIC 03: Product Catalog & Inventory
│   ├── T15: [BE/API] Multi-level Category Tree Management
│   ├── T16: [BE/API] Product SPU & SKU Multi-variant Management
│   ├── T17: [BE/API] Inventory 2-State Management & Ledger Logging
│   └── T18: [BE/API] Public Product Search Engine (Full-text + Trigram)
│
├── EPIC 04: Cart & Checkout Engine
│   ├── T19: [BE/API] Cart Multi-shop Management Use Cases
│   ├── T20: [DOMAIN] PriceCalculator Domain Service (3-tier Proration Engine)
│   ├── T21: [BE] Calculate Checkout Draft Query (Realtime Preview)
│   ├── T22: [BE] Checkout Command Handler (Atomic UoW 8-Step Transaction)
│   └── T23: [API] Order Controller Endpoints & Seller Order Operations
│
├── EPIC 05: Discounts & Vouchers (3-tier)
│   ├── T24: [BE/API] Discount & Voucher Lifecycle Management
│   └── T25: [BE/API] Voucher Validation & Proration Calculation Query
│
├── EPIC 06: Flash Sale (Atomic PostgreSQL)
│   ├── T26: [BE/API] Flash Sale Campaign & Registration Management
│   └── T27: [BE] Atomic Flash Sale Reserve (PG Condition UPDATE)
│
├── EPIC 07: Payment & Direct Settlement
│   ├── T28: [INFRA] MoMo Payment Gateway Adapter & IPN Handler
│   ├── T29: [INFRA] VietQR Payment Gateway Adapter & Webhook Handler
│   ├── T30: [BE/API] Payment Initiator & Realtime Status Query
│   ├── T31: [REALTIME] SignalR PaymentNotificationHub Integration
│   └── T32: [BE] Direct Payment Settlement & Shop Wallet Credit
│
├── EPIC 08: Shipping & Fulfillment
│   ├── T33: [INFRA] GHN Shipping Adapter & Webhook Integrator
│   ├── T34: [INFRA] GHTK & ViettelPost Shipping Adapters
│   ├── T35: [BE/API] Seller Shipping Fulfillment & Label PDF Generator
│   └── T36: [BE] Shipping Webhook Handler & Delivery Status Sync
│
├── EPIC 09: Returns & Disputes
│   ├── T37: [BE/API] Return Request Lifecycle (Buyer Claim ≤ 7 Days)
│   ├── T38: [BE/API] Seller Return Response & Inspection Confirmation
│   └── T39: [BE/API] Admin Dispute Arbitration & Refund Resolution
│
├── EPIC 10: Livestream Commerce (Agora RTC + SignalR)
│   ├── T40: [INFRA] Agora RTC Token Generation Service
│   ├── T41: [BE/API] Livestream Session Lifecycle & Product Pinning
│   ├── T42: [REALTIME] SignalR LivestreamHub (Chat, Reaction, Pin, Viewers)
│   └── T43: [REALTIME] SignalR OrderNotificationHub Integration
│
├── EPIC 11: Reviews & Ratings
│   ├── T44: [BE/API] Review & Rating Management (Edit-once, Reply, Moderation)
│   └── T45: [WORKER] Rating Calculation Background Worker
│
├── EPIC 12: Background Workers & Event-Driven
│   ├── T46: [WORKER] OrderPlacedConsumer (Email/SMS + Push Noti)
│   ├── T47: [WORKER] InventorySyncConsumer (Deduct Stock After Paid)
│   ├── T48: [WORKER] CODReconciliationConsumer (COD Settlement & Wallet Credit)
│   └── T49: [WORKER] TimeoutOrderRollbackWorker & ProductSearchVectorConsumer
│
├── EPIC 13: Reports & Admin Dashboard
│   ├── T50: [BE/API] Seller Performance Dashboard & Wallet Analytics
│   └── T51: [BE/API] Admin Platform Executive Dashboard & Financial Reports
│
├── EPIC 14: Testing & DevOps Hardening
│   ├── T52: [TEST] Domain & Application Unit Tests
│   ├── T53: [TEST] Integration Tests (Auth, Checkout, Webhooks)
│   ├── T54: [TEST] End-to-End Tests (Full Purchase to Direct Settlement Cycle)
│   └── T55: [DEVOPS] Production Docker Stack (Nginx SSL + Health Checks)
│
└── EPIC 15: Frontend Web Applications (Next.js / React)
    ├── T56: [FE] Setup Frontend Boilerplate, Auth Hub & User Profile
    ├── T57: [FE] Buyer Marketplace & Product Search / Details Portal
    ├── T58: [FE] Cart, Multi-shop Checkout & Payment Flow
    ├── T59: [FE] Seller Center Portal (Shop, Products, Orders & Shipping)
    ├── T60: [FE] Livestream Commerce Portal (Agora RTC + SignalR Realtime)
    └── T61: [FE] Returns, Admin Operations & Executive Dashboard
```

---

# PART 3 — TASK LIST (T01–T61)

| ID | Task Name | Type | Layer | Status | Priority | Assign | Dependency |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **T01** | Setup Clean Architecture Multi-project Solution (.NET 10) | ARCH | ARCH | **COMPLETED** | P0 | - | - |
| **T02** | Docker Compose Dev Stack (PG + Redis + RabbitMQ + MinIO) | DEVOPS | DEVOPS | **COMPLETED** | P0 | - | T01 |
| **T03** | AppDbContext, EF Core Configurations & Initial Migration | FEATURE | DB | **COMPLETED** | P0 | - | T01 |
| **T04** | Auto Migration Runner on API Startup | FEATURE | BE | **COMPLETED** | P0 | - | T03 |
| **T05** | MediatR Pipeline Behaviors & Result Pattern | FEATURE | BE | **COMPLETED** | P0 | - | T01 |
| **T06** | Global Exception Handler & OpenAPI Scalar | FEATURE | BE/API | **COMPLETED** | P0 | - | T01 |
| **T07** | Abstraction Interfaces & Complete V1 Contracts DTOs | FEATURE | CONTRACTS | **COMPLETED** | P0 | - | T01 |
| **T08** | Complete Auth Engine & RBAC Pipeline (Register, OTP, Login, Rotation, Reset, Endpoints, Middleware) | FEATURE | BE/API/INFRA | MISSING | P0 | Dev A | T03 |
| **T09** | Seed System Roles (Admin, Seller, Buyer) & Permissions | FEATURE | DB | MISSING | P0 | Dev A | T03 |
| **T10** | User Profile & Address Book Management | FEATURE | BE/API | MISSING | P1 | Dev B | T08 |
| **T11** | Shop Registration, KYC Upload & Warehouse Management | FEATURE | BE/API | MISSING | P1 | Dev B | T08, T12 |
| **T12** | MinIO File Storage Adapter & Multi-bucket Upload | FEATURE | INFRA | MISSING | P1 | Dev C | T02 |
| **T13** | Shop Wallet, Financial Ledger & Payout Request | FEATURE | BE/API | MISSING | P1 | Dev B | T11 |
| **T14** | Admin Shop Onboarding, Ban Policy & Payout Approval | FEATURE | BE/API | MISSING | P1 | Dev B | T08, T11 |
| **T15** | Multi-level Category Tree Management | FEATURE | BE/API | MISSING | P1 | Dev B | T08 |
| **T16** | Product SPU & SKU Multi-variant Management | FEATURE | BE/API | COMPLETED | P1 | Dev B | T11, T15 |
| **T17** | Inventory 2-State Management & Ledger Logging | FEATURE | BE/API | COMPLETED | P1 | Dev B | T16 |
| **T18** | Public Product Search Engine (Full-text + Trigram) | FEATURE | BE/API | MISSING | P1 | Dev B | T16 |
| **T19** | Cart Multi-shop Management Use Cases | FEATURE | BE/API | MISSING | P1 | Dev A | T16, T08 |
| **T20** | PriceCalculator Domain Service (3-tier Proration Engine) | FEATURE | DOMAIN | MISSING | P0 | Dev A | T07 |
| **T21** | Calculate Checkout Draft Query (Realtime Preview) | FEATURE | BE | MISSING | P0 | Dev A | T19, T20 |
| **T22** | Checkout Command Handler (Atomic UoW 8-Step Transaction) | FEATURE | BE | MISSING | P0 | Dev A | T21, T32 |
| **T23** | Order Controller Endpoints & Seller Order Operations | FEATURE | API | MISSING | P0 | Dev A | T22 |
| **T24** | Discount & Voucher Lifecycle Management | FEATURE | BE/API | MISSING | P1 | Dev B | T08, T11 |
| **T25** | Voucher Validation & Proration Calculation Query | FEATURE | BE/API | MISSING | P1 | Dev B | T24 |
| **T26** | Flash Sale Campaign & Registration Management | FEATURE | BE/API | MISSING | P2 | Dev B | T16, T08 |
| **T27** | Atomic Flash Sale Reserve (PG Condition UPDATE) | FEATURE | BE | MISSING | P2 | Dev B | T26, T22 |
| **T28** | MoMo Payment Gateway Adapter & IPN Handler | FEATURE | INFRA | MISSING | P0 | Dev C | T07 |
| **T29** | VietQR Payment Gateway Adapter & Webhook Handler | FEATURE | INFRA | MISSING | P0 | Dev C | T07 |
| **T30** | Payment Initiator & Realtime Status Query | FEATURE | BE/API | MISSING | P0 | Dev A | T23, T28, T29 |
| **T31** | SignalR PaymentNotificationHub Integration | FEATURE | REALTIME | MISSING | P0 | Dev C | T30 |
| **T32** | Direct Payment Settlement & Shop Wallet Credit | FEATURE | BE | MISSING | P0 | Dev A | T23 |
| **T33** | GHN Shipping Adapter & Webhook Integrator | FEATURE | INFRA | MISSING | P1 | Dev C | T07 |
| **T34** | GHTK & ViettelPost Shipping Adapters | FEATURE | INFRA | MISSING | P1 | Dev C | T07 |
| **T35** | Seller Shipping Fulfillment & Label PDF Generator | FEATURE | BE/API | MISSING | P1 | Dev C | T33, T34, T23 |
| **T36** | Shipping Webhook Handler & Delivery Status Sync | FEATURE | BE | MISSING | P1 | Dev C | T35, T32 |
| **T37** | Return Request Lifecycle (Buyer Claim ≤ 7 Days) | FEATURE | BE/API | MISSING | P1 | Dev A | T36 |
| **T38** | Seller Return Response & Inspection Confirmation | FEATURE | BE/API | MISSING | P1 | Dev A | T37 |
| **T39** | Admin Dispute Arbitration & Refund Resolution | FEATURE | BE/API | MISSING | P1 | Dev A | T38 |
| **T40** | Agora RTC Token Generation Service | FEATURE | INFRA | MISSING | P2 | Dev C | T07 |
| **T41** | Livestream Session Lifecycle & Product Pinning | FEATURE | BE/API | MISSING | P2 | Dev C | T40, T16, T08 |
| **T42** | SignalR LivestreamHub (Chat, Reaction, Pin, Viewers) | FEATURE | REALTIME | MISSING | P2 | Dev C | T41 |
| **T43** | SignalR OrderNotificationHub Integration | FEATURE | REALTIME | MISSING | P2 | Dev C | T23 |
| **T44** | Review & Rating Management (Edit-once, Reply, Moderation) | FEATURE | BE/API | MISSING | P2 | Dev B | T36, T08 |
| **T45** | Rating Calculation Background Worker | WORKER | WORKER | MISSING | P2 | Dev C | T44 |
| **T46** | OrderPlacedConsumer (Email/SMS + Push Noti) | WORKER | WORKER | MISSING | P1 | Dev C | T22 |
| **T47** | InventorySyncConsumer (Deduct Stock After Paid) | WORKER | WORKER | MISSING | P0 | Dev C | T30, T22 |
| **T48** | CODReconciliationConsumer (COD Settlement & Wallet Credit) | WORKER | WORKER | MISSING | P0 | Dev C | T32, T36 |
| **T49** | TimeoutOrderRollbackWorker & ProductSearchVectorConsumer | WORKER | WORKER | MISSING | P1 | Dev C | T22, T16 |
| **T50** | Seller Performance Dashboard & Wallet Analytics | FEATURE | BE/API | MISSING | P2 | Dev B | T32, T36 |
| **T51** | Admin Platform Executive Dashboard & Financial Reports | FEATURE | BE/API | MISSING | P2 | Dev B | T39, T48 |
| **T52** | Domain & Application Unit Tests | TEST | TEST | MISSING | P1 | All | T20, T22, T27 |
| **T53** | Integration Tests (Auth, Checkout, Webhooks) | TEST | TEST | MISSING | P1 | All | T08, T30, T36 |
| **T54** | End-to-End Tests (Full Purchase to Direct Settlement Cycle) | TEST | TEST | MISSING | P1 | All | T39, T48 |
| **T55** | Production Docker Stack (Nginx SSL + Health Checks) | DEVOPS | DEVOPS | MISSING | P1 | Dev C | T08, T02 |
| **T56** | Setup Frontend Boilerplate, Auth Hub & User Profile | FEATURE | FE | MISSING | P0 | Dev FE | T08, T10 |
| **T57** | Buyer Marketplace & Product Search / Details Portal | FEATURE | FE | MISSING | P1 | Dev FE | T16, T18, T44 |
| **T58** | Cart, Multi-shop Checkout & Payment Flow | FEATURE | FE | MISSING | P0 | Dev FE | T19, T23, T30, T31 |
| **T59** | Seller Center Portal (Shop, Products, Orders & Shipping) | FEATURE | FE | MISSING | P1 | Dev FE | T11, T13, T16, T35 |
| **T60** | Livestream Commerce Portal (Agora RTC + SignalR Realtime) | FEATURE | FE | MISSING | P2 | Dev FE | T41, T42 |
| **T61** | Returns, Admin Operations & Executive Dashboard | FEATURE | FE | MISSING | P1 | Dev FE | T14, T37, T39, T51 |

---

# PART 4 — DETAILED TASK SPECIFICATION (EXTENDED TECHNICAL DETAILS)

---

## 🟡 EPIC 01 — AUTH, IDENTITY & RBAC

### T08 — [BE/API/INFRA] Complete Auth Engine & RBAC Pipeline (Gộp gọn toàn bộ luồng Auth)
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev A
- **SRS Requirement**: `FR-AUTH-001` đến `FR-AUTH-011`, `NFR-SEC-001`, `NFR-SEC-002`, `NFR-SEC-004`, `7.1 REST API`
- **Detailed Technical Implementation**:
  1. **User Registration & OTP Verification**:
     - `RegisterUserCommand`: Kiểm tra email/phone chưa tồn tại trong DB (nếu trùng trả về `409 Conflict`). Hash password bằng BCrypt (work factor 12). Lưu User mới với `account_status = Unverified`. Sinh OTP 6 chữ số ngẫu nhiên, hash SHA-256 lưu `UserOtps` với `expires_at = NOW() + 5 phút`. Gửi email OTP.
     - `VerifyOtpCommand`: Tìm `UserOtps` khớp `user_id` và OTP hash. Kiểm tra `expires_at` và `used_at IS NULL`. Nếu đúng → cập nhật `User.account_status = Active`, đánh dấu `used_at`, gán `Role` mặc định `Buyer` trong `UserRoles`.
     - `ResendOtpCommand`: Kiểm tra Redis key `otp:rate:{email}`. Nếu đã gửi quá 3 lần/15 phút → trả về `429 Too Many Requests`. Ngược lại sinh OTP mới và gửi lại.
  2. **Login & Token Management (JWT + Refresh Token Rotation + Reuse Detection)**:
     - `LoginCommand`: Kiểm tra email và BCrypt password. Kiểm tra `account_status == Active`. Nếu sai password 5 lần liên tiếp (dùng Redis counter `login:failed:{email}`) → tạm khóa tài khoản với backoff TTL (5 phút → 15 phút → 1 giờ → 24 giờ). Khi đúng → reset counter, sinh cặp Token:
       - **Access Token**: JWT Bearer HS256, payload chứa `userId`, `roles[]`, `shopId?`, `jti` (UUID unique), TTL 15 phút.
       - **Refresh Token**: Chuỗi ngẫu nhiên 64 bytes cryptographically secure. Hash SHA-256 lưu vào `RefreshTokens` kèm `device_info` và `ip_address`, TTL 30 ngày.
     - `RefreshTokenCommand`: Nhận `refreshToken`. Hash SHA-256 và tra cứu trong DB.
       - **Token Reuse Detection**: Nếu `RefreshTokens.revoked_at != NULL` (token đã bị thu hồi trước đó nhưng bị gửi lại) → Hệ thống cảnh báo tấn công chiếm đoạt token: Đánh dấu thu hồi **toàn bộ RefreshTokens** thuộc `user_id` đó và trả về `401 Unauthorized`.
       - Nếu token hợp lệ: Đánh dấu `revoked_at = NOW()`, sinh Refresh Token mới và Access Token mới (Token Rotation).
  3. **Password Self-Service & Logout**:
     - `ForgotPasswordCommand`: Gửi OTP reset password 6 chữ số qua email.
     - `ResetPasswordCommand`: Kiểm tra OTP, hash password mới bằng BCrypt và cập nhật DB.
     - `ChangePasswordCommand`: Xác thực password cũ, đổi password mới và đánh dấu `revoked_at` cho toàn bộ Refresh Tokens của user.
     - `LogoutCommand`: Nhận `refreshToken`, đánh dấu `revoked_at`. Đưa `jti` của Access Token hiện tại vào Redis Blacklist với TTL bằng thời gian sống còn lại của Access Token (`Blacklist:{jti}`).
  4. **API Controllers & Security Middlewares**:
     - `AuthController`: Implement 10 REST endpoints dưới route `/api/v1/auth/*`; `POST /auth/login/google`: **hoãn, chưa làm**.
     - `JwtRoleContextMiddleware`: Trích xuất JWT từ Authorization Header. Đọc `jti` và check key `Blacklist:{jti}` trong Redis. Nếu tồn tại → ngắt pipeline trả về `401 Unauthorized`. Bind `CurrentUser` context.
     - `AuthorizationBehavior`: Pipeline Behavior của MediatR đọc attribute `[IRequirePermission]` hoặc `[AuthorizeRole]`, kiểm tra role của user context trước khi execute handler.
- **Dependencies**: T03, T09
- **API Contracts**: `RegisterRequest`, `VerifyOtpRequest`, `ResendOtpRequest`, `LoginRequest`, `RefreshTokenRequest`, `ForgotPasswordRequest`, `ResetPasswordRequest`, `ChangePasswordRequest`, `LogoutRequest`, `AuthResponse`, `UserInfoResponse` (`NovaLive.Contracts.V1.Auth`)
- **Acceptance Criteria** (`AC-001`):
  - Đăng ký email trùng trả về HTTP 409. Đăng nhập sai 5 lần bị khóa tài khoản theo backoff.
  - Sử dụng lại Refresh Token đã revoked lập tức ngắt toàn bộ phiên đăng nhập của User trên mọi thiết bị.
  - Token bị logout không thể gọi bất kỳ API protected nào.
- **Estimated Size**: XL

---

### T09 — [DB] Seed System Roles, Resources & Default Permissions
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev A
- **SRS Requirement**: `FR-AUTH-001`, `FR-SHOP-005`
- **Detailed Technical Implementation**:
  - Viết logic trong `SystemDataSeeder : IDataSeeder` chạy khi khởi động ứng dụng (được gọi từ `MigrationService`).
  - Nạp 3 Roles cố định vào bảng `Roles`: `Admin` (ID: System), `Seller` (ID: System), `Buyer` (ID: System) với `is_system = TRUE`.
  - Nạp danh sách `Resources` (Auth, Users, Shops, Products, Inventory, Orders, Discounts, FlashSales, Livestreams, Reviews, System).
  - Nạp `Permissions` chuẩn với định dạng `code = {resource}:{action}` (ví dụ: `products:create`, `orders:approve`, `shops:ban`).
  - Gán danh sách `RolePermissions` tương ứng cho từng Role theo đúng bảng Matrix Phân Quyền trong `ecommerce_business_logic.md`.
- **Dependencies**: T03
- **Acceptance Criteria** (`AC-001`): DB khởi tạo có đủ 3 System Roles và toàn bộ mã Permission chuẩn, không bị duplicate khi chạy seeder nhiều lần (Idempotent seeding).
- **Estimated Size**: S

---

## 🟡 EPIC 02 — USER PROFILE & SHOP MANAGEMENT

### T10 — [BE/API] User Profile & Address Book Management
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev B
- **SRS Requirement**: `FR-USER-001`, `FR-USER-002`, `FR-USER-003`, `FR-USER-004`
- **Detailed Technical Implementation**:
  - `GetUserProfileQuery` & `UpdateUserProfileCommand`: Xem và cập nhật `full_name`, `phone`, `birthday`, `gender`, `avatar_url` trong `Users`.
  - `GetUserAddressesQuery`: Lấy danh sách sổ địa chỉ nhận hàng từ `UserAddresses` xếp theo `is_default DESC, created_at DESC`.
  - `CreateUserAddressCommand`: Thêm địa chỉ mới (`recipient_name`, `phone`, `province_id`, `province_name`, `district_id`, `district_name`, `ward_code`, `ward_name`, `detail_address`). Nếu `is_default = TRUE` hoặc đây là địa chỉ đầu tiên → tự động bỏ `is_default` của các địa chỉ cũ.
  - `UpdateUserAddressCommand` & `SetDefaultAddressCommand`: Cập nhật địa chỉ và đặt làm mặc định.
  - `DeleteUserAddressCommand`: Xóa địa chỉ. **Bảo vệ nghiệp vụ**: Nếu địa chỉ cần xóa đang có `is_default = TRUE` và user vẫn còn các địa chỉ khác trong sổ địa chỉ → Ném `BusinessRuleException("Không thể xóa địa chỉ mặc định khi còn địa chỉ khác trong sổ địa chỉ")`.
  - Endpoint `UsersController`: `GET /users/me`, `PUT /users/me`, `GET /users/me/addresses`, `POST /users/me/addresses`, `PUT /users/me/addresses/{id}`, `DELETE /users/me/addresses/{id}`, `PUT /users/me/addresses/{id}/default`.
- **Dependencies**: T08
- **API Contracts**: `UpdateProfileRequest`, `CreateAddressRequest`, `UpdateAddressRequest`, `UserProfileResponse`, `AddressResponse` (`NovaLive.Contracts.V1.Users`)
- **Estimated Size**: S

---

### T11 — [BE/API] Shop Registration, KYC Upload & Warehouse Management
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev B
- **SRS Requirement**: `FR-SHOP-001`, `FR-SHOP-002`, `FR-SHOP-003`, `FR-SHOP-008`, `FR-SHOP-009`
- **Detailed Technical Implementation**:
  - `RegisterShopCommand`: Kiểm tra User hiện tại có `account_status == Active`. **Check ràng buộc 1-1**: Tra cứu `Shops WHERE owner_id = userId`. Nếu đã có shop → Trả về lỗi `400 Bad Request` ("Mỗi tài khoản chỉ được sở hữu tối đa 1 gian hàng"). Nộp thông tin tạo `Shops` với `status = Pending`, tạo `ShopVerifications` chứa URL ảnh KYC (`id_card_front`, `id_card_back`, `business_license`, `bank_account`, `bank_name`).
  - `UpdateShopInfoCommand`: Seller cập nhật logo, banner, mô tả shop, số điện thoại liên hệ.
  - `AddShopAddressCommand` & `GetShopAddressesQuery`: Quản lý kho lấy hàng (`ShopAddresses`). Cho phép đánh dấu `is_primary = TRUE` (kho lấy hàng mặc định) và `is_return = TRUE` (kho nhận hàng hoàn).
  - `FollowShopCommand` & `UnfollowShopCommand`: Ghi nhận/xóa bản ghi trong `ShopFollowers` (UNIQUE `shop_id, user_id`).
  - `ShopsController`: Endpoints `POST /shops/register`, `GET /shops/me`, `PUT /shops/me`, `GET/POST /shops/me/addresses`, `GET /shops/{shopId}`, `POST/DELETE /shops/{shopId}/follow`.
- **Dependencies**: T08, T12
- **API Contracts**: `RegisterShopRequest`, `UpdateShopRequest`, `ShopAddressRequest`, `ShopResponse`, `ShopDetailResponse` (`NovaLive.Contracts.V1.Shops`)
- **Acceptance Criteria** (`AC-002`): User đã tạo shop không thể gửi thêm yêu cầu đăng ký shop thứ 2. Thông tin KYC lưu trữ đúng URL từ MinIO.
- **Estimated Size**: M

---

### T12 — [INFRA] MinIO File Storage Adapter & Multi-bucket Upload
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev C
- **SRS Requirement**: `2.4 Phụ thuộc ngoài`, `NFR-SEC-008`
- **Detailed Technical Implementation**:
  - Implement class `MinioFileStorageService : IFileStorageService` sử dụng MinIO C# SDK kết nối tới MinIO S3 API Container (`http://minio:9000`).
  - Tự động kiểm tra và khởi tạo các S3 Buckets công khai & riêng tư khi khởi chạy: `products`, `kyc-docs`, `reviews`, `livestreams`.
  - Method `UploadAsync(Stream fileStream, string fileName, string contentType, string bucketName)`: Sinh UUID ngẫu nhiên gắn vào tên file tránh ghi đè, kiểm tra extension hợp lệ (`.jpg`, `.jpeg`, `.png`, `.webp`, `.mp4`), kiểm tra dung lượng stream (ảnh <= 10MB, video <= 100MB). Trả về URL CDN đầy đủ `https://cdn.novalive.vn/{bucket}/{filename}`.
  - Endpoint `POST /api/v1/upload/media` hỗ trợ `multipart/form-data` nhận file từ client.
- **Dependencies**: T02
- **Acceptance Criteria**: Upload file thành công trả về HTTP 200 kèm CDN URL. File sai định dạng hoặc vượt quá 100MB trả về HTTP 400 Bad Request.
- **Estimated Size**: M

---

### T13 — [BE/API] Shop Wallet, Financial Ledger & Payout Request
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev B
- **SRS Requirement**: `FR-WALLET-001`, `FR-WALLET-002`, `FR-WALLET-003`, `FR-WALLET-004`
- **Detailed Technical Implementation**:
  - `GetShopWalletQuery`: Trả về `ShopWallets` của shop seller (`balance`, `locked_balance`).
  - `GetWalletTransactionsQuery`: Truy vấn bảng sổ cái `ShopWalletTransactions` có phân trang, lọc theo loại giao dịch (`type`) và khoảng thời gian (`from`, `to`).
  - `CreatePayoutRequestCommand`:
    1. Kiểm tra số tiền rút `amount`: Phải `>= 50.000 VNĐ` và `<= ShopWallets.balance`.
    2. Thực thi trong 1 DB Transaction nguyên tử: Trừ `ShopWallets.balance -= amount`, tăng `ShopWallets.locked_balance += amount`.
    3. Tạo bản ghi `SellerPayouts` ở trạng thái `Pending` lưu thông tin số tài khoản, tên ngân hàng thụ hưởng (lấy mặc định từ `ShopVerifications` đã KYC).
    4. Ghi sổ cái `ShopWalletTransactions` loại `PayoutLock` với `amount = -amount`, ghi nhận `balance_before` và `balance_after`.
  - `WalletController`: Endpoints `GET /seller/wallet`, `GET /seller/wallet/transactions`, `POST /seller/wallet/payout`.
- **Dependencies**: T11
- **API Contracts**: `PayoutRequest`, `ShopWalletResponse`, `WalletTransactionResponse`, `PayoutResponse` (`NovaLive.Contracts.V1.Shops`)
- **Acceptance Criteria** (`AC-010`): Không thể rút số tiền lớn hơn `balance`. `balance` và `locked_balance` biến động chính xác trong cùng một transaction.
- **Estimated Size**: M

---

### T14 — [BE/API] Admin Shop Onboarding, Ban Policy & Payout Approval
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev B
- **SRS Requirement**: `FR-SHOP-004`, `FR-SHOP-005`, `FR-SHOP-006`, `FR-SHOP-007`, `FR-WALLET-005`, `FR-ADMIN-001`, `FR-ADMIN-002`, `FR-ADMIN-006`
- **Detailed Technical Implementation**:
  - `ApproveShopCommand` (Admin): Chuyển `Shops.status = Active`, `ShopVerifications.status = Approved`. Gán `Role` `Seller` cho owner. Khởi tạo `ShopWallets` ban đầu với `balance = 0, locked_balance = 0`.
  - `BanShopCommand` (Admin): Chuyển `Shops.status = Banned` (hoặc `Suspended`). Tự động cập nhật toàn bộ `Spus.status = Inactive` của shop để ẩn sản phẩm khỏi tìm kiếm công khai, ngăn chặn tạo đơn mới.
  - `ApprovePayoutCommand` (Admin): Nhập `transfer_ref` ngân hàng. Thực thi transaction: Trừ `ShopWallets.locked_balance -= amount`, chuyển `SellerPayouts.status = Completed`, ghi sổ cái `ShopWalletTransactions` loại `PayoutWithdrawal`.
  - `RejectPayoutCommand` (Admin): Nếu từ chối lệnh rút tiền → Hoàn trả `locked_balance -= amount`, `balance += amount`, chuyển `SellerPayouts.status = Failed`, ghi sổ cái loại `PayoutFailedUnlock`.
  - `AdminShopsController`: Endpoints `GET /admin/shops`, `PUT /admin/shops/{id}/approve`, `PUT /admin/shops/{id}/ban`, `GET /admin/payouts`, `PUT /admin/payouts/{id}/approve`, `PUT /admin/payouts/{id}/reject`.
- **Dependencies**: T08, T11
- **API Contracts**: `ApproveShopRequest`, `BanShopRequest`, `ApprovePayoutRequest` (`NovaLive.Contracts.V1.Shops`)
- **Estimated Size**: M

---

## 🟡 EPIC 03 — PRODUCT CATALOG & INVENTORY

### T15 — [BE/API] Multi-level Category Tree Management
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev B
- **SRS Requirement**: `FR-CAT-001`, `FR-ADMIN-003`
- **Detailed Technical Implementation**:
  - `GetCategoriesTreeQuery`: Đọc toàn bộ bảng `Categories` sắp xếp theo `display_order ASC`. Build cấu trúc cây JSON n-cấp (Parent-Child hierarchy) trong bộ nhớ, tính toán số lượng sản phẩm `ProductCount` cho từng danh mục.
  - `CreateCategoryCommand` & `UpdateCategoryCommand` (Admin): Tạo/sửa tên danh mục, slug, icon_url, parent_id, display_order. Kiểm tra không được chọn `parent_id` chính là danh mục hiện tại (chống vòng lặp vô tận).
  - `DeleteCategoryCommand` (Admin): Xóa danh mục. **Ràng buộc**: Kiểm tra nếu danh mục đang chứa `Spus` hoặc đang có `Categories` con → Báo lỗi `400 Bad Request` ("Không thể xóa danh mục đang có sản phẩm hoặc danh mục con").
  - `CategoriesController`: Public `GET /categories`, `GET /categories/{id}`; Admin `POST /categories`, `PUT /categories/{id}`, `DELETE /categories/{id}`.
- **Dependencies**: T08
- **API Contracts**: `CreateCategoryRequest`, `UpdateCategoryRequest`, `CategoryResponse` (`NovaLive.Contracts.V1.Categories`)
- **Estimated Size**: S

---

### T16 — [BE/API] Product SPU & SKU Multi-variant Management
- **Status**: COMPLETED | **Priority**: P1 | **Assign**: Dev B
- **SRS Requirement**: `FR-CAT-002`, `FR-CAT-003`, `FR-CAT-004`, `FR-CAT-005`
- **Detailed Technical Implementation**:
  - `CreateSpuCommand` (Seller):
    1. Nhận tên SPU, mô tả, category_id, brand, thumbnail_url, attributesConfig (trục biến thể: Màu, Size), danh sách SKUs biến thể.
    2. Kiểm tra `sku_code` duy nhất trong cùng `shop_id`. Kiểm tra `original_price` và `sell_price >= 0`, `weight_gram > 0`.
    3. Tạo bản ghi `Spus` với `status = Active`.
    4. Tạo danh sách `Skus` gắn với `spu_id` kèm `attributes_json` (máy trận Descartes tổ hợp thuộc tính).
    5. **Tự động tạo Tồn kho ban đầu**: Với mỗi SKU mới tạo, chèn 1 bản ghi vào bảng `Inventories` với `qty_on_hand = initialStock`, `reserved_qty = 0`, `min_stock = 5`. Ghi log `InventoryHistories` loại `Import`.
    6. Ghi `OutboxMessage(ProductUpdatedEvent)` để worker cập nhật `search_vector` PostgreSQL.
  - `UpdateSpuCommand` & `UpdateSkuCommand`: Cập nhật thông tin SPU, đổi giá bán `sell_price`, giá gốc `original_price`, trọng lượng `weight_gram`, ẩn/hiện SKU (`is_active`).
  - `DeleteSpuCommand`: Soft-delete SPU (`deleted_at = NOW()`) và toàn bộ SKUs liên quan.
  - `ProductsController` (Public) & `SellerProductsController` (`GET/POST/PUT/DELETE /seller/products`, `PUT /seller/skus/{skuId}`).
- **Dependencies**: T11, T15
- **API Contracts**: `CreateProductRequest`, `UpdateProductRequest`, `UpdateSkuRequest`, `SpuDetailResponse`, `SkuDetailResponse` (`NovaLive.Contracts.V1.Products`)
- **Acceptance Criteria** (`AC-002`): SPU tạo thành công tự động có bản ghi tồn kho tương ứng cho từng SKU. `sku_code` bị trùng trong cùng shop sẽ báo lỗi.
- **Estimated Size**: L

---

### T17 — [BE/API] Inventory 2-State Management & Ledger Logging
- **Status**: COMPLETED | **Priority**: P1 | **Assign**: Dev B
- **SRS Requirement**: `FR-CAT-006`, `FR-CAT-007`, `FR-CAT-008`, `NFR-REL-006`
- **Detailed Technical Implementation**:
  - `GetSellerInventoryQuery`: Lấy danh sách tồn kho các SKU của shop. Tính toán động: `AvailableStock = Inventories.qty_on_hand - Inventories.reserved_qty`. Hỗ trợ filter `lowStock = TRUE` (khi `AvailableStock <= min_stock`).
  - `AdjustInventoryCommand` (Seller điều chỉnh tồn kho thủ công):
    1. Nhận `skuId`, `qtyChange` (số nguyên âm hoặc dương), `changeType` (`Import`, `ManualAdjust`), `note`.
    2. Cập nhật `Inventories.qty_on_hand += qtyChange`.
    3. **Ràng buộc cốt lõi**: Kiểm tra `qty_on_hand >= reserved_qty`. Nếu điều chỉnh làm `qty_on_hand < reserved_qty` → Ném ngoại lệ `BusinessRuleException("Số lượng tồn kho vật lý không thể nhỏ hơn số lượng đang giữ chỗ đặt hàng")`.
    4. Ghi bản ghi sổ cái bất biến vào `InventoryHistories`: Lưu `qty_before`, `qty_change`, `reserved_before`, `reserved_change = 0`, `qty_after`, `created_by = userId`.
  - `GetInventoryHistoriesQuery`: Xem lịch sử biến động kho phân trang của từng SKU.
  - Endpoints: `GET /seller/inventory`, `POST /seller/inventory/adjust`, `GET /seller/inventory/histories`.
- **Dependencies**: T16
- **API Contracts**: `AdjustInventoryRequest`, `InventoryResponse`, `InventoryHistoryResponse` (`NovaLive.Contracts.V1.Products`)
- **Estimated Size**: M

---

### T18 — [BE/API] Public Product Search Engine (Full-text + Trigram)
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev B
- **SRS Requirement**: `FR-SEARCH-001`, `FR-SEARCH-002`, `FR-SEARCH-003`, `FR-SEARCH-004`, `FR-SEARCH-005`, `NFR-PERF-001`, `NFR-PERF-004`
- **Detailed Technical Implementation**:
  - Thực thi trong `PostgresProductQueryService : IProductQueryService`:
  - Query sử dụng kết hợp PostgreSQL Full-Text Search và Trigram Similarity:
    ```sql
    SELECT s.*, ts_rank(s.search_vector, websearch_to_tsquery('vietnamese', @keyword)) AS rank
    FROM Spus s
    JOIN Shops sh ON s.shop_id = sh.id
    WHERE s.status = 'Active' AND s.deleted_at IS NULL AND sh.status = 'Active'
      AND (
        s.search_vector @@ websearch_to_tsquery('vietnamese', @keyword)
        OR s.name % @keyword
      )
    ORDER BY rank DESC, s.created_at DESC;
    ```
  - Hỗ trợ lọc theo `categoryId`, `shopId`, khoảng giá `minPrice` - `maxPrice`, xếp hạng theo bán chạy, giá tăng/giảm.
  - Endpoint `ProductsController`: `GET /products`, `GET /products/{spuId}`, `GET /products/search`. API hỗ trợ cache Redis 60 giây cho các truy vấn phổ biến.
- **Dependencies**: T16
- **API Contracts**: `ProductSearchRequest`, `ProductSummaryResponse`, `SpuDetailResponse` (`NovaLive.Contracts.V1.Products`)
- **Acceptance Criteria** (`AC-003`): Tìm kiếm sản phẩm trả về nhanh (P95 <= 500ms), tự động gợi ý từ khóa gần đúng nhờ trigram index.
- **Estimated Size**: M

---

## 🟡 EPIC 04 — CART & CHECKOUT ENGINE

### T19 — [BE/API] Cart Multi-shop Management Use Cases
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev A
- **SRS Requirement**: `FR-CART-001`, `FR-CART-002`, `FR-CART-003`, `FR-CART-004`
- **Detailed Technical Implementation**:
  - `GetCartQuery`: Tra cứu `Carts` của Buyer. Lấy toàn bộ `CartItems`, join `Skus`, `Spus`, `Shops`. Gom nhóm danh sách các món theo từng `Shop` (`CartGroupedByShopResponse`).
  - `AddToCartCommand`: Nhận `skuId`, `quantity`. Tra cứu SKU (kiểm tra `is_active == TRUE`, tồn kho khả dụng `available > 0`). Nếu SKU đã có trong giỏ → Cộng dồn `CartItems.quantity += quantity`. Nếu chưa có → Thêm dòng mới kèm `unit_price` snapshot.
  - `UpdateCartItemCommand`: Nhận `cartItemId`, `quantity`. Nếu `quantity == 0` → Tự động xóa dòng `CartItems`. Nếu `quantity > 0` → Cập nhật số lượng.
  - `RemoveCartItemCommand` & `RemoveSelectedCartItemsCommand`: Xóa 1 hoặc danh sách nhiều `cartItemIds` sau khi checkout.
  - Endpoint `CartController`: `GET /cart`, `POST /cart/items`, `PUT /cart/items/{id}`, `DELETE /cart/items/{id}`, `DELETE /cart/items`.
- **Dependencies**: T16, T08
- **API Contracts**: `AddToCartRequest`, `UpdateCartItemRequest`, `CartResponse`, `CartGroupedByShopResponse` (`NovaLive.Contracts.V1.Carts`)
- **Estimated Size**: M

---

### T20 — [DOMAIN] PriceCalculator Domain Service (3-tier Proration Engine)
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev A
- **SRS Requirement**: `FR-CHECKOUT-003`, `FR-DISCOUNT-005`, `NFR-MAINT-005`
- **Detailed Technical Implementation**:
  - Tạo Domain Service `PriceCalculator` thuần túy trong `NovaLive.Domain`:
  - Nhận vào danh sách các Shop Items, Phí ship từng shop, Shop Vouchers, Platform Product Voucher, Platform FreeShip Voucher.
  - **Thuật toán tính toán 5 bước**:
    1. **Shop Subtotal**: Với mỗi Shop $S_i$, tính $\text{Subtotal}_{S_i} = \sum (\text{unit\_price} \times \text{quantity})$.
    2. **Tầng 1 (Shop Voucher)**: Trừ trực tiếp giá trị voucher shop vào $\text{Subtotal}_{S_i}$.
    3. **Tầng 2 (Platform Product Voucher Proration)**: Nếu áp voucher sàn giảm $V$ VNĐ trên tổng đơn:
       $$\text{PlatformDiscount}_{S_i} = \text{ROUND}\left(V \times \frac{\text{Subtotal}_{S_i}}{\sum \text{Subtotal}}\right)$$
       Đảm bảo tổng phần tiền giảm phân bổ $\sum \text{PlatformDiscount}_{S_i} == V$ (xử lý sai số làm tròn 1 đồng vào shop có subtotal lớn nhất).
    4. **Tầng 3 (Platform FreeShip Voucher)**: Giảm trực tiếp vào cước vận chuyển $\text{ShippingFee}_{S_i}$.
    5. **Grand Total**: Tính $\text{SubTotalNet}_{S_i} = \text{Subtotal}_{S_i} + \text{ShippingFee}_{S_i} - \text{ShopDiscount}_{S_i} - \text{PlatformDiscount}_{S_i} - \text{FreeShipDiscount}_{S_i}$. Tổng tiền thanh toán `grand_total` = $\sum \text{SubTotalNet}_{S_i}$.
- **Dependencies**: T07
- **Acceptance Criteria**: Unit Test đạt 100% các case proration phân bổ chính xác từng đồng, không bị lệch tiền tổng.
- **Estimated Size**: L

---

### T21 — [BE] Calculate Checkout Draft Query (Realtime Preview)
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev A
- **SRS Requirement**: `FR-CHECKOUT-001`, `FR-CHECKOUT-002`, `FR-CHECKOUT-003`
- **Detailed Technical Implementation**:
  - `CalculateCheckoutDraftQuery`: Nhận `cartItemIds[]`, `shippingAddressId`, mã vouchers.
  - Lấy thông tin chi tiết địa chỉ nhận từ `UserAddresses`.
  - Gom nhóm các `cartItemIds` theo `shop_id`.
  - Với mỗi shop: Gọi `IShippingProvider.CalculateFeeAsync` tính cước phí ship thời gian thực dựa trên địa chỉ kho shop (`ShopAddresses.is_primary`) và địa chỉ nhận của Buyer.
  - Gọi Domain Service `PriceCalculator` thực thi tính toán Voucher 3 tầng và Proration phân bổ tiền giảm.
  - Trả về `CheckoutPreviewResponse`: Chi tiết phân rã số tiền hàng, phí ship, voucher cho từng Sub-order và tổng tiền `grand_total` để giao diện hiển thị trước khi bấm Đặt hàng.
- **Dependencies**: T19, T20
- **API Contracts**: `CalculateCheckoutRequest`, `CheckoutPreviewResponse` (`NovaLive.Contracts.V1.Orders`)
- **Estimated Size**: M

---

### T22 — [BE] Checkout Command Handler (Atomic UoW 7-Step Transaction)
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev A
- **SRS Requirement**: `FR-CAT-009`, `FR-CHECKOUT-004`, `FR-CHECKOUT-005`, `FR-CHECKOUT-006`, `FR-CHECKOUT-007`, `FR-CHECKOUT-008`, `NFR-REL-001`, `NFR-PERF-002`
- **Detailed Technical Implementation**:
  - `CheckoutCommand` thực thi trong một Database Transaction duy nhất (`IUnitOfWork`):
    1. **Khóa & Kiểm tra tồn kho**: Tra cứu `Inventories WHERE sku_id IN (...) FOR UPDATE`. Kiểm tra lượng khả dụng (`qty_on_hand - reserved_qty >= quantity`). Nếu không đủ → Rollback và báo lỗi hết hàng.
    2. **Inventory Reservation**: Cập nhật `Inventories.reserved_qty += quantity` cho tất cả các SKU đặt mua. Ghi log `InventoryHistories` loại `ReserveAdd`.
    3. **Tạo ParentOrder**: Sinh `order_code = "NOVA-{YYYYMMDD}-{RANDOM4}"`, snapshot thông tin địa chỉ nhận vào `shipping_address_json`, lưu `grand_total`, `payment_status = Pending`.
    4. **Tạo SubOrders & OrderItems**: Với N shop có sản phẩm được chọn, sinh N bản ghi `SubOrders` (`sub_order_code = "{order_code}-S{i}"`). Tạo `OrderItems` cho từng sản phẩm kèm snapshot DTO bất biến `sku_snapshot_json` ({skuCode, spuName, attributes, thumbnailUrl}) và phân bổ tiền giảm `discount_amount`.
    5. **Khởi tạo Payment**: Tạo bản ghi `Payments` (trỏ tới `parent_order_id`, method = MoMo/VietQR/COD, status = Pending).
    6. **Dọn giỏ hàng**: Xóa đúng các `cartItemIds` đã checkout khỏi `CartItems` (giữ lại các món khác chưa được tick chọn).
    7. **Outbox Event**: Ghi `OutboxMessage(OrderPlacedEvent)` trong cùng Transaction.
  - **Xử lý đặc thù đơn COD**: Nếu `paymentMethod == COD` → `ParentOrder.payment_status = Pending`, nhưng các `SubOrders.status` lập tức chuyển thành `Confirmed` để Seller có thể đóng gói hàng ngay lập tức.
- **Dependencies**: T21, T32
- **API Contracts**: `CheckoutRequest`, `CheckoutResponse` (`NovaLive.Contracts.V1.Orders`)
- **Acceptance Criteria** (`AC-004`, `AC-005`): Thất bại ở bất kỳ bước nào từ 1-8 sẽ rollback toàn bộ. Checkout hoàn thành P95 <= 1.5s.
- **Estimated Size**: XL

---

### T23 — [API] Order Controller Endpoints & Seller Order Operations
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev A
- **SRS Requirement**: `7.1 REST API`
- **Detailed Technical Implementation**:
  - `OrdersController` (Buyer):
    - `POST /orders/calculate-checkout`: Gọi `CalculateCheckoutDraftQuery`.
    - `POST /orders/checkout`: Gọi `CheckoutCommand`.
    - `GET /orders`: Lấy lịch sử mua hàng phân trang.
    - `GET /orders/{id}`: Chi tiết ParentOrder kèm danh sách SubOrders và Timeline trạng thái.
    - `POST /orders/{id}/cancel`: Hủy đơn (chỉ được hủy khi SubOrders chưa chuyển `Confirmed`).
  - `SellerOrdersController` (Seller):
    - `GET /seller/orders`: Danh sách SubOrders cần xử lý của Shop.
    - `GET /seller/orders/{subOrderId}`: Chi tiết SubOrder.
    - `PUT /seller/orders/{subOrderId}/confirm`: Xác nhận đơn & chọn địa chỉ kho lấy hàng (`warehouse_address_id`).
    - `POST /seller/orders/{subOrderId}/cancel`: Hủy đơn kèm lý do hết hàng/sự cố.
- **Dependencies**: T22
- **API Contracts**: `CancelOrderRequest`, `ConfirmSubOrderRequest`, `ParentOrderDetailResponse`, `SubOrderDetailResponse` (`NovaLive.Contracts.V1.Orders`)
- **Estimated Size**: M

---

## 🟡 EPIC 05 — DISCOUNTS & VOUCHERS

### T24 — [BE/API] Discount & Voucher Lifecycle Management
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev B
- **SRS Requirement**: `FR-DISCOUNT-001`, `FR-DISCOUNT-002`, `FR-DISCOUNT-003`, `FR-DISCOUNT-007`, `FR-DISCOUNT-008`
- **Detailed Technical Implementation**:
  - `CreateDiscountCommand`:
    - **Seller**: Tạo Voucher Shop (`shop_id = currentShopId`). Loại discount: `PercentCart`, `FixedCart`.
    - **Admin**: Tạo Voucher Toàn Sàn (`shop_id = NULL`). Loại discount: `PercentCart`, `FixedCart`, `PercentShip`, `FreeShip`.
    - Kiểm tra `code` duy nhất toàn sàn. Kiểm tra `valid_to > valid_from`, `min_order_amount >= 0`, `max_discount_amount > 0` (nếu là % discount). Cấu hình `applies_to` (`AllProducts`, `SpecificSpus`).
  - `UpdateDiscountCommand`: Cho phép cập nhật `name`, tăng `maxUses`, gia hạn `validTo`, hoặc kích hoạt/tắt (`isActive`). Khóa không cho sửa `code`, `discountValue` khi đã có người dùng.
  - Endpoints `DiscountsController`: Public `GET /discounts/shop/{shopId}`, `GET /discounts/platform`; Seller `GET/POST/PUT /seller/discounts`; Admin `POST /admin/discounts`.
- **Dependencies**: T08, T11
- **API Contracts**: `CreateDiscountRequest`, `UpdateDiscountRequest`, `DiscountResponse` (`NovaLive.Contracts.V1.Discounts`)
- **Estimated Size**: M

---

### T25 — [BE/API] Voucher Validation & Proration Calculation Query
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev B
- **SRS Requirement**: `FR-DISCOUNT-004`, `FR-DISCOUNT-006`
- **Detailed Technical Implementation**:
  - `ValidateVoucherQuery`:
    1. Tra cứu voucher theo `code`. Kiểm tra `is_active == TRUE` và `NOW()` nằm trong khoảng `[valid_from, valid_to]`.
    2. Kiểm tra `used_count < max_uses` (nếu có giới hạn tổng lượt dùng).
    3. Kiểm tra số lượt user đã dùng trong `DiscountUsages`: Count `< per_user_limit`.
    4. Kiểm tra tổng tiền đơn hàng `cartAmount >= min_order_amount`.
    5. Kiểm tra danh sách sản phẩm trong giỏ hàng có chứa SKU thuộc `target_ids` (nếu `applies_to == SpecificSpus`).
    6. Tính toán số tiền giảm giá chính xác và trả về `VoucherValidationResponse(isValid, discountAmount, reasonIfInvalid)`.
- **Dependencies**: T24
- **API Contracts**: `ValidateVoucherRequest`, `VoucherValidationResponse` (`NovaLive.Contracts.V1.Discounts`)
- **Estimated Size**: S

---

## 🟡 EPIC 06 — FLASH SALE

### T26 — [BE/API] Flash Sale Campaign & Registration Management
- **Status**: MISSING | **Priority**: P2 | **Assign**: Dev B
- **SRS Requirement**: `FR-FS-001`, `FR-FS-002`, `FR-FS-003`, `FR-FS-004`, `FR-ADMIN-004`
- **Detailed Technical Implementation**:
  - `CreateFlashSaleCampaignCommand` (Admin): Tạo chiến dịch Flash Sale với khung giờ `start_at` -> `end_at` (`status = Scheduled`).
  - `RegisterSkuForFlashSaleCommand` (Seller đăng ký SKU): Nhập `skuId`, `flashPrice`, `quantity`, `perUserLimit`. **Kiểm tra nghiệp vụ**: Giá `flashPrice` phải thấp hơn giá bán thường `sell_price` tối thiểu **10%**. Lưu vào `FlashSaleItems` ở trạng thái `Pending`.
  - `ApproveFlashSaleItemCommand` / `RejectFlashSaleItemCommand` (Admin): Duyệt/từ chối SKU tham gia. Khi duyệt → Chuyển `status = Approved`.
  - `FlashSaleController`: Endpoints `GET /flash-sales/active`, `GET /flash-sales/{campaignId}/products`, `POST /seller/flash-sales/{campaignId}/register`, `PUT /admin/flash-sales/items/{itemId}/approve`. Cache danh sách Flash Sale active lên Redis.
- **Dependencies**: T16, T08
- **API Contracts**: `CreateFlashSaleCampaignRequest`, `RegisterFlashSaleSkuRequest`, `FlashSaleCampaignResponse`, `FlashSaleItemResponse` (`NovaLive.Contracts.V1.FlashSales`)
- **Estimated Size**: M

---

### T27 — [BE] Atomic Flash Sale Reserve (PG Condition UPDATE)
- **Status**: MISSING | **Priority**: P2 | **Assign**: Dev B
- **SRS Requirement**: `FR-FS-005`, `FR-FS-006`, `FR-FS-007`, `FR-FS-008`, `FR-FS-009`, `NFR-REL-002`
- **Detailed Technical Implementation**:
  - Trong luồng `CheckoutCommand` khi mua SKU thuộc Flash Sale đang Active:
  - Kiểm tra số lượng User đã mua SKU này trong chiến dịch (DB check `< per_user_limit`).
  - Thực thi câu lệnh PostgreSQL Atomic UPDATE trực tiếp không cần Lock bảng:
    ```sql
    UPDATE FlashSaleItems
    SET reserved_qty = reserved_qty + @request_qty
    WHERE id = @flashSaleItemId
      AND status = 'Approved'
      AND (quantity - (reserved_qty + sold_qty)) >= @request_qty;
    ```
  - Kiểm tra số dòng bị ảnh hưởng (`affected_rows`):
    - `affected_rows == 1`: Reserve slot thành công → Cho phép tiến hành tạo đơn hàng.
    - `affected_rows == 0`: Hết suất Flash Sale → Ném exception ngắt checkout ngay lập tức.
  - Khi thanh toán thành công: Cập nhật `sold_qty += qty`, `reserved_qty -= qty`. Nếu hủy/timeout: Cập nhật `reserved_qty -= qty`.
- **Dependencies**: T26, T22
- **Acceptance Criteria** (`AC-006`): Đảm bảo 100% không overselling ngay cả khi 1000 requests đồng thời tranh mua 10 sản phẩm Flash Sale.
- **Estimated Size**: L

---

## 🟡 EPIC 07 — PAYMENT & DIRECT SETTLEMENT

### T28 — [INFRA] MoMo Payment Gateway Adapter & IPN Handler
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev C
- **SRS Requirement**: `FR-PAY-001`, `FR-PAY-002`, `FR-PAY-003`, `2.4 Phụ thuộc ngoài`, `7.3 Webhook`
- **Detailed Technical Implementation**:
  - Implement class `MoMoPaymentAdapter : IPaymentGateway`.
  - Method `InitiateAsync`: Tạo HMAC-SHA256 signature với SecretKey MoMo, gọi MoMo Payment API (`POST /v2/gateway/api/create`) truyền `orderId`, `amount`, `orderInfo`, `redirectUrl`, `ipnUrl`. Trả về `payUrl` và `qrCodeUrl`.
  - Method `VerifyWebhookAsync`: Trích xuất payload IPN từ MoMo, tính lại HMAC-SHA256 signature đối chứng với `signature` từ MoMo gửi lên. Ném lỗi nếu signature không khớp.
  - Endpoint `POST /api/v1/payments/webhook/momo` → Gọi `HandleMoMoWebhookCommand`: Khi `resultCode == 0` → Cập nhật `Payments.status = Success`, `paid_at = NOW()`, `transaction_ref = transId`. Publish `PaymentSuccessEvent`.
- **Dependencies**: T07
- **API Contracts**: `MoMoWebhookRequest` (`NovaLive.Contracts.V1.Payments`)
- **Estimated Size**: M

---

### T29 — [INFRA] VietQR Payment Gateway Adapter & Webhook Handler
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev C
- **SRS Requirement**: `FR-PAY-001`, `FR-PAY-002`, `FR-PAY-003`, `2.4 Phụ thuộc ngoài`, `7.3 Webhook`
- **Detailed Technical Implementation**:
  - Implement class `VietQRPaymentAdapter : IPaymentGateway`.
  - Method `InitiateAsync`: Sinh chuỗi VietQR chuẩn EMVCo Động chứa số tài khoản ngân hàng, số tiền, và nội dung chuyển khoản bắt buộc `NOVA_{orderCode}`.
  - Endpoint `POST /api/v1/payments/webhook/vietqr` (Callback đối soát biến động số dư ngân hàng/OpenBanking):
    1. Xác thực HMAC Secret của Webhook.
    2. Trích xuất nội dung chuyển khoản `NOVA_{orderCode}`.
    3. Tra cứu `Payments` theo `orderCode`. Kiểm tra số tiền chuyển khớp `amount`.
    4. Cập nhật `Payments.status = Success`, `paid_at = NOW()`. Publish `PaymentSuccessEvent`.
- **Dependencies**: T07
- **API Contracts**: `VietQRWebhookRequest` (`NovaLive.Contracts.V1.Payments`)
- **Estimated Size**: M

---

### T30 — [BE/API] Payment Initiator & Realtime Status Query
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev A
- **SRS Requirement**: `FR-PAY-004`, `FR-PAY-005`, `FR-PAY-006`
- **Detailed Technical Implementation**:
  - `InitiatePaymentCommand`: Nhận `orderId`, `method` (MoMo / VietQR). Tra cứu `Payments` của order. Gọi `IPaymentGateway.InitiateAsync`, cập nhật `transaction_ref`, đặt thời hạn thanh toán `expired_at = NOW() + 15 phút`. Trả về `payUrl` / `qrCode`.
  - `GetPaymentStatusQuery`: Query trạng thái `Payments.status` từ DB/Redis cache cho frontend polling.
  - Endpoints `PaymentsController`: `POST /payments/initiate`, `GET /payments/{paymentId}/status`.
  - **Xử lý COD**: Đơn COD không cần gọi Initiate payment online. `Payments.status` giữ `Pending` cho đến khi đơn giao xong và ĐVVC đối soát COD thành công.
- **Dependencies**: T23, T28, T29
- **API Contracts**: `InitiatePaymentRequest`, `PaymentInitResponse`, `PaymentStatusResponse` (`NovaLive.Contracts.V1.Payments`)
- **Estimated Size**: M

---

### T31 — [REALTIME] SignalR PaymentNotificationHub Integration
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev C
- **SRS Requirement**: `FR-NOTI-003`, `7.2 Realtime API`
- **Detailed Technical Implementation**:
  - Implement `PaymentNotificationHub` tại route `wss://api.novalive.vn/hubs/payment`.
  - Client kết nối gửi JWT Access Token qua Query string (`?access_token=...`). Hub map `Context.ConnectionId` với `userId`.
  - Khi Consumer nhận được `PaymentSuccessEvent` → Gọi SignalR Hub push event `PaymentSuccess { orderId, amount, paymentMethod }` tới đúng client connection của Buyer đang mở màn hình quét mã QR.
  - Cấu hình Redis Backplane (`AddStackExchangeRedis`) cho SignalR để đồng bộ notification giữa các container instances trong môi trường production.
- **Dependencies**: T30
- **Estimated Size**: M

---

### T32 — [BE] Direct Payment Settlement & Shop Wallet Credit
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev A
- **SRS Requirement**: `FR-PAY-005`, `FR-WALLET-001`, `FR-WALLET-002`
- **Detailed Technical Implementation**:
  - `SettleOrderPaymentCommand` (chạy khi Payment online Success hoặc COD đã đối soát):
    1. Tra cứu các `SubOrders` thuộc `ParentOrder`.
    2. Với mỗi SubOrder của từng Shop:
       - Tính số tiền thực nhận của Shop: $\text{shop\_net} = \text{sub\_order\_total} - \text{platform\_fee}$.
       - Cộng trực tiếp vào ví Shop: `ShopWallets.balance += shop_net`.
       - Ghi log sổ cái `ShopWalletTransactions` loại `OrderRevenue` với `ref_type = 'Order'`, `ref_id = subOrderId`.
    3. Cập nhật `SubOrders.status = Confirmed` (chuyển sang bước đóng gói giao hàng).
    4. Gửi SignalR notification thông báo đơn hàng mới và doanh thu đã cộng cho Seller.
- **Dependencies**: T23
- **Acceptance Criteria** (`AC-007`): Tiền thanh toán thành công được cộng ngay lập tức vào `balance` của từng Shop tương ứng, sổ cái `ShopWalletTransactions` ghi nhận đầy đủ.
- **Estimated Size**: M

---

## 🟡 EPIC 08 — SHIPPING & FULFILLMENT

### T33 — [INFRA] GHN Shipping Adapter & Webhook Integrator
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev C
- **SRS Requirement**: `FR-SHIP-001`, `FR-SHIP-003`, `FR-SHIP-004`, `2.4 Phụ thuộc ngoài`
- **Detailed Technical Implementation**:
  - Implement `GhnShippingAdapter : IShippingProvider`:
    - `CalculateFeeAsync`: Gọi GHN Fee API (`POST /shiip/public-api/v2/shipping-order/fee`) truyền `from_district_id`, `to_district_id`, `to_ward_code`, `weight`. Trả về số tiền cước.
    - `CreateShipmentAsync`: Gọi GHN Create Order API, truyền thông tin người gửi (kho shop), người nhận, danh sách hàng, COD amount. Trả về `tracking_code` (mã vận đơn GHN).
    - `GetShippingLabelAsync`: Gọi GHN Label API tải file PDF phiếu giao hàng.
  - Endpoint `POST /api/v1/shipping/webhook/ghn`: Xác thực checksum header, trích xuất `OrderCode`, `Status` (`ready_for_pick`, `picking`, `storing`, `delivering`, `delivered`). Gọi `HandleShippingWebhookCommand`.
- **Dependencies**: T07
- **API Contracts**: `CalculateShippingFeeRequest`, `ShippingFeeResponse`, `GhnWebhookRequest` (`NovaLive.Contracts.V1.Shipping`)
- **Estimated Size**: M

---

### T34 — [INFRA] GHTK & ViettelPost Shipping Adapters
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev C
- **SRS Requirement**: `FR-SHIP-001`, `FR-SHIP-003`, `FR-SHIP-004`, `2.4 Phụ thuộc ngoài`
- **Detailed Technical Implementation**:
  - Implement `GhtkShippingAdapter` & `ViettelPostShippingAdapter` kế thừa `IShippingProvider`.
  - Tích hợp API tính cước, tạo đơn vận chuyển và tra cứu mã vận đơn với GHTK và ViettelPost.
  - Webhook endpoints: `POST /shipping/webhook/ghtk`, `POST /shipping/webhook/viettelpost` nhận cập nhật hành trình vận đơn tự động.
- **Dependencies**: T07
- **API Contracts**: `GhtkWebhookRequest`, `ViettelPostWebhookRequest` (`NovaLive.Contracts.V1.Shipping`)
- **Estimated Size**: M

---

### T35 — [BE/API] Seller Shipping Fulfillment & Label PDF Generator
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev C
- **SRS Requirement**: `FR-SHIP-002`, `FR-SHIP-005`
- **Detailed Technical Implementation**:
  - `CreateShipmentOrderCommand` (Seller bấm "Giao hàng"):
    1. Kiểm tra `SubOrder.status == Confirmed`.
    2. Lấy thông tin kho shop (`ShopAddresses`), địa chỉ nhận Buyer snapshot.
    3. Gọi `IShippingProvider.CreateShipmentAsync` theo nhà vận chuyển đã chọn (GHN/GHTK/ViettelPost).
    4. Lưu bản ghi `ShippingOrders` chứa `tracking_code`, `provider_order_id`, `shipping_fee`.
    5. Cập nhật `SubOrder.status = Shipping`, `shipped_at = NOW()`.
  - `GetShippingLabelQuery`: Gọi `IShippingProvider.GetShippingLabelAsync` stream file PDF phiếu giao hàng chuẩn khổ in A6 cho Seller in dán lên thùng hàng.
  - `GetOrderTrackingQuery`: Tra cứu hành trình vận đơn thời gian thực cho Buyer/Seller xem.
  - Endpoints: `POST /seller/shipping/create-order`, `GET /seller/shipping/{subOrderId}/label`, `GET /orders/{orderId}/tracking`.
- **Dependencies**: T33, T34, T23
- **API Contracts**: `CreateShipmentRequest`, `ShipmentResponse`, `TrackingResponse` (`NovaLive.Contracts.V1.Shipping`)
- **Estimated Size**: M

---

### T36 — [BE] Shipping Webhook Handler & Delivery Status Sync
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev C
- **SRS Requirement**: `FR-SHIP-006`, `FR-SHIP-007`, `FR-SHIP-008`, `NFR-REL-004`
- **Detailed Technical Implementation**:
  - `HandleShippingWebhookCommand`:
    1. Tra cứu `ShippingOrders` theo `tracking_code`.
    2. Cập nhật `ShippingOrders.status`. Cập nhật `SubOrders.status` tương ứng (`Picking` -> `Shipping` -> `Delivered`).
    3. Khi Webhook báo trạng thái `Delivered`:
       - Cập nhật `SubOrders.delivered_at = NOW()`, `status = Delivered`.
       - Mở quyền tạo Đánh giá (Review) cho Buyer đối với các sản phẩm trong SubOrder.
       - Ghi `OutboxMessage(ShipmentDeliveredEvent)`.
  - **Polling Fallback Background Job**: Job chạy 6 tiếng/lần quét các `ShippingOrders` đang ở trạng thái `Delivering` quá 3 ngày không nhận được webhook → Chủ động gọi API tra cứu trạng thái của ĐVVC để tự động cập nhật `Delivered`.
- **Dependencies**: T35, T32
- **Estimated Size**: M

---

## 🟡 EPIC 09 — RETURNS & DISPUTES

### T37 — [BE/API] Return Request Lifecycle (Buyer Claim ≤ 7 Days)
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev A
- **SRS Requirement**: `FR-RETURN-001`, `FR-RETURN-002`, `FR-RETURN-003`
- **Detailed Technical Implementation**:
  - `RequestReturnCommand` (Buyer yêu cầu trả hàng / hoàn tiền):
    1. Kiểm tra `SubOrder.status == Delivered`.
    2. **Kiểm tra thời hạn 7 ngày**: `NOW() <= SubOrder.delivered_at + 7 days`. Nếu quá 7 ngày → Báo lỗi `400 Bad Request` ("Đã hết thời hạn khiếu nại trả hàng").
    3. Tạo bản ghi `OrderReturns` (`reason`, `evidence_urls`, `status = Pending`). Tạo `OrderReturnItems` cho từng món cần trả.
  - Endpoints: `POST /returns`, `GET /returns`, `GET /returns/{id}`.
- **Dependencies**: T36
- **API Contracts**: `CreateReturnRequest`, `ReturnDetailResponse` (`NovaLive.Contracts.V1.Returns`)
- **Acceptance Criteria** (`AC-009`): Tạo yêu cầu trả hàng thành công trong vòng 7 ngày kể từ khi nhận hàng.
- **Estimated Size**: M

---

### T38 — [BE/API] Seller Return Response & Inspection Confirmation
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev A
- **SRS Requirement**: `FR-RETURN-004`, `FR-RETURN-005`
- **Detailed Technical Implementation**:
  - `ApproveReturnCommand` (Seller đồng ý): Chuyển `OrderReturns.status = SellerApproved`. Buyer tiến hành gửi hàng trả về địa chỉ kho shop (`ShopAddresses.is_return`).
  - `RejectReturnCommand` (Seller từ chối): Nhập lý do từ chối + ảnh/video đối chứng. Chuyển `OrderReturns.status = SellerRejected`.
  - `ConfirmReturnReceivedCommand` (Seller xác nhận đã nhận lại hàng hoàn):
    - Chuyển `OrderReturns.status = Completed`, `SubOrders.status = Returned`.
    - Thực hiện hoàn tiền từ tài khoản Shop cho Buyer: trừ `ShopWallets.balance -= refundAmount`, ghi sổ cái `ShopWalletTransactions` loại `OrderRefund`.
    - Cộng lại tồn kho vật lý cho các sản phẩm còn nguyên vẹn: `Inventories.qty_on_hand += qty`, ghi `InventoryHistories(ReturnIn)`.
  - Endpoints `SellerReturnsController`: `GET /seller/returns`, `PUT /seller/returns/{id}/approve`, `PUT /seller/returns/{id}/reject`, `PUT /seller/returns/{id}/received`.
- **Dependencies**: T37
- **Estimated Size**: M

---

### T39 — [BE/API] Admin Dispute Arbitration & Refund Resolution
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev A
- **SRS Requirement**: `FR-RETURN-006`, `FR-RETURN-007`, `FR-RETURN-008`, `FR-ADMIN-005`
- **Detailed Technical Implementation**:
  - **Tự động leo thang Tranh chấp**: Background Job quét các `OrderReturns WHERE status IN ('Pending', 'SellerRejected')` quá 3 ngày không thỏa thuận xong → Tự động chuyển `status = AdminDispute` lên Admin phân xử.
  - `ResolveDisputeCommand` (Admin đưa ra phán quyết cuối cùng):
    - **Nếu `decision == BuyerWins`**: Chuyển `OrderReturns.status = AdminApproved`. Trừ tiền từ tài khoản Shop để hoàn tiền cho Buyer (ghi `ShopWalletTransactions` loại `OrderRefund`). Ghi nhận lỗi vi phạm vào điểm uy tín của Shop.
    - **Nếu `decision == SellerWins`**: Chuyển `OrderReturns.status = AdminRejected`. Giữ nguyên đơn hàng hoàn thành cho Seller.
  - Endpoints `AdminDisputesController`: `GET /admin/disputes`, `PUT /admin/disputes/{returnId}/resolve`.
- **Dependencies**: T38, T32
- **API Contracts**: `ResolveDisputeRequest`, `DisputeResponse` (`NovaLive.Contracts.V1.Returns`)
- **Estimated Size**: M

---

## 🟡 EPIC 10 — LIVESTREAM COMMERCE

### T40 — [INFRA] Agora RTC Token Generation Service
- **Status**: MISSING | **Priority**: P2 | **Assign**: Dev C
- **SRS Requirement**: `FR-LIVE-002`, `2.4 Phụ thuộc ngoài`
- **Detailed Technical Implementation**:
  - Implement class `AgoraTokenService : IAgoraTokenService` sử dụng thư viện Agora Dynamic Key C#.
  - Method `GeneratePublisherToken(string channelName, string userId)`: Sinh Agora RTC Token cho Seller (Role: Publisher) với quyền phát Video/Audio stream lên kênh. TTL 2 giờ.
  - Method `GenerateSubscriberToken(string channelName, string userId)`: Sinh Agora RTC Token cho Viewer (Role: Subscriber) chỉ có quyền xem stream. TTL 2 giờ.
- **Dependencies**: T07
- **Estimated Size**: S

---

### T41 — [BE/API] Livestream Session Lifecycle & Product Pinning
- **Status**: MISSING | **Priority**: P2 | **Assign**: Dev C
- **SRS Requirement**: `FR-LIVE-001`, `FR-LIVE-003`, `FR-LIVE-005`, `FR-LIVE-007`, `FR-LIVE-008`
- **Detailed Technical Implementation**:
  - `StartLiveSessionCommand` (Seller): Tạo `LivestreamSessions` (`status = Live`, `agora_channel_name = "live_{shopId}_{uuid}"`), gọi `IAgoraTokenService.GeneratePublisherToken` trả về client.
  - `EndLiveSessionCommand` (Seller): Đổi `status = Ended`, `ended_at = NOW()`, tính toán tổng kết `peak_viewer_count` và tổng doanh số phát sinh trong buổi live.
  - `PinProductCommand` (Seller ghim sản phẩm): Tạo/cập nhật `LivestreamProducts` với `is_pinned = TRUE`, gán `flash_price` riêng trong live và thời hạn đếm ngược. Broadcast SignalR event `ProductPinned`.
  - `UnpinProductCommand`: Gỡ ghim sản phẩm. Broadcast SignalR event `ProductUnpinned`.
  - Endpoints `LivestreamsController`: Public `GET /livestreams/active`, `GET /livestreams/{id}`; Seller `POST /seller/livestreams/start`, `POST /seller/livestreams/{id}/end`, `POST/DELETE /seller/livestreams/{id}/pin-product`.
- **Dependencies**: T40, T16, T08
- **API Contracts**: `StartLivestreamRequest`, `PinProductRequest`, `LivestreamSessionResponse`, `PinnedProductResponse` (`NovaLive.Contracts.V1.Livestreams`)
- **Estimated Size**: L

---

### T42 — [REALTIME] SignalR LivestreamHub (Chat, Reaction, Pin, Viewers)
- **Status**: MISSING | **Priority**: P2 | **Assign**: Dev C
- **SRS Requirement**: `FR-LIVE-004`, `FR-LIVE-006`, `7.2 Realtime API`, `NFR-PERF-003`
- **Detailed Technical Implementation**:
  - Implement `LivestreamHub` tại route `wss://api.novalive.vn/hubs/livestream`.
  - Client Actions:
    - `JoinLiveSession(sessionId)`: Tham gia SignalR Group `live_{sessionId}`. Tăng `viewer_count`, broadcast event `ViewerCountUpdated` tới toàn phòng.
    - `SendMessage(content)`: Người xem gửi bình luận. Ghi vào DB `LivestreamComments` và lưu 100 comment mới nhất vào Redis List cache. Broadcast event `ReceiveMessage`.
    - `SendReaction(type)`: Người xem thả tim. Gom batch thả tim mỗi 500ms broadcast event `ReceiveReaction` (hiệu ứng tim bay).
  - Server Broadcast Events: `ProductPinned`, `ProductUnpinned`, `ViewerCountUpdated`, `ReceiveMessage`, `ReceiveReaction`.
- **Dependencies**: T41
- **Acceptance Criteria** (`AC-011`): Latency push comment và reaction trong phòng live P95 < 300ms.
- **Estimated Size**: L

---

### T43 — [REALTIME] SignalR OrderNotificationHub Integration
- **Status**: MISSING | **Priority**: P2 | **Assign**: Dev C
- **SRS Requirement**: `FR-NOTI-002`, `7.2 Realtime API`
- **Detailed Technical Implementation**:
  - Implement `OrderNotificationHub` tại route `wss://api.novalive.vn/hubs/order`.
  - Seller kết nối Hub được join vào Group `seller_{shopId}`.
  - Khi `OrderPlacedConsumer` nhận tin nhắn đơn mới từ RabbitMQ → Push realtime event `OrderPlaced { subOrderId, shopId, itemCount, totalAmount }` thông báo âm thanh và pop-up đơn mới cho Seller.
  - Push event `OrderStatusChanged` tới Buyer khi trạng thái đơn thay đổi.
- **Dependencies**: T23
- **Estimated Size**: S

---

## 🟡 EPIC 11 — REVIEWS & RATINGS

### T44 — [BE/API] Review & Rating Management (Edit-once, Reply, Moderation)
- **Status**: MISSING | **Priority**: P2 | **Assign**: Dev B
- **SRS Requirement**: `FR-REVIEW-001`, `FR-REVIEW-002`, `FR-REVIEW-003`, `FR-REVIEW-004`, `FR-REVIEW-005`, `FR-REVIEW-006`
- **Detailed Technical Implementation**:
  - `CreateReviewCommand` (Buyer):
    1. Kiểm tra `SubOrder.status == Delivered`.
    2. **Ràng buộc UNIQUE**: Kiểm tra `order_item_id` chưa từng được đánh giá.
    3. Tạo bản ghi `Reviews` (`rating` 1-5 sao, `title`, `content`), chèn ảnh/video vào `ReviewImages`.
  - `UpdateReviewCommand` (Buyer sửa đánh giá):
    1. **Giới hạn 1 lần**: Kiểm tra `edit_count == 0`. Nếu `edit_count >= 1` → Báo lỗi ("Chỉ được chỉnh sửa đánh giá 1 lần duy nhất").
    2. **Giới hạn 30 ngày**: Kiểm tra `NOW() <= created_at + 30 ngày`.
    3. Cập nhật nội dung, tăng `edit_count = 1`.
  - `ReplyReviewCommand` (Seller): Phản hồi công khai 1 lần duy nhất vào `seller_reply`.
  - `HideReviewCommand` (Admin): Đặt `is_visible = FALSE` cho review vi phạm.
  - Endpoints: `GET /products/{spuId}/reviews`, `POST /reviews`, `PUT /reviews/{id}`, `POST /seller/reviews/{id}/reply`, `PUT /admin/reviews/{id}/hide`.
- **Dependencies**: T36, T08
- **API Contracts**: `CreateReviewRequest`, `UpdateReviewRequest`, `ReplyReviewRequest`, `ReviewResponse` (`NovaLive.Contracts.V1.Reviews`)
- **Estimated Size**: M

---

### T45 — [WORKER] Rating Calculation Background Worker
- **Status**: MISSING | **Priority**: P2 | **Assign**: Dev C
- **SRS Requirement**: `FR-REVIEW-007`
- **Detailed Technical Implementation**:
  - Implement Background Worker `RatingCalculationWorker` chạy định kỳ 5 phút/lần.
  - Quét các `Spus` và `Shops` có review mới được tạo/sửa.
  - Tính toán điểm sao trung bình:
    $$\text{rating\_avg} = \frac{\sum \text{rating}}{\text{count(reviews)}} \quad (\text{với } \text{is\_visible} = \text{TRUE})$$
  - Cập nhật `Shops.rating_avg`, `Shops.rating_count`.
- **Dependencies**: T44
- **Estimated Size**: S

---

## 🟡 EPIC 12 — BACKGROUND WORKERS & EVENT-DRIVEN

### T46 — [WORKER] OrderPlacedConsumer (Email/SMS + Push Noti)
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev C
- **SRS Requirement**: `FR-NOTI-001`, `FR-NOTI-002`, `FR-EVENT-001`, `FR-EVENT-002`
- **Detailed Technical Implementation**:
  - Implement MassTransit Consumer `OrderPlacedConsumer : IConsumer<OrderPlacedEvent>`.
  - Đọc event từ RabbitMQ (do Outbox Worker publish).
  - Gửi Email HTML xác nhận đơn hàng chi tiết cho Buyer.
  - Tạo bản ghi `Notifications` in-app cho Buyer và Seller.
  - Gọi `OrderNotificationHub` push thông báo đơn mới realtime tới gian hàng của Seller.
- **Dependencies**: T22
- **Estimated Size**: M

---

### T47 — [WORKER] InventorySyncConsumer (Deduct Stock After Paid)
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev C
- **SRS Requirement**: `FR-CAT-010`, `FR-EVENT-003`, `NFR-REL-003`
- **Detailed Technical Implementation**:
  - Implement MassTransit Consumer `InventorySyncConsumer : IConsumer<PaymentSuccessEvent>`.
  - **Idempotency Check**: Kiểm tra event đã xử lý chưa bằng `RedisIdempotencyService`. Nếu đã xử lý → Bỏ qua.
  - Với từng `OrderItem` trong đơn hàng thanh toán thành công:
    - Trừ tồn kho vật lý và giải phóng giữ chỗ:
      `Inventories.qty_on_hand -= quantity`
      `Inventories.reserved_qty -= quantity`
    - Ghi bản ghi sổ cái `InventoryHistories` loại `SaleConfirmed`.
- **Dependencies**: T30, T22
- **Acceptance Criteria**: Đảm bảo trừ kho chính xác 1 lần duy nhất ngay cả khi event bị lặp (Idempotent execution).
- **Estimated Size**: M

---

### T48 — [WORKER] CODReconciliationConsumer (COD Order Settlement & Wallet Credit)
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev C
- **SRS Requirement**: `FR-PAY-003`, `AC-008`
- **Detailed Technical Implementation**:
  - Implement Background Job `CODReconciliationWorker` / Consumer xử lý khi nhận webhook xác nhận giao hàng thu tiền COD thành công từ ĐVVC.
  - Khi đơn hàng COD chuyển trạng thái `Delivered`:
  - Thực thi ghi nhận doanh thu: cộng tiền `ShopWallets.balance += shop_net_amount` cho ví Seller và ghi log sổ cái `ShopWalletTransactions` loại `OrderRevenue`.
- **Dependencies**: T30, T36
- **Estimated Size**: M

---

### T49 — [WORKER] TimeoutOrderRollbackWorker & ProductSearchVectorConsumer
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev C
- **SRS Requirement**: `FR-CAT-011`, `FR-PAY-004`, `FR-SEARCH-004`
- **Detailed Technical Implementation**:
  - `TimeoutOrderRollbackWorker`: Job chạy mỗi 1 phút quét các `Payments WHERE status = 'Pending' AND expired_at <= NOW()`. Cập nhật `Payments.status = Expired`, `ParentOrders.payment_status = Cancelled`, `SubOrders.status = Cancelled`. Hoàn trả tồn kho giữ chỗ: `Inventories.reserved_qty -= quantity`, ghi log `InventoryHistories(ReserveRelease)`.
  - `ProductSearchVectorConsumer`: Consumer lắng nghe `ProductUpdatedEvent` -> Cập nhật `Spus.search_vector = to_tsvector('vietnamese', name || ' ' || description || ' ' || brand)` trong PostgreSQL async.
- **Dependencies**: T22, T16
- **Estimated Size**: M

---

## 🟡 EPIC 13 — REPORTS & DASHBOARD

### T50 — [BE/API] Seller Performance Dashboard & Wallet Analytics
- **Status**: MISSING | **Priority**: P2 | **Assign**: Dev B
- **SRS Requirement**: `FR-REPORT-002`
- **Detailed Technical Implementation**:
  - `GetSellerDashboardQuery`:
    - Tổng doanh thu thuần từ các đơn hàng (doanh thu đã ghi nhận trực tiếp vào ví Shop).
    - Biểu đồ doanh thu theo ngày/tuần/tháng trong khoảng `from` - `to`.
    - Phân rã số đơn theo trạng thái (Pending, Shipping, Delivered, Returned, Cancelled).
    - Top 10 sản phẩm bán chạy nhất của shop (theo số lượng và doanh thu).
    - Tổng quan ví Shop: Số dư khả dụng (`balance`) và lịch sử biến động số dư.
    - Thống kê hiệu quả doanh số phát sinh từ các buổi phát Livestream.
  - Endpoint `ReportsController`: `GET /seller/reports/dashboard`.
- **Dependencies**: T30, T36
- **API Contracts**: `SellerDashboardResponse` (`NovaLive.Contracts.V1.Dashboards`)
- **Estimated Size**: M

---

### T51 — [BE/API] Admin Platform Executive Dashboard & Financial Reports
- **Status**: MISSING | **Priority**: P2 | **Assign**: Dev B
- **SRS Requirement**: `FR-REPORT-001`, `FR-ADMIN-006`
- **Detailed Technical Implementation**:
  - `GetAdminDashboardQuery`:
    - GMV (Gross Merchandise Value) toàn sàn theo thời gian.
    - Doanh thu hoa hồng thực nhận của Sàn (Net Platform Revenue = $\sum \text{platform\_fee}$ từ các đơn hàng hoàn tất).
    - Tổng số người dùng mới (Buyers, Sellers), tổng số gian hàng `Active`.
    - Tỷ lệ hoàn hàng & Tỷ lệ tranh chấp toàn sàn.
    - Danh sách các lệnh rút tiền `SellerPayouts` đang chờ duyệt.
  - Endpoint `AdminReportsController`: `GET /admin/reports/dashboard`.
- **Dependencies**: T30, T39
- **API Contracts**: `AdminDashboardResponse` (`NovaLive.Contracts.V1.Dashboards`)
- **Estimated Size**: M

---

## 🟡 EPIC 14 — TESTING & DEVOPS

### T52 — [TEST] Domain & Application Unit Tests
- **Status**: MISSING | **Priority**: P1 | **Assign**: All
- **SRS Requirement**: `9. Yêu cầu kiểm thử tối thiểu`, `NFR-MAINT-005`
- **Detailed Technical Implementation**:
  - Viết Unit Tests với xUnit + FluentAssertions + Moq trong project `NovaLive.Application.Tests` và `NovaLive.Domain.Tests`:
  - Test `PriceCalculator`: Bao phủ 100% các case proration phân bổ Voucher Sàn, voucher hết lượt, voucher chưa đến hạn, đơn dưới min_order_amount.
  - Test `CheckoutCommand`: Mock repository test đúng 8 bước transaction rollback khi có lỗi.
  - Test `AtomicFlashSaleReserve`, `DirectPaymentSettlement`, `TokenReuseDetection`. Target test coverage >= 80%.
- **Dependencies**: T20, T22, T27
- **Estimated Size**: L

---

### T53 — [TEST] Integration Tests (Auth, Checkout, Webhooks)
- **Status**: MISSING | **Priority**: P1 | **Assign**: All
- **SRS Requirement**: `9. Yêu cầu kiểm thử tối thiểu`
- **Detailed Technical Implementation**:
  - Viết Integration Tests trong `NovaLive.Api.Tests` sử dụng `WebApplicationFactory` + TestContainers (PostgreSQL 17 & Redis containers):
  - Test Auth flow: Đăng ký -> Verify OTP -> Đăng nhập -> Refresh Token -> Reuse Token Detection.
  - Test Checkout flow: Add Cart -> Calculate -> Submit Checkout -> MoMo Webhook -> Direct Shop Wallet Settlement & Inventory Sync.
  - Test Shipping Webhook -> Delivered -> Review eligibility enabled.
- **Dependencies**: T08, T30, T36
- **Estimated Size**: L

---

### T54 — [TEST] End-to-End Tests (Full Purchase to Direct Settlement & Dispute Cycle)
- **Status**: MISSING | **Priority**: P1 | **Assign**: All
- **SRS Requirement**: `8. Tiêu chí chấp nhận cấp hệ thống` (`AC-001` đến `AC-012`)
- **Detailed Technical Implementation**:
  - Giả lập kịch bản E2E toàn vẹn: Buyer đăng ký -> Mua hàng đa shop -> Thanh toán MoMo / VietQR -> Tiền vào ví Shop ngay -> Seller đóng gói & giao hàng -> GHN Webhook Delivered -> Buyer gửi khiếu nại Trả hàng -> Admin phân xử BuyerWins -> Thu hồi tiền hoàn về Buyer.
- **Dependencies**: T30, T39
- **Estimated Size**: M

---

### T55 — [DEVOPS] Production Docker Stack (Nginx SSL + Health Checks)
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev C
- **SRS Requirement**: `NFR-OPS-001`, `NFR-OPS-002`, `NFR-OPS-003`
- **Detailed Technical Implementation**:
  - Xây dựng `docker-compose.production.yml` và `docker/nginx/nginx.conf`:
  - Nginx Reverse Proxy SSL (HTTPS port 443 / WSS port 443): Proxy `/api/v1/*` về CoreApi container, `/hubs/*` về RealtimeApi container.
  - Tối ưu Dockerfile Multi-stage build cho CoreApi và RealtimeApi (alpine runtime image).
  - Tích hợp Health Check Endpoints `/api/v1/health` kiểm tra kết nối Postgres, Redis, RabbitMQ, MinIO.
- **Dependencies**: T08, T02
- **Estimated Size**: M

---

## 🔵 EPIC 15 — FRONTEND WEB APPLICATIONS (NEXT.JS / REACT)

### T56 — [FE] Setup Frontend Boilerplate, Auth Hub & User Profile
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev FE
- **SRS Requirement**: `FR-AUTH-001` đến `FR-AUTH-011`, `FR-USER-001` đến `FR-USER-004`
- **Detailed Technical Implementation**:
  1. **Boilerplate & Core Architecture**:
     - Khởi tạo Next.js 15 (App Router), TypeScript 5, Tailwind CSS v4, Lucide Icons, Shadcn UI / Radix UI components.
     - Cấu hình Axios / Fetch Client Interceptors: tự động gắn `Bearer {accessToken}` vào HTTP Request. Lắng nghe HTTP 401: Gọi API `/auth/refresh-token` xoay vòng Refresh Token tự động. Nếu thất bại -> Xóa token state và chuyển hướng về `/login`.
     - Zustand / Redux Toolkit Store: Quản lý Auth state (`currentUser`, `accessToken`, `roles`, `permissions`, `isLoggedIn`).
  2. **Auth Pages & Modals**:
     - Page `/login`: Form đăng nhập Email/Password với validation Zod.
     - Page `/register`: Form đăng ký Buyer/Seller. Modal nhập mã OTP 6 chữ số kèm đếm ngược 60s để resend OTP.
     - Page `/forgot-password` & `/reset-password`.
  3. **User Profile & Address Book**:
     - Page `/user/profile`: Xem và cập nhật thông tin cá nhân (Full name, Avatar preview upload, Phone, Birthday, Gender).
     - Page `/user/addresses`: Sổ địa chỉ giao hàng. Modal Thêm mới / Cập nhật địa chỉ với Cascading Dropdowns (Tỉnh/Thành -> Quận/Huyện -> Phường/Xã). Đặt địa chỉ mặc định, Xóa địa chỉ với popup xác nhận.
  4. **Route Protection**: Next.js Middleware (`middleware.ts`) bảo vệ các tuyến đường riêng tư (`/user/*`, `/seller/*`, `/admin/*`).
- **Dependencies**: T08, T10
- **Estimated Size**: L

---

### T57 — [FE] Buyer Marketplace & Product Search / Details Portal
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev FE
- **SRS Requirement**: `FR-CAT-001` đến `FR-CAT-009`, `FR-SEARCH-001` đến `FR-SEARCH-004`, `FR-REVIEW-001` đến `FR-REVIEW-006`
- **Detailed Technical Implementation**:
  - **Marketplace Homepage**: Top Navigation Bar (Logo, Category Mega Menu, Dynamic Search bar, Cart badge with item count, User Profile Avatar). Banner Carousel, Khối Flash Sale với đồng hồ đếm ngược thời gian thực, Top Sản phẩm bán chạy & Shop nổi bật.
  - **Product Search & Catalog Filtering Page (`/search`, `/category/[id]`)**:
    - Input tìm kiếm với Debounce (300ms).
    - Bộ lọc bên sidebar: Cây danh mục đa cấp, Khoảng giá (Price Slider), Chấm sao Đánh giá (1-5 sao), Lọc theo Shop, Lọc hàng có sẵn.
    - Sắp xếp (Giá tăng/giảm, Bán chạy nhất, Mới nhất) và Phân trang Pagination / Infinite Scrolling.
  - **Product Detail Page (PDP - `/products/[spuId]`)**:
    - Image Gallery (Thumbnail selector, Image Zoom).
    - Ma trận chọn biến thể SKU (Variant Matrix: Màu sắc, Size...): Tự động khớp SKU khi Buyer bấm chọn, cập nhật giá tương ứng và hiển thị tồn kho vật lý (`qty_on_hand`).
    - Khối thông tin Shop snapshot (Rating shop, button "Ghé Shop" / "Chat ngay").
    - Tab Mô tả chi tiết & Thông số kỹ thuật.
    - Tab Reviews: Danh sách đánh giá phân loại theo số sao, hình ảnh/video từ Buyer. Form gửi Đánh giá & Phản hồi (chấm sao, upload media, cho phép sửa 1 lần duy nhất).
- **Dependencies**: T16, T18, T44
- **Estimated Size**: XL

---

### T58 — [FE] Cart, Multi-shop Checkout & Payment Flow
- **Status**: MISSING | **Priority**: P0 | **Assign**: Dev FE
- **SRS Requirement**: `FR-CART-001` đến `FR-CART-005`, `FR-ORD-001` đến `FR-ORD-008`, `FR-PAY-001` đến `FR-PAY-006`
- **Detailed Technical Implementation**:
  - **Cart Page (`/cart`)**:
    - Giao diện giỏ hàng gom nhóm sản phẩm theo từng Shop riêng biệt.
    - Checkbox chọn sản phẩm thanh toán toàn bộ hoặc chọn từng shop. Tăng/giảm số lượng (kiểm tra max stock), nút Xóa sản phẩm.
    - Modal áp dụng Voucher: Voucher Gian hàng (Shop Voucher) và Voucher Sàn (Platform Voucher) với thông báo mức giảm tối đa.
  - **Checkout Page (`/checkout`)**:
    - Bộ chọn địa chỉ nhận hàng từ Address Book.
    - Bảng tổng quan đơn hàng phân rã theo SubOrder từng Shop: Preview phí vận chuyển thời gian thực từ ĐVVC, mức giảm giá Voucher 3 cấp proration.
    - Bộ chọn Đơn vị vận chuyển (GHN / GHTK / ViettelPost) và Phương thức thanh toán (MoMo / VietQR / COD).
  - **Payment Gateways & Realtime QR Integration**:
    - Bấm "Đặt hàng": Submit checkout command API.
    - Nếu chọn MoMo hoặc VietQR: Hiển thị Modal quét mã QR Code thanh toán thời gian thực kèm đếm ngược 15 phút.
    - Tích hợp **SignalR Client (`PaymentNotificationHub`)**: Kết nối WSS lắng nghe event `PaymentSuccess`. Ngay khi quét mã xong -> Tự động ẩn QR Modal, phát hiệu ứng thành công và chuyển hướng tới `/orders/[id]/success`.
- **Dependencies**: T19, T23, T30, T31
- **Estimated Size**: XL

---

### T59 — [FE] Seller Center Portal (Shop, Products, Orders & Shipping)
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev FE
- **SRS Requirement**: `FR-SHOP-001` đến `FR-SHOP-007`, `FR-SHIP-001` đến `FR-SHIP-008`
- **Detailed Technical Implementation**:
  - **Seller Onboarding & Settings (`/seller/setup`, `/seller/settings`)**: Form đăng ký gian hàng, Upload tài liệu KYC (CCCD/ĐKKD) lên MinIO, Quản lý địa chỉ kho gửi hàng & kho trả hàng.
  - **Product Management (`/seller/products`)**:
    - Form tạo mới / chỉnh sửa SPU: Upload nhiều ảnh vào MinIO S3 bucket `products`, nhập tên, danh mục, mô tả chi tiết.
    - Bảng khởi tạo biến thể SKU: Tạo ma trận biến thể (Color, Size...), nhập giá gốc, giá bán, mã SKU, số lượng tồn kho.
    - Bật/Tắt trạng thái kinh doanh (`is_active`), Xóa/Ẩn sản phẩm.
  - **Order Fulfillment & Shipping (`/seller/orders`)**:
    - Bảng danh sách đơn hàng SubOrders phân theo Tab trạng thái (Chờ xác nhận, Đang chuẩn bị, Đang giao, Đã giao, Trả hàng, Đã hủy).
    - Thao tác "Giao hàng": Chọn ĐVVC, kích hoạt tạo mã vận đơn (`tracking_code`).
    - Nút "In phiếu giao hàng": Mở cửa sổ in file PDF phiếu giao khổ A6 chuẩn nhà vận chuyển. Xem hành trình vận đơn realtime.
  - **Ví Gian Hàng & Analytics (`/seller/wallet`, `/seller/reports`)**: Xem số dư khả dụng (`balance`), lịch sử giao dịch sổ cái. Form tạo lệnh rút tiền về tài khoản ngân hàng.
- **Dependencies**: T11, T13, T16, T35
- **Estimated Size**: XL

---

### T60 — [FE] Livestream Commerce Portal (Agora RTC + SignalR Realtime)
- **Status**: MISSING | **Priority**: P2 | **Assign**: Dev FE
- **SRS Requirement**: `FR-LIVE-001` đến `FR-LIVE-008`, `FR-NOTI-003`
- **Detailed Technical Implementation**:
  - **Seller Live Studio (`/seller/livestream/studio`)**:
    - Tích hợp **Agora Web RTC SDK (Publisher Mode)**: Preview camera video, mic selector, kiểm tra bitrate/fps. Nút "Bắt đầu Live" xin Agora RTC token từ API và phát sóng.
    - Khối quản lý Sản phẩm Ghim: Chọn sản phẩm trong gian hàng để ghim (`Pin Product`) kèm giá Flash price trong live. Nút gỡ ghim (`Unpin Product`).
    - Frame Chat realtime & Thống kê lượt xem (Peak viewers, current viewers). Nút "Kết thúc Live" hiển thị popup tổng kết doanh số buổi live.
  - **Buyer Livestream Viewer (`/livestreams/[id]`)**:
    - Giao diện xem Live dạng dọc chuẩn Mobile / Responsive Desktop. Player **Agora Web RTC SDK (Subscriber Mode)** độ trễ siêu thấp (< 1s).
    - Floating Heart Reaction (hiệu ứng tim bay Canvas/CSS animation), Khung chat tự động cuộn.
    - Dynamic Pinned Product Banner: Hiển thị sản phẩm đang ghim góc màn hình. Bấm Banner -> Mở Quick-Buy Drawer cho phép chọn biến thể SKU, chọn Voucher và Đặt hàng ngay mà không làm gián đoạn video livestream.
  - **SignalR Client (`LivestreamHub`)**: Lắng nghe & gửi sự kiện chat, thả tim, cập nhật banner ghim sản phẩm, đếm số người xem thời gian thực.
- **Dependencies**: T41, T42
- **Estimated Size**: XL

---

### T61 — [FE] Returns, Admin Operations & Executive Dashboard
- **Status**: MISSING | **Priority**: P1 | **Assign**: Dev FE
- **SRS Requirement**: `FR-RETURN-001` đến `FR-RETURN-008`, `FR-ADMIN-001` đến `FR-ADMIN-006`, `FR-REPORT-001`
- **Detailed Technical Implementation**:
  - **Returns & Refunds Portal (`/user/returns`, `/seller/returns`)**:
    - Buyer UI: Form gửi yêu cầu Trả hàng / Hoàn tiền (lựa chọn lý do, upload hình ảnh/video bằng chứng), hiển thị trạng thái đếm ngược 7 ngày khiếu nại sau khi nhận hàng.
    - Seller UI: Danh sách yêu cầu trả hàng. Thao tác Đồng ý (hiển thị địa chỉ kho nhận hàng trả) / Từ chối (tải ảnh/video đối chứng) / Xác nhận đã nhận hàng hoàn.
  - **Admin Operations Portal (`/admin/shops`, `/admin/disputes`)**:
    - Duyệt Shop Onboarding: Xem hồ sơ KYC, chấp thuận / từ chối gian hàng. Danh sách gian hàng, Khóa / Mở khóa Shop.
    - Admin Dispute Arbitration (Phân xử Tranh chấp): Màn hình xem bằng chứng đối sánh giữa Buyer & Seller, timeline vận chuyển. Nút phán quyết: `Buyer Wins` (hoàn tiền cho Buyer, trừ tiền Shop) hoặc `Seller Wins` (giữ nguyên doanh thu cho Shop).
  - **Admin Executive Dashboard (`/admin/dashboard`)**:
    - Biểu đồ tổng quan GMV toàn sàn, Doanh thu hoa hồng thực nhận (Platform Net Revenue).
    - Thống kê tỷ lệ hoàn hàng, tranh chấp, số người dùng/gian hàng mới. Duyệt lệnh rút tiền Seller Payouts.
- **Dependencies**: T14, T37, T39, T51
- **Estimated Size**: L

---

# PART 5 — SPRINT PLAN (3 THÀNH VIÊN)

> **Quy ước**: Sprint 2 tuần | **Dev A**: Auth/Orders/Payment/Returns | **Dev B**: Product/Shop/Discount/Reports | **Dev C**: Realtime/Infra/Workers/DevOps

## 🏁 SPRINT 1 — Foundation & Auth Core (Tuần 1–2)

| Task | Mô tả ngắn | Assign | Size |
| :--- | :--- | :---: | :---: |
| **T08** | Complete Auth Engine & RBAC Pipeline | Dev A | XL |
| **T09** | Seed System Roles & Default Permissions | Dev A | S |
| **T12** | MinIO File Storage Adapter | Dev C | M |
| **T11** | Shop Registration + KYC Upload | Dev B | M |
| **T15** | Category Tree Management (Admin CRUD) | Dev B | S |

**Goal Sprint 1**: Hoàn thiện toàn bộ luồng Auth gọn trong 1 task kĩ thuật + Shop registration + Category + MinIO.

---

## 🏁 SPRINT 2 — Products & Inventory & Payment Adapters (Tuần 3–4)

| Task | Mô tả ngắn | Assign | Size |
| :--- | :--- | :---: | :---: |
| **T10** | User Profile & Address Book CRUD | Dev B | S |
| **T16** | SPU + SKU CRUD (Seller manage products) | Dev B | L |
| **T17** | Inventory Management + History Ledger | Dev B | M |
| **T18** | Public Product Search & Detail (Full-text) | Dev B | M |
| **T28** | MoMo Payment Adapter + Webhook | Dev C | M |
| **T29** | VietQR Payment Adapter + Bank Webhook | Dev C | M |

**Goal Sprint 2**: Product Catalog sẵn sàng, Search full-text PostgreSQL chạy, Payment Adapters đã kết nối.

---

## 🏁 SPRINT 3 — Cart, Checkout & Payment (Tuần 5–6)

| Task | Mô tả ngắn | Assign | Size |
| :--- | :--- | :---: | :---: |
| **T19** | Cart CRUD (Add/Update/Remove/Get) | Dev A | M |
| **T20** | PriceCalculator Domain Service (3-tier Voucher) | Dev A | L |
| **T21** | Calculate Checkout Draft Query | Dev A | M |
| **T22** | Checkout Command Handler (UoW Transaction) | Dev A | XL |
| **T23** | Order Endpoints (Checkout + History + Cancel) | Dev A | M |
| **T13** | Shop Wallet: Balance + Transaction + Payout | Dev B | M |
| **T14** | Admin Shop Management + Payout Approve | Dev B | M |
| **T24** | Discount/Voucher CRUD (Shop + Platform) | Dev B | M |
| **T25** | Voucher Validation Query | Dev B | S |
| **T33** | GHN Shipping Adapter | Dev C | M |
| **T34** | GHTK + ViettelPost Adapters | Dev C | M |
| **T46** | OrderPlacedConsumer (Email/SMS + Push) | Dev C | M |

**Goal Sprint 3**: Checkout đa shop hoàn thiện, Voucher 3 cấp proration chính xác, GHN/GHTK sẵn sàng.

---

## 🏁 SPRINT 4 — Payment Flow + Direct Settlement + Shipping (Tuần 7–8)

| Task | Mô tả ngắn | Assign | Size |
| :--- | :--- | :---: | :---: |
| **T30** | Payment Initiate & Status (Direct Settlement) | Dev A | M |
| **T31** | SignalR PaymentNotificationHub | Dev C | M |
| **T35** | Seller Shipping (Confirm + Shipment + Label PDF) | Dev C | M |
| **T36** | Shipping Webhook Handler & Delivery Status Sync | Dev C | M |
| **T47** | InventorySyncConsumer (Deduct stock) | Dev C | M |
| **T48** | CODReconciliationConsumer (COD Settlement) | Dev C | M |
| **T26** | Flash Sale Campaign Management | Dev B | M |
| **T27** | Atomic Flash Sale Reserve (PG Condition UPDATE) | Dev B | L |

**Goal Sprint 4**: Direct payment settlement flow & COD đối soát thành công, Flash sale atomic hoạt động.

---

## 🏁 SPRINT 5 — Returns, Livestream & Reviews (Tuần 9–10)

| Task | Mô tả ngắn | Assign | Size |
| :--- | :--- | :---: | :---: |
| **T37** | Return Request Use Case (Buyer hoàn hàng) | Dev A | M |
| **T38** | Seller Return Response (Approve/Reject/Received) | Dev A | M |
| **T39** | Admin Dispute Resolution (BuyerWins/SellerWins) | Dev A | M |
| **T40** | Agora RTC Token Service | Dev C | S |
| **T41** | Livestream Session Use Cases (Start/End + Pin) | Dev C | L |
| **T42** | SignalR LivestreamHub (Chat + Reactions + Pin) | Dev C | L |
| **T43** | SignalR OrderNotificationHub | Dev C | S |
| **T44** | Review CRUD (Create + Edit + Reply + Admin Hide) | Dev B | M |
| **T45** | Rating Calculation Background Worker | Dev C | S |
| **T49** | TimeoutOrderRollbackWorker + SearchVectorConsumer | Dev C | M |

**Goal Sprint 5**: Tranh chấp hoàn hàng & xử lý bồi hoàn, Livestream video & pin SP realtime.

---

## 🏁 SPRINT 6 — Reports, Tests & Production (Tuần 11–12)

| Task | Mô tả ngắn | Assign | Size |
| :--- | :--- | :---: | :---: |
| **T50** | Seller Dashboard & Reports | Dev B | M |
| **T51** | Admin Platform Dashboard (GMV + Disputes) | Dev B | M |
| **T52** | Unit Tests (Domain + Application layer) | All | L |
| **T53** | Integration Tests (Auth + Checkout + Webhooks) | All | L |
| **T54** | End-to-End Tests (Full purchase + Direct settlement cycle) | All | M |
| **T55** | Production Docker Stack (Nginx SSL + Health) | Dev C | M |

**Goal Sprint 6**: Dashboards hoàn tất, 100% test suite pass, deploy Production Docker Nginx.

---

## 🏁 SPRINT 7 — Frontend Web Applications (Tuần 13–14)

| Task | Mô tả ngắn | Assign | Size |
| :--- | :--- | :---: | :---: |
| **T56** | Frontend Boilerplate, Auth Hub & User Profile | Dev FE | L |
| **T57** | Buyer Marketplace & Product Search / Details | Dev FE | XL |
| **T58** | Cart, Multi-shop Checkout & Payment QR Flow | Dev FE | XL |
| **T59** | Seller Center Portal (Products, Orders, Shipping) | Dev FE | XL |
| **T60** | Livestream Commerce Portal (Agora RTC + SignalR) | Dev FE | XL |
| **T61** | Returns, Admin Operations & Executive Dashboard | Dev FE | L |

**Goal Sprint 7**: Hoàn thiện toàn bộ giao diện Web App cho Buyer, Seller và Admin kết nối đồng bộ 100% REST API và Realtime SignalR/Agora SDK.

---

# PART 6 — PHÂN CÔNG THEO THÀNH VIÊN

## 👨‍💻 Dev A — Auth / Orders / Payment / Returns

| Task | Tên Task kĩ thuật | Size |
| :--- | :--- | :---: |
| **T08** | Complete Auth Engine & RBAC Pipeline | XL |
| **T09** | Seed System Roles & Default Permissions | S |
| **T19** | Cart Multi-shop Management Use Cases | M |
| **T20** | PriceCalculator Domain Service (3-tier Voucher Proration) | L |
| **T21** | Calculate Checkout Draft Query | M |
| **T22** | Checkout Command Handler (Atomic UoW 8-Step Transaction) | XL |
| **T23** | Order Controller Endpoints & Seller Order Operations | M |
| **T30** | Payment Initiator & Direct Settlement Flow | M |
| **T37** | Return Request Lifecycle (Buyer Claim ≤ 7 Days) | M |
| **T38** | Seller Return Response & Inspection Confirmation | M |
| **T39** | Admin Dispute Arbitration & Refund Resolution | M |
| **T52-T54** | Shared Unit, Integration & E2E Tests | L |

## 👨‍💻 Dev B — Products / Shop / Discount / Flash Sale / Reports

| Task | Tên Task kĩ thuật | Size |
| :--- | :--- | :---: |
| **T10** | User Profile & Address Book Management | S |
| **T11** | Shop Registration, KYC Upload & Warehouse Management | M |
| **T13** | Shop Wallet, Financial Ledger & Payout Request | M |
| **T14** | Admin Shop Onboarding, Ban Policy & Payout Approval | M |
| **T15** | Multi-level Category Tree Management | S |
| **T16** | Product SPU & SKU Multi-variant Management | L |
| **T17** | Inventory 2-State Management & Ledger Logging | M |
| **T18** | Public Product Search Engine (Full-text + Trigram) | M |
| **T24** | Discount & Voucher Lifecycle Management | M |
| **T25** | Voucher Validation & Proration Calculation Query | S |
| **T26** | Flash Sale Campaign & Registration Management | M |
| **T27** | Atomic Flash Sale Reserve (PG Condition UPDATE) | L |
| **T44** | Review & Rating Management | M |
| **T50** | Seller Performance Dashboard & Wallet Analytics | M |
| **T51** | Admin Platform Executive Dashboard & Financial Reports | M |

## 👨‍💻 Dev C — Realtime / Infra / Workers / DevOps

| Task | Tên Task kĩ thuật | Size |
| :--- | :--- | :---: |
| **T12** | MinIO File Storage Adapter & Multi-bucket Upload | M |
| **T28** | MoMo Payment Gateway Adapter & IPN Handler | M |
| **T29** | VietQR Payment Gateway Adapter & Webhook Handler | M |
| **T31** | SignalR PaymentNotificationHub Integration | M |
| **T33** | GHN Shipping Adapter & Webhook Integrator | M |
| **T34** | GHTK & ViettelPost Shipping Adapters | M |
| **T35** | Seller Shipping Fulfillment & Label PDF Generator | M |
| **T36** | Shipping Webhook Handler & Delivery Status Sync | M |
| **T40** | Agora RTC Token Generation Service | S |
| **T41** | Livestream Session Lifecycle & Product Pinning | L |
| **T42** | SignalR LivestreamHub (Chat, Reaction, Pin, Viewers) | L |
| **T43** | SignalR OrderNotificationHub Integration | S |
| **T45** | Rating Calculation Background Worker | S |
| **T46** | OrderPlacedConsumer (Email/SMS + Push Noti) | M |
| **T47** | InventorySyncConsumer (Deduct Stock After Paid) | M |
| **T48** | CODReconciliationConsumer (COD Order Settlement & Wallet Credit) | M |
| **T49** | TimeoutOrderRollbackWorker & ProductSearchVectorConsumer | M |
| **T55** | Production Docker Stack (Nginx SSL + Health Checks) | M |

## 🎨 Dev FE — Frontend Portal & Web Applications

| Task | Tên Task kĩ thuật | Size |
| :--- | :--- | :---: |
| **T56** | Setup Frontend Boilerplate, Auth Hub & User Profile | L |
| **T57** | Buyer Marketplace & Product Search / Details Portal | XL |
| **T58** | Cart, Multi-shop Checkout & Payment Flow | XL |
| **T59** | Seller Center Portal (Shop, Products, Orders & Shipping) | XL |
| **T60** | Livestream Commerce Portal (Agora RTC + SignalR Realtime) | XL |
| **T61** | Returns, Admin Operations & Executive Dashboard | L |

