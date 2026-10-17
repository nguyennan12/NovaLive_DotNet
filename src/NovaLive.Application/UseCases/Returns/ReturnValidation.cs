using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Returns;
public static class ReturnValidation
{
    public static bool IsMediaUrl(string? value) => Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme is "https" or "http";
    public static bool IsStatus(string? value) => value == null || Enum.GetNames<ReturnStatus>().Contains(value);
}
