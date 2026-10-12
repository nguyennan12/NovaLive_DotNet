using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;

namespace NovaLive.Application.UseCases.Auth.Commands.ResendOtp;

public sealed record ResendOtpCommand(ResendOtpRequest Data) : ICommand, ITransactionalCommand;
