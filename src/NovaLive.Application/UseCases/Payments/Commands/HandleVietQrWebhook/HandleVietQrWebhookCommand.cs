using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Application.UseCases.Payments.Commands.HandleVietQrWebhook;

/// <summary>
/// Command tiếp nhận và xử lý webhook biến động số dư chuyển khoản VietQR / Ngân hàng.
/// </summary>
/// <param name="Request">DTO thông tin biến động số dư đã parse.</param>
/// <param name="RawBody">Chuỗi raw payload HTTP body phục vụ kiểm tra chữ ký HMAC.</param>
/// <param name="Signature">Chữ ký bảo mật truyền từ HTTP header (tuỳ chọn).</param>
public sealed record HandleVietQrWebhookCommand(
    VietQrWebhookRequest Request,
    string RawBody,
    string? Signature = null)
    : ICommand<VietQrWebhookResponse>, ITransactionalCommand;
