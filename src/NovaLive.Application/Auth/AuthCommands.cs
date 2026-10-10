using NovaLive.Application.Common.Messaging;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Contracts.V1.Auth;
namespace NovaLive.Application.Auth;

public sealed record RegisterUserCommand(RegisterUserRequest Data) : ICommand<RegisterUserResponse>, ITransactionalCommand;
public sealed record VerifyOtpCommand(VerifyOtpRequest Data) : ICommand, ITransactionalCommand;
public sealed record ResendOtpCommand(ResendOtpRequest Data) : ICommand, ITransactionalCommand;
public sealed record LoginCommand(LoginRequest Data, string? IpAddress = null, string? UserAgent = null) : ICommand<AuthResponse>, ITransactionalCommand;
public sealed record LoginGoogleCommand(LoginGoogleRequest Data, string? IpAddress = null, string? UserAgent = null) : ICommand<AuthResponse>, ITransactionalCommand;
public sealed record RefreshTokenCommand(RefreshTokenRequest Data, string? IpAddress = null, string? UserAgent = null) : ICommand<AuthResponse>, ICommitOnFailureCommand;
public sealed record ForgotPasswordCommand(ForgotPasswordRequest Data) : ICommand, ITransactionalCommand;
public sealed record ResetPasswordCommand(ResetPasswordRequest Data) : ICommand, ITransactionalCommand;
[RequireAuthenticated]
public sealed record ChangePasswordCommand(ChangePasswordRequest Data) : ICommand, ITransactionalCommand;
[RequireAuthenticated]
public sealed record LogoutCommand(LogoutRequest Data) : ICommand, ITransactionalCommand;
[RequireAuthenticated]
public sealed record GetCurrentUserQuery : IQuery<UserInfoResponse>;

