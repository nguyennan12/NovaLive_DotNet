using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;

namespace NovaLive.Application.UseCases.Auth.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(ForgotPasswordRequest Data) : ICommand, ITransactionalCommand;
