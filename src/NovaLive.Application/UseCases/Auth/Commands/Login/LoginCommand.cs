using NovaLive.Application.UseCases.Auth.Common;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;

namespace NovaLive.Application.UseCases.Auth.Commands.Login;

public sealed record LoginCommand(LoginRequest Data, string? IpAddress = null, string? UserAgent = null) : ICommand<SessionResult>, ITransactionalCommand;
