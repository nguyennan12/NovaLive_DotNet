# 🛒 KIẾN TRÚC HỆ THỐNG NOVALIVE ECOMMERCE & LIVESTREAM PLATFORM

> **Mô hình**: Multi-vendor Marketplace + Livestream Commerce  
> **Tech Stack**: .NET 10, ASP.NET Core Web API, PostgreSQL 17, Redis 7, MassTransit + RabbitMQ, MinIO, SignalR, Agora RTC
> **Kiến trúc**: Clean Architecture (Domain $\rightarrow$ Application $\rightarrow$ Infrastructure $\rightarrow$ Api) + CQRS (MediatR) + Event-Driven Architecture (Outbox Pattern)  
> **Auth**: JWT Bearer (HMAC-SHA256 / HS256) + Redis JTI Blacklist + Refresh Token Rotation  
> **Triển khai**: Docker & Docker Compose + Nginx Reverse Proxy SSL  

---

## 1. 🗺️ SƠ ĐỒ TỔNG THỂ KIẾN TRÚC (DUAL-HOST GATEWAY)

```
 ┌─────────────────────────────────────────────────────────────────────────────────────────┐
 │                            CLOUD INFRASTRUCTURE (Docker Host)                           │
 │                                                                                         │
 │  ┌───────────────────────────────────────────────────────────────────────────────────┐  │
 │  │                       Nginx (Reverse Proxy & Load Balancer)                       │  │
 │  │      /api/* ──► CoreApi (:5000)      |      /hubs/* ──► RealtimeApi (:5001)       │  │
 │  │      https://cdn.novalive.vn ──► MinIO S3                                         │  │
 │  └──────────────────────────┬────────────────────────────┬───────────────────────────┘  │
 │                             │                            │                              │
 │  ┌──────────────────────────▼──────────────┐  ┌──────────▼───────────────────────────┐  │
 │  │    NovaLive.CoreApi (.NET 10)           │  │   NovaLive.RealtimeApi (.NET 10)     │  │
 │  │ • REST API: Auth, Products, Orders      │  │ • SignalR Hubs: Livestream, Chat, Pin│  │
 │  │ • Payments, Shipping, Admin, Reports    │  │ • Redis Backplane Pub/Sub Sync       │  │
 │  │ • MassTransit Consumers & Workers       │  │ • Realtime Notification Push         │  │
 │  └──────────────┬──────────────────────────┘  └──────────┬───────────────────────────┘  │
 │                 │                                        │                              │
 │  ┌──────────────┴────────────────────────────────────────┴───────────────────────────┐  │
 │  │                               INFRASTRUCTURE SERVICES                             │  │
 │  │  ┌───────┐  ┌───────┐  ┌───────┐  ┌───────┐  ┌──────────────┐                    │  │
 │  │  │ PG 17 │  │ Redis │  │  RMQ  │  │ MinIO │  │  Agora RTC   │                    │  │
 │  │  │ (ACID/│  │(Cache/│  │(Queue)│  │(Media)│  │ (Livestream  │                    │  │
 │  │  │ Search)│ │Backpl)│  │       │  │       │  │  P2P/Cloud)  │                    │  │
 │  │  └───────┘  └───────┘  └───────┘  └───────┘  └──────────────┘                    │  │
 │  └───────────────────────────────────────────────────────────────────────────────────┘  │
 └─────────────────────────────────────────────────────────────────────────────────────────┘
              ▲                                           ▲
              │ HTTPS REST + JWT (HS256)                  │ WebSocket (WSS / SignalR)
 ┌────────────┴───────────────────────────────────────────┴───────────────────────────────┐
 │                           CLIENT APPS (Flutter & React 19)                              │
 │           Mobile App (Buyer/Seller)   |   Web Marketplace   |   Admin Portal            │
 └─────────────────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. 🏗️ CLEAN ARCHITECTURE LAYERS

```
┌───────────────────────────────────────────────────────────────────────────────┐
│                      PRESENTATION LAYER (Dual-Host Gateway)                   │
│   • NovaLive.CoreApi (Port 5000): REST Controllers (DTOs ──► MediatR Commands)│
│     Middlewares: ExceptionHandling, JwtRoleContext, RateLimiting              │
│   • NovaLive.RealtimeApi (Port 5001): SignalR Hubs (LivestreamHub, OrderHub)  │
│     Redis Backplane: StackExchangeRedis Pub/Sub Sync đa container             │
└───────────────────────────────────────┬───────────────────────────────────────┘
                                        │ Gọi Mediator / Shared Application
