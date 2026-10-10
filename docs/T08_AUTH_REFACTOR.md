# T08 — Báo cáo refactor Auth

## Phạm vi và kết quả

Chỉ thay đổi source NovaLive. Đã đọc POS tại `../lapTrinhTrucQuan/BÀI TẬP LỚN`: POS dùng `UseCases/Auth/Commands/<UseCase>`, `Queries/<UseCase>`, `Errors`; mỗi Command/Query, Handler, Validator là một file riêng. Không tìm thấy implementation/config email trong POS, nên chọn Resend theo nhánh dự phòng của yêu cầu. Không chạy build/test hoặc ghi file trong POS.

Ba giai đoạn, mỗi giai đoạn build/test thành công trước commit:

| Giai đoạn | Commit / message | Build | Test toàn solution |
| --- | --- | --- | --- |
| 1 | `872c2fc` — `refactor(auth): defer Google login and remove integration` | Thành công | 87 pass, 2 skip |
| 2 | `eb033b4` — `refactor(auth): deliver OTP through Resend HTTP API` | Thành công | 107 pass, 2 skip |
| 3 | `refactor(auth): organize use cases and preserve auth contracts` | Thành công | 139 pass, 2 skip |

Hai test skip giữ nguyên điều kiện ban đầu: `PostgresAuthTests.ConcurrentRefresh_OnlyOneSucceeds_AndReuseRevokesTheWinningToken` và `UniqueViolation_IsTranslatedToConflict_AndTransactionRollsBack`. Môi trường chưa có `NOVALIVE_TEST_POSTGRES`, Docker daemon chưa chạy. Không thay đổi hoặc làm yếu các test này. Các test workflow hiện chạy với SQLite; test cập nhật refresh đồng thời bằng hai kết nối SQLite vẫn pass.

NuGet mạng bị chặn DNS nên restore bằng cache có sẵn; VSTest cần chạy ngoài sandbox để mở kết nối loopback. Các lệnh đã dùng:

```powershell
dotnet restore --source C:/Users/Admin/.nuget/packages --disable-parallel -p:NuGetAudit=false -m:1
dotnet build --no-restore --disable-build-servers -m:1
dotnet test --no-build --no-restore -m:1
```

Build còn một warning MSB3277 về `Microsoft.EntityFrameworkCore.Relational` 10.0.4/10.0.12 tại project Api.Tests. Chưa điều chỉnh các package EF ngoài phạm vi Auth. Restore offline không thực hiện kiểm tra advisory NuGet.

## Cây thư mục

```text
src/NovaLive.Application/UseCases/Auth/
├── Common/
│   ├── AuthSessionService.cs
│   ├── AuthValidation.cs
│   └── OtpFlowService.cs
├── Errors/
│   └── AuthErrors.cs
├── Commands/
│   ├── Register/
│   │   ├── RegisterCommand.cs
│   │   ├── RegisterCommandHandler.cs
│   │   └── RegisterCommandValidator.cs
│   ├── VerifyOtp/       (VerifyOtpCommand + Handler + Validator)
│   ├── ResendOtp/       (ResendOtpCommand + Handler + Validator)
│   ├── Login/           (LoginCommand + Handler + Validator)
│   ├── Refresh/         (RefreshTokenCommand + Handler + Validator)
│   ├── ForgotPassword/  (ForgotPasswordCommand + Handler + Validator)
│   ├── ResetPassword/   (ResetPasswordCommand + Handler + Validator)
│   ├── ChangePassword/  (ChangePasswordCommand + Handler + Validator)
│   └── Logout/          (LogoutCommand + Handler + Validator)
└── Queries/
    └── GetCurrentUser/
        ├── GetCurrentUserQuery.cs
        └── GetCurrentUserQueryHandler.cs

tests/NovaLive.Application.Tests/UseCases/Auth/
├── Common/             (assembly scanning + helper kiểm tra log)
└── Commands/
    ├── Register/       (validator)
    ├── VerifyOtp/      (validator + log)
    ├── ResendOtp/      (validator)
    ├── Login/          (validator + log)
    ├── Refresh/        (validator + log)
    ├── ForgotPassword/ (validator)
    ├── ResetPassword/  (validator)
    ├── ChangePassword/ (validator)
    └── Logout/         (validator)

tests/NovaLive.Infrastructure.Tests/UseCases/Auth/
├── Commands/<cùng 9 use case>/  (workflow tests đã tách)
└── Queries/GetCurrentUser/     (workflow query)
```

