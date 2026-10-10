using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NovaLive.Api.Auth;
using NovaLive.Application.Auth;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
namespace NovaLive.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    private string? Ip => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? Device => Request.Headers.UserAgent.ToString();
    [AllowAnonymous, HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest data, CancellationToken ct) =>
        Respond(await sender.Send(new RegisterUserCommand(data), ct));
    [AllowAnonymous, HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest data, CancellationToken ct) =>
        Respond(await sender.Send(new VerifyOtpCommand(data), ct));
    [AllowAnonymous, HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(ResendOtpRequest data, CancellationToken ct) =>
        Respond(await sender.Send(new ResendOtpCommand(data), ct));
    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest data, CancellationToken ct) =>
        Respond(await sender.Send(new LoginCommand(data, Ip, Device), ct));
    [AllowAnonymous, HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest data, CancellationToken ct) =>
        Respond(await sender.Send(new RefreshTokenCommand(data, Ip, Device), ct));
    [AllowAnonymous, HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest data, CancellationToken ct) =>
        Respond(await sender.Send(new ForgotPasswordCommand(data), ct));
    [AllowAnonymous, HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest data, CancellationToken ct) =>
        Respond(await sender.Send(new ResetPasswordCommand(data), ct));
    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest data, CancellationToken ct) =>
        Respond(await sender.Send(new LogoutCommand(data), ct));
    [Authorize, HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest data, CancellationToken ct) =>
        Respond(await sender.Send(new ChangePasswordCommand(data), ct));
    [Authorize, HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct) =>
        Respond(await sender.Send(new GetCurrentUserQuery(), ct));

    private IActionResult Respond<T>(Result<T> result) => result.IsSuccess
        ? Ok(ApiResponse<T>.Ok(result.Value!)) : Failure(result);
    private IActionResult Respond(Result result) => result.IsSuccess
        ? Ok(ApiResponse<object>.Ok(new { message = "Request completed." })) : Failure(result);
    private IActionResult Failure(Result result)
    {
        var problem = AuthProblem.Create(HttpContext, result.Error);
        if (result is IValidationResult validation)
            problem.Extensions["errors"] = validation.Errors.GroupBy(error => error.Code)
                .ToDictionary(group => group.Key, group => group.Select(error => error.Message).ToArray());
        return new ObjectResult(problem) { StatusCode = problem.Status, ContentTypes = { "application/problem+json" } };
    }
}

