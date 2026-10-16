namespace NovaLive.Application.Abstractions.Services;

public static class StorageBuckets
{
    public const string Products = "products";

    public const string KycDocs = "kyc-docs";

    public const string Reviews = "reviews";

    public const string Livestreams = "livestreams";

    public static readonly string[] All = [Products, KycDocs, Reviews, Livestreams];

    public static readonly string[] Public = [Products, Reviews, Livestreams];

    public static readonly string[] Private = [KycDocs];

    public static bool IsValid(string bucketName) =>
        All.Contains(bucketName, StringComparer.OrdinalIgnoreCase);

    public static bool IsPublic(string bucketName) =>
        Public.Contains(bucketName, StringComparer.OrdinalIgnoreCase);
}
