namespace NovaLive.Application.Abstractions.Services;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream content, string objectName, string contentType, CancellationToken cancellationToken = default);
}
