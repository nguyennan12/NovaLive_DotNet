namespace NovaLive.Application.UseCases.Shops;

public sealed class KycUrlOptions
{
    public const string SectionName = "KycUrls";
    // Full trusted bucket prefixes, e.g. https://cdn.example.com/kyc-docs/.
    // Empty configuration rejects every KYC URL (fail closed).
    public string[] AllowedPrefixes { get; set; } = [];

    public bool IsAllowed(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value != value.Trim() || value.Contains('\\')
            || !Uri.TryCreate(value, UriKind.Absolute, out var url)
            || url.Scheme is not ("https" or "http") || url.UserInfo.Length > 0 || url.Fragment.Length > 0)
            return false;

        // Reject encoded separators/dot segments before matching the canonical URI.
        var path = url.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
        if (new[] { "%2f", "%5c", "%25" }.Any(escape => path.Contains(escape, StringComparison.OrdinalIgnoreCase))
            || Uri.UnescapeDataString(path).Split('/').Any(segment => segment is "." or "..")) return false;

        return AllowedPrefixes.Any(value => Uri.TryCreate(value, UriKind.Absolute, out var prefix)
            && prefix.Scheme is "https" or "http" && prefix.UserInfo.Length == 0
            && prefix.Query.Length == 0 && prefix.Fragment.Length == 0
            && prefix.AbsolutePath.TrimEnd('/').EndsWith("/kyc-docs", StringComparison.Ordinal)
            && url.Scheme == prefix.Scheme && url.Host == prefix.Host && url.Port == prefix.Port
            && url.AbsolutePath.StartsWith(prefix.AbsolutePath.TrimEnd('/') + "/", StringComparison.Ordinal)
            && url.AbsolutePath.Length > prefix.AbsolutePath.TrimEnd('/').Length + 1);
    }
}
