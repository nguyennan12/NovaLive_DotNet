# 🛒 KIẾN TRÚC HỆ THỐNG NOVALIVE ECOMMERCE & LIVESTREAM PLATFORM

> **Mô hình**: Multi-vendor Marketplace + Livestream Commerce  
> **Tech Stack**: .NET 10, ASP.NET Core Web API, PostgreSQL 17, Redis 7, MassTransit + RabbitMQ, MinIO, SignalR, Agora RTC  
> **Kiến trúc**: Clean Architecture (`Domain` $\rightarrow$ `Application` $\rightarrow$ `Infrastructure` $\rightarrow$ `Api`) + CQRS (MediatR) + Event-Driven Architecture (Outbox Pattern)  
> **Auth**: JWT Bearer (HMAC-SHA256 / HS256) + Redis JTI Blacklist + Refresh Token Rotation  
> **Triển khai**: Docker & Docker Compose (Hot-reload Dev / Production Multi-stage) + Nginx Reverse Proxy SSL  

---

## 1. 🗺️ SƠ ĐỒ TỔNG THỂ KIẾN TRÚC (UNIFIED SERVICE GATEWAY)

```text
 ┌─────────────────────────────────────────────────────────────────────────────────────────┐
 │                            CLOUD INFRASTRUCTURE (Docker Host)                           │
 │                                                                                         │
 │  ┌───────────────────────────────────────────────────────────────────────────────────┐  │
 │  │                       Nginx (Reverse Proxy & Load Balancer)                       │  │
 │  │      /api/* ──► NovaLive.Api (:5001)       |      /hubs/* ──► NovaLive.Api (:5001) │  │
 │  │      https://cdn.novalive.vn ──► MinIO S3 Console/Storage                           │  │
 │  └─────────────────────────────────────────┬─────────────────────────────────────────┘  │
 │                                            │                                            │
 │  ┌─────────────────────────────────────────▼─────────────────────────────────────────┐  │
 │  │                         NovaLive.Api (.NET 10 Web API Host)                       │  │
 │  │ • REST API Endpoints: Auth, Users, Shops, Products, Cart, Orders, Payments, Shipping  │  │
 │  │ • SignalR WebSockets Hubs: LivestreamHub, OrderHub, PaymentHub                       │  │
 │  │ • Outbox Background Worker, Health Checks (/health), Scalar API Docs (/scalar/v1)   │  │
 │  └──────────────┬────────────────────────────────────────────────────────┬───────────┘  │
 │                 │                                                        │              │
 │  ┌──────────────┴────────────────────────────────────────────────────────┴───────────┐  │
 │  │                               INFRASTRUCTURE SERVICES                             │  │
 │  │  ┌───────┐  ┌───────┐  ┌───────┐  ┌───────┐  ┌──────────────┐                    │  │
 │  │  │ PG 17 │  │ Redis │  │  RMQ  │  │ MinIO │  │  Agora RTC   │                    │  │
 │  │  │ (ACID/│  │(Cache/│  │(Queue/│  │(Media)│  │ (Livestream  │                    │  │
 │  │  │ Search)│ │Backpl)│  │Outbox)│  │ S3)   │  │  P2P/Cloud)  │                    │  │
 │  │  └───────┘  └───────┘  └───────┘  └───────┘  └──────────────┘                    │  │
 │  └───────────────────────────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────────────────────────┘
              ▲                                           ▲
              │ HTTPS REST + JWT (HS256)                  │ WebSocket (WSS / SignalR)
 ┌────────────┴───────────────────────────────────────────┴───────────────────────────────┐
 │                                CLIENT APPLICATIONS                                     │
 │           Mobile App (Buyer/Seller)   |   Web Marketplace   |   Admin Backoffice        │
 └─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. 🏗️ CLEAN ARCHITECTURE LAYERS

```text
┌───────────────────────────────────────────────────────────────────────────────┐
│                      PRESENTATION LAYER (NovaLive.Api)                        │
│   • Controllers: Auth, Users, Shops, Products, Cart, Orders, Payments...       │
│   • SignalR Hubs: LivestreamHub (/hubs/livestream), OrderHub, PaymentHub       │
│   • Middlewares: GlobalExceptionHandler, JwtRoleContext, RateLimiting         │
│   • API Docs: Scalar OpenAPI documentation (/scalar/v1)                       │
└───────────────────────────────────────┬───────────────────────────────────────┘
                                        │ Dispatch Queries / Commands via MediatR