┌───────────────────────────────────────▼───────────────────────────────────────┐
│                       APPLICATION LAYER (Use Cases & CQRS)                    │
│   • Commands & Queries: Mỗi use case 1 folder (Command + Handler + Validator) │
│   • Pipeline Behaviors: Validation (FluentValidation), Authorization, Logging,│
│     UnitOfWork Transaction Behavior                                           │
│   • Ports (Interfaces): IProductRepository, IParentOrderRepository,           │
│     ISubOrderRepository, ICacheService, IMessageBus, IPaymentGateway          │
└───────────────────────────────────────┬───────────────────────────────────────┘
                                        │ Đọc Domain Rules & Gọi Domain Services
┌───────────────────────────────────────▼───────────────────────────────────────┐
│                          DOMAIN LAYER (Enterprise Core)                       │
│   • Entities: User, Shop, ShopWallet, Product, Sku, ParentOrder, SubOrder     │
│   • Value Objects: Money, Address, OtpCode                                    │
│   • Domain Services: PriceCalculator, InventoryChecker, EscrowCalculator      │
│   • Domain Events: OrderPlacedEvent, PaymentReceivedEvent, ShipmentDelivered  │
│   • Enums & Custom Result<T> / Error Pattern (Không phụ thuộc thư viện ngoài) │
└───────────────────────────────────────────────────────────────────────────────┘
                                        ▲
┌───────────────────────────────────────┴───────────────────────────────────────┐
│                    INFRASTRUCTURE LAYER (Adapters & External Tech)            │
│   • Persistence: EF Core 10 + Npgsql ──► PostgreSQL 17 (Migrations & Configs) │
│   • Caching & Backplane: StackExchange.Redis ──► Redis 7 (Cache, JTI, PubSub) │
│   • Messaging: MassTransit ──► RabbitMQ (Outbox Pattern + Background Workers) │
│   • Search: PostgreSQL Full-Text Search + pg_trgm indexes                     │
│   • Storage: Minio C# SDK ──► MinIO S3                                        │
│   • External APIs: Refit Clients ──► GHN, GHTK, ViettelPost, MoMo, VietQR, Agora │
└───────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. 📁 CẤU TRÚC SOLUTION C# (.NET 10)

