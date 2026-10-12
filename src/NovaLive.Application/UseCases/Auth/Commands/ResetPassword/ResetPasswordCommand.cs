using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;

namespace NovaLive.Application.UseCases.Auth.Commands.ResetPassword;

public sealed record ResetPasswordCommand(ResetPasswordRequest Data) : ICommand, ITransactionalCommand;
