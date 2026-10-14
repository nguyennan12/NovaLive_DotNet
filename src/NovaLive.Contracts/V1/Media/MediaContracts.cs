namespace NovaLive.Contracts.V1.Media;

public record UploadMediaResponse(
    string Url,
    string? FileName = null,
    string? Bucket = null,
    long? Size = null);