```
NovaLive.sln
│
├── 🌐 NovaLive.CoreApi/                    (REST API Entry Point - Port 5000)
│   ├── Controllers/
│   │   ├── AuthController.cs               ← Register, VerifyOtp, Login, Refresh, Logout
│   │   ├── UsersController.cs              ← Profile, Addresses
│   │   ├── ShopsController.cs              ← Register shop, KYC, Shop addresses
│   │   ├── ProductsController.cs           ← Public product search & Seller SPU/SKU CRUD
│   │   ├── CartController.cs               ← Add/Update/Remove, Clear selected items
│   │   ├── OrdersController.cs             ← CalculateCheckout, Submit Checkout, Cancel Order
│   │   ├── PaymentsController.cs           ← Create payment QR, Webhooks (MoMo/VietQR)
│   │   ├── ShippingController.cs           ← Create pickup, Webhook tracking GHN/GHTK
│   │   ├── ReturnsController.cs            ← Return request, Seller review, Admin dispute
│   │   ├── FlashSaleController.cs          ← Campaign list, Seller register, Reserve slot
│   │   ├── LivestreamsController.cs        ← Start/End stream, Agora RTC token
│   │   ├── ReviewsController.cs            ← Create review, Edit review (30d), Seller reply
│   │   ├── AdminController.cs              ← Approve shop, Settle disputes, Approve payouts
│   │   └── ReportsController.cs            ← Platform analytics & Seller dashboard
│   ├── Middlewares/
│   │   ├── ExceptionMiddleware.cs          ← Bắt lỗi toàn cục trả về ProblemDetails JSON
│   │   └── JwtRoleContextMiddleware.cs     ← Kiểm tra Redis JTI blacklist & bind UserContext
│   └── Program.cs                          ← DI, Database Context, MediatR, MassTransit
│
├── ⚡ NovaLive.RealtimeApi/                (WebSocket & SignalR Gateway - Port 5001)
│   ├── Hubs/
│   │   ├── LivestreamHub.cs                ← WebSockets: Pin SP, Flash price, Chat, Reactions
│   │   ├── OrderNotificationHub.cs         ← Push thông báo đơn mới realtime cho Seller
│   │   └── PaymentNotificationHub.cs       ← Push kết quả thanh toán QR realtime cho Buyer
│   ├── Middlewares/
│   │   └── WebSocketAuthMiddleware.cs      ← Xác thực JWT token từ Query param (?access_token=)
│   └── Program.cs                          ← SignalR + Redis Backplane (AddStackExchangeRedis)
│
├── 📋 NovaLive.Application/                (CQRS Use Cases, MediatR, Business Ports)
│   ├── Abstractions/
│   │   ├── Persistence/ (IUserRepository, IParentOrderRepository, ISubOrderRepository, IShopWalletRepository, IUnitOfWork...)
│   │   ├── Cache/ (ICacheService)
│   │   ├── Messaging/ (IMessageBus)
│   │   ├── Search/ (IProductQueryService - PostgreSQL full-text/trigram)
│   │   ├── Storage/ (IFileStorageService)
│   │   └── ThirdParty/ (IPaymentGateway, IShippingProvider, IAgoraTokenService)
│   ├── UseCases/
│   │   ├── Auth/Commands/ (Register, Login, RefreshToken, Logout, VerifyOtp)
│   │   ├── Shops/Commands/ (RegisterShop, UpdateKyc, AddWarehouseAddress)
│   │   ├── Products/Commands/ (CreateProduct, UpdateProduct, RebuildProductSearchVector)
│   │   ├── Cart/Commands/ (AddToCart, UpdateCartItem, RemoveCartItem)
│   │   ├── Orders/
│   │   │   ├── Queries/ (CalculateCheckoutDraftQuery, GetOrderDetailQuery)
│   │   │   └── Commands/ (CheckoutCommand, CancelOrderCommand)
│   │   ├── Payments/Commands/ (InitiatePayment, HandleMoMoWebhook, HandleVietQRWebhook)
│   │   ├── Shipping/Commands/ (CreateShipmentOrder, HandleShippingWebhook)
│   │   ├── Returns/Commands/ (RequestReturn, SellerApproveReturn, AdminResolveDispute)
│   │   ├── FlashSale/Commands/ (CreateCampaign, RegisterSku, ReserveFlashSaleSlot)
│   │   ├── Livestreams/Commands/ (StartLiveSession, EndLiveSession, PinProductRealtime)
│   │   └── Reviews/Commands/ (CreateReview, UpdateReview, ReplyReview)
│   └── Common/Behaviors/ (ValidationBehavior, LoggingBehavior, TransactionBehavior)
│
├── 🏛️ NovaLive.Domain/                     (Core Entities, Value Objects, Domain Events)
│   ├── Entities/
│   │   ├── Users/ (User, UserAddress, UserOtp, RefreshToken)
│   │   ├── Shops/ (Shop, ShopVerification, ShopAddress, ShopWallet, ShopWalletTransaction)
│   │   ├── Products/ (Category, Spu, Sku, SkuImage, ProductAttribute)
│   │   ├── Inventory/ (Inventory, InventoryHistory)
│   │   ├── Cart/ (Cart, CartItem)
│   │   ├── Orders/ (ParentOrder, SubOrder, OrderItem, OrderStatusHistory, OrderReturn, OrderReturnItem)
│   │   ├── Discounts/ (Discount, DiscountUsage)
│   │   ├── FlashSale/ (FlashSaleCampaign, FlashSaleItem)
│   │   ├── Payments/ (Payment, PaymentEscrow, SellerPayout)
│   │   ├── Shipping/ (ShippingOrder)
│   │   ├── Livestreams/ (LivestreamSession, LivestreamProduct, LivestreamComment)
│   │   ├── Reviews/ (Review, ReviewImage)
│   │   ├── Notifications/ (Notification)
│   │   └── System/ (AuditLog, OutboxMessage)
│   ├── ValueObjects/ (Money, Address, OtpCode)
│   ├── DomainServices/ (PriceCalculator, InventoryChecker, EscrowProrationService)
│   └── Events/ (OrderPlacedEvent, PaymentSuccessEvent, ShipmentDeliveredEvent, EscrowReleasedEvent)
│
├── 🔧 NovaLive.Infrastructure/             (Implementations: EF Core, Redis, MassTransit, MinIO)
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Configurations/ (EF Core Fluent API 1 file / 1 Entity)
│   │   └── Repositories/ (Implement các Repository Interfaces từ Application)
│   ├── Cache/ (RedisCacheService)
│   ├── Search/ (PostgresProductQueryService)
│   ├── Storage/ (MinioFileStorageService)
│   ├── Messaging/
│   │   ├── MassTransitBusAdapter.cs
│   │   └── Consumers/
│   │       ├── OrderPlacedConsumer.cs          ← Gửi Email/SMS xác nhận + Push Notification
│   │       ├── ProductSearchVectorConsumer.cs  ← Cập nhật search_vector PostgreSQL
│   │       ├── InventorySyncConsumer.cs        ← Trừ kho thực tế sau khi thanh toán
│   │       ├── EscrowReleaseConsumer.cs        ← Tự động cộng tiền ví Shop khi hết T+7
│   │       ├── TimeoutOrderRollbackWorker.cs   ← Hủy đơn quá hạn 15p & Hoàn trả tồn kho (Reserved Qty)
│   │       └── RatingCalculationWorker.cs      ← Tính lại điểm sao trung bình của Shop
│   ├── ExternalServices/
│   │   ├── MoMoPaymentAdapter.cs
│   │   ├── VietQRPaymentAdapter.cs
│   │   ├── GhnShippingAdapter.cs
│   │   ├── GhtkShippingAdapter.cs
│   │   ├── ViettelPostShippingAdapter.cs
│   │   └── AgoraTokenService.cs
│   └── DependencyInjection.cs
│
└── 📦 NovaLive.Contracts/                  (Public API Request/Response DTOs)
    └── V1/ (Auth, Users, Shops, Products, Cart, Orders, Payments, Shipping, Live, Reviews)
```

