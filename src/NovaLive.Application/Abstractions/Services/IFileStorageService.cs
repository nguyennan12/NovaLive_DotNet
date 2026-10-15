namespace NovaLive.Application.Abstractions.Services;

public interface IFileStorageService
{
    Task<string> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string bucketName = StorageBuckets.Products,
        CancellationToken cancellationToken = default);

    Task<string> UploadAsync(
        Stream content,
        string objectName,
        string contentType,
        CancellationToken cancellationToken);
}
