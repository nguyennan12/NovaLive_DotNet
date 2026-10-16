namespace NovaLive.Infrastructure.Storage;

public sealed class MinioOptions
{
    public const string SectionName = "Minio";

    public string Endpoint { get; set; } = "localhost:9000";

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public bool UseSsl { get; set; } = false;

    public string CdnBaseUrl { get; set; } = "https://cdn.novalive.vn";
}
