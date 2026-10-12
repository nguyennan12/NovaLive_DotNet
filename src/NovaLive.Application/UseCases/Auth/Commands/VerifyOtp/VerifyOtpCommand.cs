using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Application.UseCases.Auth.Common;

namespace NovaLive.Application.UseCases.Auth.Commands.VerifyOtp;

public sealed record VerifyOtpCommand(VerifyOtpRequest Data, string? IpAddress = null, string? UserAgent = null)
    : ICommand<SessionResult>, ITransactionalCommand;
