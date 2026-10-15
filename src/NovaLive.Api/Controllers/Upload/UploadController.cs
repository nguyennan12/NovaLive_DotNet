using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Media;
using NovaLive.Application.UseCases.Media.Commands.UploadMedia;
using NovaLive.Contracts.V1.Media;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Controllers.Upload;

/// <summary>
/// Controller xử lý tải lên các tập tin media (hình ảnh, video) lên kho lưu trữ MinIO S3.
/// </summary>
/// <param name="sender">MediatR sender dùng để gửi command tải lên.</param>
[Authorize]
[Route("api/v1/upload")]
public sealed class UploadController(ISender sender) : ApiControllerBase(sender)
{
    /// <summary>
    /// Upload media file (ảnh hoặc video) lên MinIO S3 Object Storage và nhận URL CDN đầy đủ.
    /// Yêu cầu người dùng phải đăng nhập hợp lệ.
    /// </summary>
    /// <param name="file">File tải lên từ multipart/form-data (ảnh &lt;= 10MB, video &lt;= 100MB).</param>
    /// <param name="bucket">Bucket công khai đích (mặc định: products; các bucket hợp lệ: products, reviews, livestreams).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Thông tin file và CDN URL.</returns>
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
