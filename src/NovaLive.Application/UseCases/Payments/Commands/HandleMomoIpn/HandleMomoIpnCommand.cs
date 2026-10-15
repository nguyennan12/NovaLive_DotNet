using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Application.UseCases.Payments.Commands.HandleMomoIpn;

/// <summary>
/// Command tiếp nhận và xử lý thông báo IPN tức thì từ cổng thanh toán MoMo.
/// </summary>
public sealed record HandleMomoIpnCommand(MomoIpnRequest Request)
    : ICommand<MomoIpnResponse>, ITransactionalCommand;
