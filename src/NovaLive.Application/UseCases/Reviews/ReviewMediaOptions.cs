using NovaLive.Domain.Reviews;

namespace NovaLive.Application.UseCases.Reviews;

public sealed class ReviewMediaOptions
{
    public string? BucketBaseUrl { get; set; }

    public bool IsAllowed(string? value)
    {
        if (!ReviewMedia.IsValid(value)) return false;
        if (string.IsNullOrWhiteSpace(BucketBaseUrl)) return true;
        if (!Uri.TryCreate(BucketBaseUrl, UriKind.Absolute, out var prefix)
            || prefix.Scheme != Uri.UriSchemeHttps || prefix.UserInfo.Length > 0
            || prefix.Query.Length > 0 || prefix.Fragment.Length > 0) return false;

        var url = new Uri(value!, UriKind.Absolute);
        var path = url.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
        if (new[] { "%2f", "%5c", "%25" }.Any(escape => path.Contains(escape, StringComparison.OrdinalIgnoreCase))
            || Uri.UnescapeDataString(path).Split('/').Any(segment => segment is "." or "..")) return false;

        return url.Scheme == prefix.Scheme && url.Host == prefix.Host && url.Port == prefix.Port
            && url.AbsolutePath.StartsWith(prefix.AbsolutePath.TrimEnd('/') + "/", StringComparison.Ordinal);
    }
}