┌───────────────────────────────────────▼───────────────────────────────────────┐
│                       APPLICATION LAYER (NovaLive.Application)                │
│   • Abstractions (Ports): IAppDbContext, ICacheService, IMessageBus,           │
│     IProductQueryService, IFileStorageService, IPaymentGateway, IShippingProvider│
│   • Common Behaviors: ValidationBehavior, AuthorizationBehavior,               │
│     LoggingBehavior, PerformanceBehavior, TransactionBehavior, IdempotencyBehavior│
│   • Messaging Contract Interfaces: ICommand, ICommandHandler, IQuery, IQueryHandler│
└───────────────────────────────────────┬───────────────────────────────────────┘
                                        │ Uses Domain Models & Enforces Rules
┌───────────────────────────────────────▼───────────────────────────────────────┐
│                          DOMAIN LAYER (NovaLive.Domain)                       │
│   • Entities: User, Shop, ShopWallet, Spu, Sku, Cart, ParentOrder, SubOrder    │
│   • Value Objects: Money, Address, OtpCode                                    │
│   • Common Abstractions: AggregateRoot, Entity, Result<T>, Error, Enums       │
│   • Interfaces & Interceptors: IAuditableEntity, ISoftDeletable, IDomainEvent │
└───────────────────────────────────────────────────────────────────────────────┘
                                        ▲ Implements Interfaces
┌───────────────────────────────────────┴───────────────────────────────────────┐
│                    INFRASTRUCTURE LAYER (NovaLive.Infrastructure)             │
│   • Persistence: EF Core 10 + Npgsql ──► PostgreSQL 17 (Migrations & Configs) │
│   • Interceptors: AuditableEntityInterceptor, SoftDeleteInterceptor           │
│   • Cache & Idempotency: RedisCacheService, RedisIdempotencyService           │
│   • Messaging & Outbox: MassTransitBusAdapter ──► RabbitMQ + OutboxWorker     │
│   • Search Engine: PostgresProductQueryService (Full-Text Search & pg_trgm)   │
│   • External Adapters: Minio Storage, MoMo/VietQR Payment, GHN Shipping       │
└───────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. 📁 CẤU TRÚC SOLUTION C# (.NET 10)

Cấu trúc thư mục thực tế của giải pháp `NovaLive.sln` được tổ chức chuẩn Clean Architecture:

