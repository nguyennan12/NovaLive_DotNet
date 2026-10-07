# NovaLive — E-Commerce & Livestream Platform

Hệ thống sàn thương mại điện tử đa người bán (Multi-vendor Marketplace) kết hợp Livestream phát sóng trực tiếp, xây dựng theo kiến trúc **Clean Architecture & CQRS**:

- **Backend:** ASP.NET Core (.NET 10) Web API + SignalR Realtime Hubs + MediatR (CQRS) + FluentValidation + Result Pattern + JWT Bearer
- **Database & Cache:** PostgreSQL 17 (46 Bảng, JSONB, Composite Indexes) + Redis 7 (Cache Aside, Idempotency, SignalR Backplane)
- **Message Broker & Storage:** RabbitMQ (Event-driven Outbox Pattern) + MinIO (S3-compatible Object Storage cho ảnh/video sản phẩm)
- **Testing & CI:** xUnit + FluentAssertions + Moq (Unit & Integration Tests) + GitHub Actions CI
- **Deployment:** Docker Compose (Hỗ trợ Hot-reload tức thì cho môi trường Development)

---

## 🛠 Yêu cầu môi trường

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Docker Engine + Docker Compose v2)
- (Tùy chọn) EF Core CLI tools: `dotnet tool install --global dotnet-ef`
- (Tùy chọn) DBeaver / pgAdmin / DataGrip để kết nối PostgreSQL

---

## 🚀 Khởi chạy dự án

### Cách 1: Khởi chạy toàn bộ hệ thống bằng Docker Compose (Khuyên dùng ⭐)

Hệ thống đã cấu hình sẵn file mẫu `.env.example` và tự động kích hoạt **Hot-reload** mã nguồn:

```bash
# 1. Tạo file .env từ template (nếu chưa có):
cp .env.example .env

# 2. Khởi chạy toàn bộ dịch vụ (PostgreSQL, Redis, RabbitMQ, MinIO, NovaLive API):
docker compose up -d --build
```

**Xem log ứng dụng theo thời gian thực:**
```bash
docker logs -f novalive_api_dev
```

**Dừng toàn bộ hệ thống:**
```bash
docker compose down
```

> **Lưu ý:** Hệ thống đã tích hợp cơ chế **Auto-Migration** khi khởi động API, tự động tạo đủ 46 bảng database và nạp dữ liệu mẫu ban đầu (Roles, Permissions, Super Admin) mà không cần chạy lệnh thủ công.

---

### Cách 2: Khởi chạy trực tiếp bằng .NET CLI (Local Development)

Nếu bạn muốn debug trực tiếp từ Visual Studio / Rider / VS Code:

```bash
# 1. Khởi động các dịch vụ phụ trợ (PostgreSQL, Redis, RabbitMQ, MinIO):
docker compose up -d postgres redis rabbitmq minio

# 2. Khởi chạy Backend API:
dotnet run --project src/NovaLive.Api/NovaLive.Api.csproj
```

---

## 🧪 Chạy Kiểm thử (Unit & Integration Tests)

Hệ thống có sẵn 4 project kiểm thử bao phủ toàn bộ các tầng:

```bash
# Chạy toàn bộ test trong Solution:
dotnet test NovaLive.sln
```

---

## 🌐 Danh sách Dịch vụ & Cổng truy cập

| Dịch vụ | Địa chỉ / URL | Ghi chú |
| :--- | :--- | :--- |
| **NovaLive Web API** | http://localhost:5000 | Cổng chính API Backend |
| **OpenAPI / Swagger Spec** | http://localhost:5000/openapi/v1.json | Tài liệu định dạng API |
| **Health Check Endpoint** | http://localhost:5000/api/v1/health | Kiểm tra tình trạng hoạt động của API |
| **SignalR Livestream Hub** | `ws://localhost:5000/hubs/livestream` | Kênh realtime phòng Live, chat, thả tim |
| **SignalR Order Hub** | `ws://localhost:5000/hubs/order` | Kênh realtime nhận thông báo đơn hàng |
| **SignalR Payment Hub** | `ws://localhost:5000/hubs/payment` | Kênh realtime nhận kết quả thanh toán |
| **RabbitMQ Management Dashboard** | http://localhost:15672 | Quản trị hàng đợi tin nhắn (`guest` / `guest`) |
| **MinIO S3 Console UI** | http://localhost:9001 | Giao diện quản lý lưu trữ ảnh/video (`minio_admin` / `minio_password`) |
| **PostgreSQL Database** | `localhost:5432` | Cơ sở dữ liệu chính (`novalive_db`) |
| **Redis Cache** | `localhost:6379` | Cache & Idempotency store |

---

## 🔑 Thông tin Kết nối Mặc định

### 1. PostgreSQL Database
- **Host:** `localhost` (hoặc `postgres` khi chạy trong container)
- **Port:** `5432`
- **Database:** `novalive_db`
- **Username:** `nova_user`
- **Password:** `nova_password`

### 2. Redis Cache
- **Host:** `localhost` (hoặc `redis` khi chạy trong container)
- **Port:** `6379`

### 3. RabbitMQ Message Broker
- **Host:** `localhost` (hoặc `rabbitmq` khi chạy trong container)
- **Port:** `5672` (AMQP) / `15672` (Management Web UI)
- **Username / Password:** `guest` / `guest`

### 4. MinIO S3 Storage
- **API Endpoint:** `http://localhost:9000`
- **Console Web UI:** `http://localhost:9001`
- **Root User / Password:** `minio_admin` / `minio_password`

---

```