---

## 4. ⚡ CÁC LUỒNG XỬ LÝ NGHIỆP VỤ THEN CHỐT

### 4.1 Luồng Mua hàng Đa Shop: Giỏ hàng $\rightarrow$ Tính nháp $\rightarrow$ Đặt đơn $\rightarrow$ Ký quỹ

```
[Buyer: Màn hình Giỏ hàng]
    │
    ├─ 1. Chọn sản phẩm: Tick chọn 3 món (Shop A 2 món, Shop B 1 món)
    │
    ├─ 2. Tính toán nháp Realtime:
    │       POST /orders/calculate-checkout { cartItemIds: [1,2,3], addressId, vouchers: [...] }
    │       → Server trả về bảng chi tiết: Tiền hàng A, Tiền hàng B, Phí ship A/B, Giảm giá từng shop, Giảm giá sàn
    │
    ├─ 3. Bấm "Đặt Hàng":
    │       POST /orders/checkout
    │       │
    │       ▼
    │       CheckoutCommandHandler (UnitOfWork Transaction):
    │       ├── 1. Khóa giữ chỗ tồn kho (Inventories.reserved_qty += qty)
    │       ├── 2. Tạo Parent Order (quản lý grand_total toàn giỏ)
    │       ├── 3. Tạo 2 Sub-Orders (SubOrder A cho Shop A, SubOrder B cho Shop B)
    │       ├── 4. Tạo OrderItems kèm snapshot: sku_snapshot_json, unit_price, discount_amount
    │       ├── 5. Tạo Payment record (Pending)
    │       ├── 6. Tạo 2 PaymentEscrows (PendingCapture; chuyển Holding khi thanh toán thành công)
    │       ├── 7. Xóa 3 món đã chọn khỏi giỏ hàng (giữ lại các món chưa chọn)
    │       └── 8. Ghi OutboxMessage(OrderPlacedEvent) trong cùng DB transaction
    │
    ├─ 4. Thanh toán:
    │   ├─ MoMo / VietQR: Buyer quét mã QR ──► Webhook IPN ──► Payment.Status = Success ──► SubOrders = Confirmed
    │   └─ COD: Đơn tạo thành công ──► SubOrders = Confirmed (Seller đóng gói ngay, thu tiền khi giao)
    │
    ├─ 5. Giao hàng & Escrow Release:
    │       Seller đóng gói ──► Gọi ĐVVC lấy hàng (Shipping) ──► ĐVVC giao xong (Delivered)
    │       ──► Đếm ngược 7 ngày khiếu nại (T+7)
    │       ──► Sau 7 ngày không có khiếu nại: Escrow chuyển Released ──► Cộng tiền vào ShopWallets.balance của Seller
```

