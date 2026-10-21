using NovaLive.Domain.Common;

namespace NovaLive.Domain.Reviews;

public static class ReviewMedia
{
    public static bool IsValid(string? value) => TryGetType(value).IsSuccess;

    public static Result<ReviewMediaType> TryGetType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > ReviewLimits.MediaUrlLength
            || value != value.Trim() || value.Contains('\\')
            || !Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps
            || uri.UserInfo.Length > 0 || uri.Fragment.Length > 0)
            return ReviewErrors.InvalidMedia;

        return Path.GetExtension(uri.AbsolutePath).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" or ".png" or ".webp" => ReviewMediaType.image,
            ".mp4" => ReviewMediaType.video,
            _ => ReviewErrors.InvalidMedia
        };
    }
}
