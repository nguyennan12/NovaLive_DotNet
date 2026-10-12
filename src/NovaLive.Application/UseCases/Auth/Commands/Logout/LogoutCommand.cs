using NovaLive.Application.Common.Messaging;
using NovaLive.Application.Abstractions.Auth;

namespace NovaLive.Application.UseCases.Auth.Commands.Logout;

[RequireAuthenticated]
public sealed record LogoutCommand : ICommand, ITransactionalCommand;