### 4.2 Luồng Livestream: Agora RTC $\rightarrow$ Pin SP $\rightarrow$ Mua Ngay (Instant Buy)

```
[Seller Bắt đầu Live]
    │
    ├── 1. POST /livestreams/start ──► Server sinh Agora RTC Token (Role: Publisher) ──► Phát live
    │
[Viewer Vào Xem Live]
    │
    ├── 2. GET /livestreams/{id} ──► Server sinh Agora RTC Token (Role: Subscriber) ──► Xem live
    │       Kết nối WebSocket LivestreamHub (Group: live_{id})
    │
[Seller Ghim Sản Phẩm & Bật Giá Flash]
    │
    ├── 3. POST /livestreams/{id}/pin-product { skuId, flashPrice: 99000, duration: 120s }
    │       ──► SignalR Broadcast "ProductPinned" đến toàn bộ viewer
    │
[Viewer Bấm "Mua Ngay"]
    │
    └── 4. Client tự động thêm SKU vào giỏ, gán selectedIds=[skuId] ──► Chuyển thẳng đến trang Checkout
            (Bỏ qua bước giỏ hàng, áp dụng giá flashPrice trong luồng tính tiền)
```

---

## 5. 🐳 DEPLOYMENT & PRODUCTION DOCKER COMPOSE

```yaml
version: '3.8'

services:
  core-api:
    build:
      context: .
      dockerfile: NovaLive.CoreApi/Dockerfile
    ports:
      - "5000:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=novalive_db;Username=nova_user;Password=${DB_PASSWORD}
      - Redis__ConnectionString=redis:6379,password=${REDIS_PASSWORD}
      - RabbitMQ__Host=rabbitmq
      - Jwt__SecretKey=${JWT_SECRET_KEY}
    depends_on:
      - postgres
      - redis
      - rabbitmq
      - minio

  realtime-api:
    build:
      context: .
      dockerfile: NovaLive.RealtimeApi/Dockerfile
    ports:
      - "5001:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - Redis__ConnectionString=redis:6379,password=${REDIS_PASSWORD}
      - Jwt__SecretKey=${JWT_SECRET_KEY}
    depends_on:
      - redis

  postgres:
    image: postgres:17-alpine
    environment:
      POSTGRES_DB: novalive_db
      POSTGRES_USER: nova_user
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - pgdata:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    command: redis-server --requirepass ${REDIS_PASSWORD}
    volumes:
      - redisdata:/data

  rabbitmq:
    image: rabbitmq:3.13-management-alpine
    environment:
      RABBITMQ_DEFAULT_USER: nova_guest
      RABBITMQ_DEFAULT_PASS: ${RABBITMQ_PASSWORD}
    volumes:
      - rmqdata:/var/lib/rabbitmq

  minio:
    image: minio/minio:latest
    command: server /data --console-address ":9001"
    environment:
      MINIO_ROOT_USER: ${MINIO_USER}
      MINIO_ROOT_PASSWORD: ${MINIO_PASSWORD}
    volumes:
      - miniodata:/data

volumes:
  pgdata:
  redisdata:
  rmqdata:
  miniodata:
```
