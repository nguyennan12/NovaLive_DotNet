using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Payments;

namespace NovaLive.Application.UseCases.Payments.Commands.HandleMomoIpn;

public sealed record HandleMomoIpnCommand(MomoIpnRequest Request)
    : ICommand<MomoIpnResponse>, ITransactionalCommand;
