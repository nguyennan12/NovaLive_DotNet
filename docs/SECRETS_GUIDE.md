# Hướng Dẫn Quản Lý Cấu Hình & Bí Mật (Secrets & Configuration Guide)

Tài liệu này quy chuẩn cách thức quản lý biến môi trường, thông tin kết nối và bí mật (secrets) trong toàn bộ dự án **NovaLive**.

---

## 1. Nguyên Tắc Cốt Lõi (Core Principles)

1. **Tuyệt đối KHÔNG commit Secret lên Git (Zero Secrets in Git):**
   - Mọi API Key, Password, Secret Key, Pepper,... không bao giờ được viết cứng (hardcode) trong code C# hoặc commit vào các file JSON được theo dõi bởi Git.
   - Các file chứa secret như `.env`, `appsettings.Development.json`, `appsettings.Local.json` đều được liệt kê trong `.gitignore`.

2. **Quy ước đặt tên biến môi trường (FULL UPPERCASE):**
   - Tên biến trong `.env` và `.env.example` bắt buộc sử dụng chữ in hoa và gạch dưới: `POSTGRES_PASSWORD`, `JWT_SECRET`, `OTP_PEPPER`, `EMAIL_API_KEY`, `GHN_API_TOKEN`,...
   - Không dùng `camelCase`, `PascalCase` hay dấu gạch nối trong tên biến `.env`.

3. **Phân cấp cấu hình trong ASP.NET Core (Hierarchy):**
   Thứ tự ưu tiên nạp cấu hình từ thấp đến cao:
   1. `appsettings.json` *(Tầng cơ sở - commit lên Git)*: Định nghĩa **schema/cấu trúc** cấu hình với giá trị rỗng `""` cho secret và kết nối, giữ nguyên các giá trị mặc định hệ thống (Cookies, CORS, Email Provider, Port,...).
   2. `appsettings.Development.json` *(Tầng dev local - nằm trong `.gitignore`)*: Chứa chuỗi kết nối cục bộ (`localhost:5432`, `localhost:6379`,...) khi lập trình viên chạy app trực tiếp ngoài Docker.
   3. **Environment Variables (Docker env / OS env)** *(Tầng ưu tiên cao nhất)*: Ghi đè toàn bộ các giá trị từ file JSON khi ứng dụng khởi chạy trong container hoặc môi trường Production.

4. **Docker Compose không dùng Fallback Default (`:-`):**
   - Trong `docker-compose.yml`, các biến môi trường được tham chiếu sạch dạng `${VAR}` (ví dụ: `POSTGRES_PASSWORD: "${POSTGRES_PASSWORD}"`).
   - Tuyệt đối không dùng cú pháp fallback `:- default` để đảm bảo lỗi cấu hình sẽ được phát hiện ngay lập tức thay vì chạy ngầm với mật khẩu mặc định thiếu an toàn.

5. **Code C# không dùng Hardcoded Fallback cho Secret/Kết Nối:**
   - Trong code C#, không viết `configuration.GetConnectionString("DefaultConnection") ?? "Host=localhost..."`.
   - Nếu thiếu cấu hình bắt buộc, ném ngay ngoại lệ:
     ```csharp
     var connectionString = configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
     ```

---

## 2. Ma Trận Cấu Hình Theo Môi Trường

