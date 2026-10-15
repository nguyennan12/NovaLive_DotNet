using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Application.UseCases.Payments.Commands.HandleVietQrWebhook;

/// <summary>
/// Command tiếp nhận và xử lý webhook biến động số dư chuyển khoản VietQR / Ngân hàng.
/// </summary>
public sealed record HandleVietQrWebhookCommand(VietQrWebhookRequest Request, string? Signature = null)
    : ICommand<VietQrWebhookResponse>, ITransactionalCommand;
