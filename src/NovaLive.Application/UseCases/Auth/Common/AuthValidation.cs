using System.Text;

namespace NovaLive.Application.UseCases.Auth.Common;

internal static class AuthValidation
{
    public const string PasswordMessage = "Password must have at least 8 characters, upper/lowercase letters and a number, and at most 72 UTF-8 bytes.";
    public static bool BcryptLength(string? value) => value is not null && Encoding.UTF8.GetByteCount(value) <= 72;
    public static bool ValidPassword(string? value) => value is { Length: >= 8 } && BcryptLength(value)
        && value.Any(char.IsUpper) && value.Any(char.IsLower) && value.Any(char.IsDigit);
}
