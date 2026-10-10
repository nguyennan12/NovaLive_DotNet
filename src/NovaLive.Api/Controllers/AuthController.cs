using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NovaLive.Api.Auth;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Auth.Commands.ChangePassword;
using NovaLive.Application.UseCases.Auth.Commands.ForgotPassword;
using NovaLive.Application.UseCases.Auth.Commands.Login;
using NovaLive.Application.UseCases.Auth.Commands.Logout;
using NovaLive.Application.UseCases.Auth.Commands.Refresh;
using NovaLive.Application.UseCases.Auth.Commands.Register;
using NovaLive.Application.UseCases.Auth.Commands.ResendOtp;
using NovaLive.Application.UseCases.Auth.Commands.ResetPassword;
using NovaLive.Application.UseCases.Auth.Commands.VerifyOtp;
using NovaLive.Application.UseCases.Auth.Queries.GetCurrentUser;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Auth;
using NovaLive.Domain.Common;
namespace NovaLive.Api.Controllers;

[Route("api/v1/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController(ISender sender) : ApiControllerBase(sender)
{
    private string? Ip => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? Device => Request.Headers.UserAgent.ToString();
    [AllowAnonymous, HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest data, CancellationToken ct) =>
        Respond(await Sender.Send(new RegisterCommand(data), ct));
    [AllowAnonymous, HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest data, CancellationToken ct) =>
        Respond(await Sender.Send(new VerifyOtpCommand(data), ct));
    [AllowAnonymous, HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp(ResendOtpRequest data, CancellationToken ct) =>
        Respond(await Sender.Send(new ResendOtpCommand(data), ct));
    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest data, CancellationToken ct) =>
        Respond(await Sender.Send(new LoginCommand(data, Ip, Device), ct));
    [AllowAnonymous, HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest data, CancellationToken ct) =>
        Respond(await Sender.Send(new RefreshTokenCommand(data, Ip, Device), ct));
    [AllowAnonymous, HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest data, CancellationToken ct) =>
        Respond(await Sender.Send(new ForgotPasswordCommand(data), ct));
    [AllowAnonymous, HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest data, CancellationToken ct) =>
        Respond(await Sender.Send(new ResetPasswordCommand(data), ct));
    [Authorize, HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest data, CancellationToken ct) =>
        Respond(await Sender.Send(new LogoutCommand(data), ct));
    [Authorize, HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest data, CancellationToken ct) =>
        Respond(await Sender.Send(new ChangePasswordCommand(data), ct));
    [Authorize, HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct) =>
        Respond(await Sender.Send(new GetCurrentUserQuery(), ct));

    // Preserve Auth's existing envelope and ProblemDetails while sharing the existing controller base.
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

