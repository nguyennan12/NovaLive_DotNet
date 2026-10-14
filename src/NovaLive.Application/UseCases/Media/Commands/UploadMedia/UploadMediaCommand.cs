using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Media;

namespace NovaLive.Application.UseCases.Media.Commands.UploadMedia;

public record UploadMediaCommand(
    Stream FileStream,
    string FileName,
    string ContentType,
    long Length,
    string? Bucket = null) : ICommand<UploadMediaResponse>;
