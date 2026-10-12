using NovaLive.Application.Common.Messaging;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Contracts.V1.Auth;

namespace NovaLive.Application.UseCases.Auth.Commands.ChangePassword;

[RequireAuthenticated]
public sealed record ChangePasswordCommand(ChangePasswordRequest Data) : ICommand, ITransactionalCommand;