33 file Application Auth, mỗi file một kiểu chính. Giữ assembly scanning MediatR/FluentValidation; test kiểm tra đủ 10 handler và 9 validator. Query hiện tại không có tham số nên không thêm validator rỗng. Đã so sánh phần thân cả 33 kiểu với giai đoạn 2: nguyên vẹn, ngoại trừ đổi tên `RegisterUserCommand` thành `RegisterCommand`. DTO HTTP giữ nguyên. Các workflow test cũ được tách theo use case, không bỏ assertion.

`LoggingBehavior` nhận diện toàn bộ namespace con `NovaLive.Application.UseCases.Auth.` để tiếp tục che thông tin nhạy cảm trong exception. `AGENTS.md` đã thêm quy ước cho T10, T11, T15 và các task sau.

## Package và cấu hình email

- Gỡ runtime package `Google.Apis.Auth` 1.73.0 và `MailKit` 4.17.0.
- Thêm `Microsoft.Extensions.Hosting` 10.0.11 **chỉ tại Infrastructure.Tests** để kiểm tra ValidateOnStart bằng host thật.
- Không thêm SDK Resend hoặc package runtime mới; typed HttpClient dùng IHttpClientFactory đã có trong dependency graph.
- Không còn tham chiếu Google, MailKit, SMTP trong source/test. Hai dòng docs được yêu cầu vẫn ghi login/google là **hoãn, chưa làm**.

| Key | Cách đặt |
| --- | --- |
| `Email:Provider` / `Email__Provider` | `Resend` cho gửi thật; `Logging` chỉ ở Development |
| `Email:ApiKey` / `Email__ApiKey` | Cấp bằng secret store hoặc biến môi trường |
| `Email:FromAddress` / `Email__FromAddress` | Địa chỉ người gửi được nhà cung cấp chấp nhận |
| `Email:FromName` / `Email__FromName` | Tên hiển thị người gửi; có thể để trống |

`appsettings.json` chỉ chứa chuỗi rỗng cho bốn key email. Development chọn `Logging`. Ngoài Development, provider rỗng/không hỗ trợ hoặc Logging sẽ fail-fast; Resend thiếu ApiKey/FromAddress cũng fail-fast, kể cả ở Development. LoggingEmailSender giữ hành vi cũ: chỉ log việc giả lập gửi, không in OTP. Muốn nhận mã thật trong Development phải chọn Resend và cấp cấu hình.