```text
NovaLive.sln
│
├── 🌐 src/
│   ├── NovaLive.Api/                        (ASP.NET Core Web API Host & SignalR Gateway)
│   │   ├── Auth/                            (CurrentShop, CurrentUser context accessors)
│   │   ├── Controllers/                     (REST Controllers: Health, Auth, Products, Orders...)
│   │   ├── Extensions/                      (DependencyInjection, OpenApi/Scalar)
│   │   ├── Hubs/                            (SignalR Hubs: LivestreamHub, OrderHub, PaymentHub)
│   │   ├── Middleware/                      (GlobalExceptionHandler, JwtRoleContext)
│   │   ├── Program.cs                       (Startup & Service Registration)
│   │   └── appsettings.json
│   │
│   ├── NovaLive.Application/                (CQRS Use Cases, MediatR Pipeline Behaviors, Abstractions)
│   │   ├── Abstractions/
│   │   │   ├── Auth/                        (ICurrentShop, ICurrentUser, IRequirePermission)
│   │   │   ├── Cache/                       (ICacheService)
│   │   │   ├── Clock/                       (IDateTimeProvider)
│   │   │   ├── Idempotency/                 (IIdempotencyService)
│   │   │   ├── Messaging/                   (IMessageBus)
│   │   │   ├── Persistence/                 (IAppDbContext, IMigrationService, IUnitOfWork)
│   │   │   ├── Search/                      (IProductQueryService)
│   │   │   ├── Storage/                     (IFileStorageService)
│   │   │   └── ThirdParty/                  (IAgoraTokenService, IPaymentGateway, IShippingProvider)
│   │   ├── Common/
│   │   │   ├── Behaviors/                   (AuthorizationBehavior, IdempotencyBehavior, LoggingBehavior, PerformanceBehavior, TransactionBehavior, ValidationBehavior)
│   │   │   └── Messaging/                   (ICommand, ICommandHandler, IQuery, IQueryHandler, ITransactionalCommand, IIdempotentCommand)
│   │   ├── System/                          (GetSystemStatusQuery & Handler)
│   │   └── DependencyInjection.cs
│   │
│   ├── NovaLive.Contracts/                  (Public DTO Requests & Responses)
│   │   ├── Common/                          (ApiError, ApiResponse, PagedResult)
│   │   └── V1/                              (Auth, Users, Shops, Categories, Products, Carts, Discounts, FlashSales, Orders, Payments, Shipping, Returns, Reviews, Livestreams, Dashboards)
│   │
│   ├── NovaLive.Domain/                     (Core Domain Entities, Enums, Value Objects, Domain Events)
│   │   ├── Carts/                           (Cart, CartItem)
│   │   ├── Common/                          (AggregateRoot, Entity, ValueObject, Result, Error, Enums)
│   │   ├── Discounts/                       (Discount, DiscountUsage)
│   │   ├── FlashSales/                      (FlashSaleCampaign, FlashSaleItem)
│   │   ├── Inventory/                       (Inventory, InventoryHistory)
│   │   ├── Livestreams/                     (LivestreamSession, LivestreamProduct, LivestreamComment)
│   │   ├── Notifications/                   (Notification)
│   │   ├── Orders/                          (ParentOrder, SubOrder, OrderItem, OrderStatusHistory, OrderReturn, OrderReturnItem)
│   │   ├── Payments/                        (Payment, SellerPayout)
│   │   ├── Products/                        (Category, Spu, Sku, SkuImage, ProductAttribute)
│   │   ├── Rbac/                            (Permission, Resource, Role, RolePermission, UserRole)
│   │   ├── Reviews/                         (Review, ReviewImage)
│   │   ├── Shipping/                        (ShippingOrder)
│   │   ├── Shops/                           (Shop, ShopVerification, ShopAddress, ShopFollower, ShopWallet, ShopWalletTransaction)
│   │   ├── System/                          (AuditLog, OutboxMessage)
│   │   └── Users/                           (User, UserAddress, UserOtp, RefreshToken)
│   │
│   └── NovaLive.Infrastructure/             (Implementations: EF Core, Redis, MassTransit, MinIO)
│       ├── Cache/                           (RedisCacheService)
│       ├── Clock/                           (DateTimeProvider)
│       ├── Idempotency/                     (RedisIdempotencyService)
│       ├── Messaging/                       (MassTransitMessageBus)
│       ├── Persistence/
│       │   ├── AppDbContext.cs
│       │   ├── Configurations/              (EF Core Fluent Configurations per Entity)
│       │   ├── Interceptors/                (AuditableEntityInterceptor, DomainEventsDispatcherInterceptor, SoftDeleteInterceptor)
│       │   ├── Migrations/                  (EF Core Database Migrations)
│       │   └── Seeding/                     (SystemDataSeeder)
│       ├── Search/                          (PostgresProductQueryService)
│       ├── System/                          (OutboxBackgroundWorker)
│       └── DependencyInjection.cs
│
├── 🧪 tests/
│   ├── NovaLive.Api.Tests/
│   ├── NovaLive.Application.Tests/
│   ├── NovaLive.Domain.Tests/
│   └── NovaLive.Infrastructure.Tests/
│
├── 🐳 docker/
│   └── dev/                                 (Dockerfile, docker-compose.yml)
│
└── 📄 docs/                                 (Architecture, SRS, Task Breakdown, Endpoints...)
```

---

## 4. ⚡ CÁC LUỒNG XỬ LÝ NGHIỆP VỤ THEN CHỐT

### 4.1 Luồng Mua hàng Đa Shop: Giỏ hàng $\rightarrow$ Tính nháp $\rightarrow$ Đặt đơn $\rightarrow$ Thanh toán trực tiếp cho Shop

