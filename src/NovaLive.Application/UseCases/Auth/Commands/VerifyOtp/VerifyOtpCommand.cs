using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;

namespace NovaLive.Application.UseCases.Auth.Commands.VerifyOtp;

public sealed record VerifyOtpCommand(VerifyOtpRequest Data) : ICommand, ITransactionalCommand;
