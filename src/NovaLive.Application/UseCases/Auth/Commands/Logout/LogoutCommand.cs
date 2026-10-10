using NovaLive.Application.Common.Messaging;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Contracts.V1.Auth;

namespace NovaLive.Application.UseCases.Auth.Commands.Logout;

[RequireAuthenticated]
public sealed record LogoutCommand(LogoutRequest Data) : ICommand, ITransactionalCommand;
