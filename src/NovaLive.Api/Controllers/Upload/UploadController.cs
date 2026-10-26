using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Media;
using NovaLive.Application.UseCases.Media.Commands.UploadMedia;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.V1.Media;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Controllers.Upload;

[Authorize]
[Route("api/v1/upload")]
public sealed class UploadController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost("kyc-document")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 11 * 1024 * 1024)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<UploadMediaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadKycDocument(IFormFile? file, CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return Respond<UploadMediaResponse>(MediaErrors.Empty);

        await using var stream = file.OpenReadStream();
        var result = await Sender.Send(new UploadMediaCommand(
            stream, file.FileName, file.ContentType, file.Length, StorageBuckets.KycDocs, AllowPrivateKycBucket: true), ct);
        return Respond(result);
    }

    [HttpPost("media")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(110 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 110 * 1024 * 1024)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<UploadMediaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Contracts.Common.ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadMedia(
        IFormFile? file,
        [FromForm] string? bucket = null,
        CancellationToken ct = default)
    {
        var uploadedFile = file;
        if (uploadedFile is null && HttpContext?.Request?.HasFormContentType == true && HttpContext.Request.Form?.Files is not null)
        {
            uploadedFile = HttpContext.Request.Form.Files["file"] ?? HttpContext.Request.Form.Files.FirstOrDefault();
        }

        if (uploadedFile is null || uploadedFile.Length == 0)
        {
            return Respond<UploadMediaResponse>(MediaErrors.Empty);
        }

        await using var stream = uploadedFile.OpenReadStream();
        var command = new UploadMediaCommand(
            stream,
            uploadedFile.FileName,
            uploadedFile.ContentType,
            uploadedFile.Length,
            bucket);

        var result = await Sender.Send(command, ct);
        return Respond(result);
    }
}
