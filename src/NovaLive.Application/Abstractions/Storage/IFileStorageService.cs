namespace NovaLive.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream content, string objectName, string contentType, CancellationToken cancellationToken = default);
}