```text
[Buyer: Màn hình Giỏ hàng]
    │
    ├─ 1. Chọn sản phẩm: Tick chọn 3 món (Shop A 2 món, Shop B 1 món)
    │
    ├─ 2. Tính toán nháp Realtime:
    │       POST /api/v1/orders/calculate-checkout { cartItemIds: [1,2,3], addressId, vouchers: [...] }
    │       → Server trả về bảng chi tiết: Tiền hàng A, Tiền hàng B, Phí ship A/B, Giảm giá từng shop, Giảm giá sàn
    │
    ├─ 3. Bấm "Đặt Hàng":
    │       POST /api/v1/orders/checkout
    │       │
    │       ▼
    │       SubmitCheckoutCommandHandler (UnitOfWork Transaction):
    │       ├── 1. Khóa giữ chỗ tồn kho (Inventories.reserved_qty += qty)
    │       ├── 2. Tạo Parent Order (quản lý grand_total toàn giỏ)
    │       ├── 3. Tạo 2 Sub-Orders (SubOrder A cho Shop A, SubOrder B cho Shop B)
    │       ├── 4. Tạo OrderItems kèm snapshot: sku_snapshot_json, unit_price, discount_amount
    │       ├── 5. Tạo Payment record (Pending) cho ParentOrder
    │       ├── 6. Xóa 3 món đã chọn khỏi giỏ hàng (giữ lại các món chưa chọn)
    │       └── 7. Ghi OutboxMessage(OrderPlacedEvent) trong cùng DB transaction
    │
    ├─ 4. Thanh toán & Ghi nhận Doanh thu:
    │   ├─ MoMo / VietQR: Buyer quét mã QR ──► Webhook IPN ──► Payment.Status = Success
    │   │   └── Cộng tiền trực tiếp vào ShopWallets.balance của Shop A & Shop B ──► SubOrders = Confirmed
    │   └─ COD: Đơn tạo thành công ──► SubOrders = Confirmed (Seller đóng gói ngay, đối soát tiền sau khi giao)
    │
    ├─ 5. Giao hàng & Hoàn tất:
    │       Seller đóng gói ──► Gọi ĐVVC lấy hàng (Shipping) ──► ĐVVC giao xong (Delivered)
    │       ──► SubOrder hoàn thành (Completed) & Buyer có thể viết Đánh giá (Review)
```

### 4.2 Luồng Livestream: Agora RTC $\rightarrow$ Pin SP $\rightarrow$ Mua Ngay (Instant Buy)

```text
[Seller Bắt đầu Live]
    │
    ├── 1. POST /api/v1/seller/livestreams/start ──► Server sinh Agora RTC Token (Role: Publisher) ──► Phát live
    │
[Viewer Vào Xem Live]
    │
    ├── 2. GET /api/v1/livestreams/{id} ──► Server sinh Agora RTC Token (Role: Subscriber) ──► Xem live
    │       Kết nối WebSocket LivestreamHub (/hubs/livestream)
    │
[Seller Ghim Sản Phẩm & Bật Giá Flash]
    │
    ├── 3. POST /api/v1/seller/livestreams/{id}/pin-product { skuId, flashPrice: 99000, duration: 120s }
    │       ──► SignalR Broadcast "ProductPinned" đến toàn bộ viewer trong room
    │
[Viewer Bấm "Mua Ngay"]
    │
    └── 4. Client gọi QuickBuyCheckoutRequest ──► Chuyển thẳng đến tạo đơn hàng Instant Checkout
```

---

## 5. 🐳 DEPLOYMENT & PRODUCTION DOCKER COMPOSE

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:17-alpine
    container_name: novalive_postgres_prod
    environment:
      POSTGRES_DB: ${POSTGRES_DB:-novalive_db}
      POSTGRES_USER: ${POSTGRES_USER:-nova_user}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - pgdata:/var/lib/postgresql/data
    restart: always

  redis:
    image: redis:7-alpine
    container_name: novalive_redis_prod
    command: redis-server --requirepass ${REDIS_PASSWORD}
    volumes:
      - redisdata:/data
    restart: always

  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    container_name: novalive_rabbitmq_prod
    environment:
      RABBITMQ_DEFAULT_USER: ${RABBITMQ_USER:-guest}
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
    volumes:
      - rmqdata:/var/lib/rabbitmq
    restart: always

  minio:
    image: cgr.dev/chainguard/minio:latest
    container_name: novalive_minio_prod
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: ${MINIO_ROOT_USER}
      MINIO_ROOT_PASSWORD: ${MINIO_ROOT_PASSWORD}
    volumes:
      - miniodata:/data
    restart: always

  novalive-api:
    build:
      context: .
      dockerfile: docker/dev/Dockerfile
    container_name: novalive_api_prod
    ports:
      - "5001:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      Redis__ConnectionString: redis:6379,password=${REDIS_PASSWORD}
      RabbitMQ__Host: rabbitmq
      RabbitMQ__Username: ${RABBITMQ_USER}
      RabbitMQ__Password: ${RABBITMQ_PASSWORD}
      Jwt__Secret: ${JWT_SECRET}
    depends_on:
      - postgres
      - redis
      - rabbitmq
      - minio
    restart: always

volumes:
  pgdata:
  redisdata:
  rmqdata:
  miniodata:
```