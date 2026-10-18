using System.Net;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Auth;

public static class OtpEmailTemplate
{
    private const string ResourceName = "NovaLive.Application.Common.Templates.Otp.html";
    private static readonly string Template = LoadTemplate();

    public static string Render(string otp, OtpType type, string purpose)
    {
        var title = type == OtpType.EmailVerify
            ? "Mã xác thực một lần của bạn"
            : "Mã đặt lại mật khẩu của bạn";
        var digits = string.Join("", otp.Select(digit =>
            $"<td align=\"center\" style=\"padding:0 4px;\"><span style=\"display:inline-block;width:42px;height:50px;line-height:50px;border:1px solid #d7e6ff;border-radius:9px;background:#f8fbff;color:#3485f7;font-size:24px;font-weight:bold;\">{WebUtility.HtmlEncode(digit.ToString())}</span></td>"));

        return Template
            .Replace("{{TITLE}}", WebUtility.HtmlEncode(title), StringComparison.Ordinal)
            .Replace("{{INTRO}}", WebUtility.HtmlEncode($"Mã OTP để {purpose} trên NovaLive của bạn:"), StringComparison.Ordinal)
            .Replace("{{OTP_ACCESSIBLE}}", WebUtility.HtmlEncode(otp), StringComparison.Ordinal)
            .Replace("{{OTP_DIGITS}}", digits, StringComparison.Ordinal);
    }

    private static string LoadTemplate()
    {
        using var stream = typeof(IEmailSender).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded email template '{ResourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
