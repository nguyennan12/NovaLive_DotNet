using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Auth;

namespace NovaLive.Application.UseCases.Auth.Commands.Refresh;

public sealed record RefreshTokenCommand(RefreshTokenRequest Data, string? IpAddress = null, string? UserAgent = null) : ICommand<AuthResponse>, ICommitOnFailureCommand;