Request tuân theo [Resend Send Email API](https://resend.com/docs/api-reference/emails/send-email): POST `https://api.resend.com/emails`, Authorization Bearer, JSON `from`, `to`, `subject`, `html`. HTML tiếng Việt phân biệt xác thực tài khoản/đặt lại mật khẩu và ghi thời hạn 5 phút. Timeout mỗi lần là 10 giây; tối đa hai lần gửi, cách nhau 250 ms, chỉ retry 5xx/timeout. Cùng idempotency key trong hai lần để tránh gửi trùng sau timeout. Không tự theo redirect. Không retry lỗi 4xx, lỗi transport khác hoặc hủy từ caller.

Log chỉ ghi HTTP status, số lần thử, loại lỗi; không log body, header, exception của provider, OTP hay API key. Tắt logger HTTP mặc định của typed client. Lỗi cuối được OtpFlowService bắt sau commit: đăng ký và bản ghi OTP vẫn tồn tại. Test mock HttpMessageHandler kiểm tra URL/header/body, retry, timeout, cancellation, fail-fast, không rò secret và bảo toàn đăng ký.

## Hành vi Auth và AC-001

- Duplicate email/phone vẫn Conflict (HTTP 409); chuẩn hóa email/phone và BCrypt giữ nguyên.
- Đăng nhập sai vẫn khóa sau 5 lần, backoff 5/15/60/1440 phút; không kiểm password trong thời gian khóa.
- Refresh token vẫn chỉ lưu SHA-256; rotation/reuse detection và commit revocation khi thất bại giữ nguyên.
- Logout vẫn thu hồi refresh token thuộc đúng user và blacklist JTI theo TTL còn lại; protected endpoints vẫn trả 401 khi bị blacklist.
- OTP vẫn được hash, giới hạn 5 phút, kiểm lần thử/rate limit như cũ; không lưu OTP/refresh token thô hoặc ghi secret vào log.
- Không thay response envelope, ProblemDetails, Retry-After, yêu cầu xác thực hay transaction markers.

AuthController hiện kế thừa ApiControllerBase sẵn có và có đúng 10 endpoint (đã test). Base hiện chỉ cung cấp `[ApiController]` và Respond chung, **chưa có `[Authorize]` mặc định, Ip, Device, Failure** theo mẫu review. Giữ Ip/Device/Respond/Failure riêng trong AuthController vì response Auth khác base; ba endpoint protected vẫn có `[Authorize]`. Không tạo base mới hoặc thay hành vi của các controller khác. POS hiện dùng ControllerBase trực tiếp, không có ApiControllerBase riêng.

## Ví dụ curl — 10 endpoint

Cú pháp Bash/Git Bash. Thay origin bằng URL API của môi trường; các biến USER_ID, OTP, ACCESS, REFRESH lấy từ kết quả thật. Các lệnh minh họa độc lập: refresh sẽ thay token cũ, change-password/logout làm mất hiệu lực phiên đang dùng, nên cần cập nhật biến hoặc đăng nhập lại khi thử cả danh sách.

```bash
BASE='https://YOUR_API_ORIGIN/api/v1/auth'
USER_ID='USER_ID_FROM_REGISTER'
OTP='OTP_FROM_EMAIL'
ACCESS='ACCESS_TOKEN_FROM_LOGIN'
REFRESH='REFRESH_TOKEN_FROM_LOGIN'

# 1. Đăng ký
curl -X POST "$BASE/register" -H 'Content-Type: application/json' \
  -d '{"email":"buyer@example.com","phone":"0912345678","password":"Password123!","fullName":"Nguyen Van A"}'

# 2. Xác thực OTP
curl -X POST "$BASE/verify-otp" -H 'Content-Type: application/json' \
  -d "{\"userId\":\"$USER_ID\",\"otpCode\":\"$OTP\"}"

# 3. Gửi lại OTP
curl -X POST "$BASE/resend-otp" -H 'Content-Type: application/json' \
  -d '{"emailOrPhone":"buyer@example.com"}'

# 4. Đăng nhập
curl -X POST "$BASE/login" -H 'Content-Type: application/json' \
  -d '{"emailOrPhone":"buyer@example.com","password":"Password123!"}'

# 5. Xoay refresh token
curl -X POST "$BASE/refresh" -H 'Content-Type: application/json' \
  -d "{\"refreshToken\":\"$REFRESH\"}"

# 6. Quên mật khẩu
curl -X POST "$BASE/forgot-password" -H 'Content-Type: application/json' \
  -d '{"email":"buyer@example.com"}'

# 7. Đặt lại mật khẩu — OTP của email forgot-password
curl -X POST "$BASE/reset-password" -H 'Content-Type: application/json' \
  -d "{\"email\":\"buyer@example.com\",\"otpCode\":\"$OTP\",\"newPassword\":\"NewPassword123!\"}"

# 8. Đổi mật khẩu — dùng password hiện tại và phiên còn hiệu lực
curl -X POST "$BASE/change-password" -H "Authorization: Bearer $ACCESS" \
  -H 'Content-Type: application/json' \
  -d '{"oldPassword":"Password123!","newPassword":"NewPassword123!"}'

# 9. Đăng xuất
curl -X POST "$BASE/logout" -H "Authorization: Bearer $ACCESS" \
  -H 'Content-Type: application/json' -d "{\"refreshToken\":\"$REFRESH\"}"

# 10. Thông tin người dùng hiện tại
curl "$BASE/me" -H "Authorization: Bearer $ACCESS"
```

## Giả định và giới hạn còn lại

- Chưa gửi email thật qua Resend; kiểm chứng HTTP bằng mock, không dùng khóa thật. Cần cấu hình người gửi/secret trong môi trường triển khai.
- Giữ cơ chế callback sau commit hiện tại, chưa có durable email outbox. Nếu tiến trình dừng hoặc request bị hủy sau commit, email có thể chưa gửi; người dùng cần resend. Không thay cơ chế này trong refactor.
- Hai bài kiểm tra riêng PostgreSQL cần chạy lại khi có server test. Chưa tuyên bố đã kiểm chứng race/unique constraint trên PostgreSQL ở lượt này.
- Các cấu hình JWT/Otp:Pepper và hạ tầng database/cache hiện có vẫn là điều kiện chạy API; không điều chỉnh chúng ngoài phạm vi email.
- `docs/T08_AUTH_IMPLEMENTATION.md` là file untracked có sẵn trước tác vụ, được giữ nguyên, không đưa vào commit; nội dung cũ của file đó có thể còn nhắc cấu trúc/provider đã bỏ. Báo cáo này mô tả trạng thái refactor mới.
- Git status POS cuối tác vụ giống đầu tác vụ: `D docker/dev/.env.example`, `?? docker/dev/.evn.example`, `?? docs/split-payment-implementation.md`, `?? docs/t20-employees-implementation.md`. Đây là thay đổi có sẵn; tác vụ không sửa POS.
