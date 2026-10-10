using System.Text;
using FluentValidation;
namespace NovaLive.Application.Auth;

internal static class AuthValidation
{
    public const string PasswordMessage = "Password must have at least 8 characters, upper/lowercase letters and a number, and at most 72 UTF-8 bytes.";
    public static bool BcryptLength(string? value) => value is not null && Encoding.UTF8.GetByteCount(value) <= 72;
    public static bool ValidPassword(string? value) => value is { Length: >= 8 } && BcryptLength(value)
        && value.Any(char.IsUpper) && value.Any(char.IsLower) && value.Any(char.IsDigit);
}

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.Email).NotEmpty().MaximumLength(255).EmailAddress();
        RuleFor(x => x.Data.Phone).NotEmpty().Matches(@"^(0|\+84)[35789][0-9]{8}$");
        RuleFor(x => x.Data.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Data.Password).Must(AuthValidation.ValidPassword).WithMessage(AuthValidation.PasswordMessage);
        });
    }
}

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.UserId).NotEmpty();
        RuleFor(x => x.Data.OtpCode).NotEmpty().Matches("^[0-9]{6}$");
        });
    }
}

public sealed class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
{
    public ResendOtpCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.EmailOrPhone).NotEmpty().MaximumLength(255);
        });
    }
}

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.EmailOrPhone).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Data.Password).NotEmpty().Must(AuthValidation.BcryptLength).WithMessage("Password is too long.");
        });
    }
}

public sealed class LoginGoogleCommandValidator : AbstractValidator<LoginGoogleCommand>
{
    public LoginGoogleCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.IdToken).NotEmpty().MaximumLength(16384);
        });
    }
}

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.RefreshToken).NotEmpty().MaximumLength(512);
        });
    }
}

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.RefreshToken).NotEmpty().MaximumLength(512);
        });
    }
}

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.Email).NotEmpty().MaximumLength(255).EmailAddress();
        });
    }
}

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.Email).NotEmpty().MaximumLength(255).EmailAddress();
        RuleFor(x => x.Data.OtpCode).NotEmpty().Matches("^[0-9]{6}$");
        RuleFor(x => x.Data.NewPassword).Must(AuthValidation.ValidPassword).WithMessage(AuthValidation.PasswordMessage);
        });
    }
}

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.Data).NotNull();
        When(x => x.Data is not null, () =>
        {
        RuleFor(x => x.Data.OldPassword).NotEmpty().Must(AuthValidation.BcryptLength).WithMessage("Password is too long.");
        RuleFor(x => x.Data.NewPassword).Must(AuthValidation.ValidPassword).WithMessage(AuthValidation.PasswordMessage);
        });
    }
}