| Mục Cấu Hình | `appsettings.json` (Git) | `appsettings.Development.json` (Local Host Dev) | `.env` / `docker-compose.yml` (Docker Dev & Prod) |
| :--- | :--- | :--- | :--- |
| **PostgreSQL Connection** | `""` | `Host=localhost;Port=5432;...` | `ConnectionStrings__DefaultConnection: Host=postgres;...` |
| **Redis Connection** | `""` | `localhost:6379` | `Redis__ConnectionString: redis:6379` |
| **RabbitMQ Host / Auth** | `""` | `localhost` / `guest` | `RabbitMQ__Host: rabbitmq`, `${RABBITMQ_DEFAULT_USER}` |
| **JWT Secret** | `""` | Không bắt buộc nếu chạy Docker | `Jwt__Secret: ${JWT_SECRET}` |
| **OTP Pepper** | `""` | Không bắt buộc nếu chạy Docker | `Otp__Pepper: ${OTP_PEPPER}` |
| **Email Resend API Key** | `""` | Không bắt buộc nếu chạy Docker | `Email__ApiKey: ${EMAIL_API_KEY}` |
| **GHN Shipping Token** | `""` | Không bắt buộc nếu chạy Docker | `Ghn__ApiToken: ${GHN_API_TOKEN}` |
| **Agora RTC AppId / Cert** | `""` | Placeholder 32 hex chars | `Agora__AppId: ${AGORA_APP_ID}`, `Agora__AppCertificate: ${AGORA_APP_CERTIFICATE}` |
| **CORS Allowed Origins** | `["http://localhost:3000", ...]` | Kế thừa từ `appsettings.json` | Kế thừa từ `appsettings.json` |
| **Cookies Config** | `nova_at`, `nova_rt`, `Secure: true` | `Secure: false` (hỗ trợ HTTP dev) | Kế thừa từ `appsettings.json` & dev override |

---

## 3. Quy Trình Làm Việc Của Lập Trình Viên (Developer Workflow)

### Kịch bản 1: Chạy toàn bộ hệ thống bằng Docker Compose (Khuyên dùng)
1. Tạo file `.env` từ template:
   ```bash
   cp .env.example .env
   ```
2. Điền các secret thật vào `.env` (nếu cần đổi mật khẩu hoặc API Key).
3. Khởi chạy:
   ```bash
   docker compose -f docker/dev/docker-compose.yml up -d
   ```
   Docker tự động nạp `.env` và mount thư mục code vào container.

### Kịch bản 2: Debug trực tiếp qua IDE / .NET CLI (Local Host Dev)
1. Bật các dịch vụ phụ trợ (DB, Redis, RabbitMQ, MinIO) qua Docker:
   ```bash
   docker compose -f docker/dev/docker-compose.yml up -d postgres redis rabbitmq minio
   ```
2. Chỉnh sửa file [src/NovaLive.Api/appsettings.Development.json](file:///e:/Project/novalive_remake/src/NovaLive.Api/appsettings.Development.json) (file này đã được `.gitignore`):
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=novalive_db;Username=nova_user;Password=nova_password"
     },
     "Redis": {
       "ConnectionString": "localhost:6379"
     },
     "RabbitMQ": {
       "Host": "localhost",
       "Username": "guest",
       "Password": "guest"
     }
   }
   ```
3. Chạy API:
   ```bash
   dotnet run --project src/NovaLive.Api/NovaLive.Api.csproj
   ```

---

## 4. Hướng Dẫn Thêm Một Secret Mới Vào Hệ Thống (4 Bước Chuẩn)

Khi bạn tích hợp thêm một dịch vụ thứ ba (ví dụ: Payment Gateway, SMS Provider):

### Bước 1: Khai báo Options class trong C#
Tạo class POCO strongly-typed options trong tầng `Infrastructure` hoặc `Application`:
```csharp
public sealed class PaymentOptions
{
    public const string SectionName = "Payment";
    public string ApiKey { get; init; } = string.Empty;
    public string MerchantId { get; init; } = string.Empty;
}
```

### Bước 2: Thêm Schema vào `appsettings.json`
Thêm cấu trúc section với giá trị rỗng:
```json
"Payment": {
  "ApiKey": "",
  "MerchantId": ""
}
```

### Bước 3: Khai báo biến trong `.env` & `.env.example`
Thêm biến **FULL UPPERCASE**:
```ini
# Payment Gateway Secrets
PAYMENT_API_KEY=your_secret_api_key_here
PAYMENT_MERCHANT_ID=your_merchant_id_here
```

### Bước 4: Ánh xạ trong `docker/dev/docker-compose.yml`
Thêm ánh xạ biến môi trường vào service `novalive-api`:
```yaml
      Payment__ApiKey: "${PAYMENT_API_KEY}"
      Payment__MerchantId: "${PAYMENT_MERCHANT_ID}"
```
*(ASP.NET Core tự động map dấu hai gạch dưới `__` thành dấu hai chấm `:` trong Configuration binding)*.
