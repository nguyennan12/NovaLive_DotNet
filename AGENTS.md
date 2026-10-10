# Quy ước code

- Chỉ sửa trong repo NovaLive khi làm task của NovaLive. Project POS tại source folder `BÀI TẬP LỚN` chỉ dùng để tham khảo, không ghi file hoặc chạy build/test trong đó.
- Tổ chức Application theo `UseCases/<Module>/Commands/<UseCase>/` và `UseCases/<Module>/Queries/<UseCase>/`, theo quy ước POS. Mỗi use case có Command/Query, Handler, Validator riêng; mỗi file một class/record chính, namespace khớp thư mục. Query không tham số không cần validator.
- Đặt logic dùng chung tại `UseCases/<Module>/Common/`, danh sách lỗi tại `UseCases/<Module>/Errors/`. Không copy-paste quy tắc password hay logic phiên đăng nhập/OTP.
- Đăng ký handler và FluentValidation validator bằng assembly scanning; cập nhật using và test khi di chuyển namespace. Giữ test theo cấu trúc use case tương ứng.
- Gửi email qua `IEmailSender`, hiện dùng Resend HTTP API bằng typed HttpClient. Cấu hình `Email:Provider`, `Email:ApiKey`, `Email:FromAddress`, `Email:FromName`; không đưa khóa thật vào source hoặc log. LoggingEmailSender chỉ dùng ở Development.
- OTP chỉ gửi sau khi transaction commit; lỗi gửi email phải được log an toàn và không rollback đăng ký. Không ghi password, OTP, refresh token thô, API key, request/response body của nhà cung cấp vào log.
- Khi đổi namespace Auth, giữ cơ chế lọc log cho toàn bộ namespace con `NovaLive.Application.UseCases.Auth.`.
- Các task tiếp theo (T10, T11, T15...) áp dụng quy ước này. Refactor không thay đổi hành vi nghiệp vụ; chạy `dotnet build` và `dotnet test` trước khi commit.
