using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers.Upload;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Media.Commands.UploadMedia;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Media;
using NovaLive.Domain.Common;
using Xunit;

namespace NovaLive.Api.Tests.Upload;

public class UploadControllerTests
{
    private readonly Mock<ISender> _senderMock = new();
    private readonly UploadController _controller;

    public UploadControllerTests()
    {
        _controller = new UploadController(_senderMock.Object);
    }

    [Fact]
    public async Task UploadMedia_WithValidFile_ShouldReturn200_WithCdnUrl()
    {
        // Arrange
        const string expectedUrl = "https://cdn.novalive.vn/products/uuid_image.png";
        var content = "dummy image content"u8.ToArray();
        var stream = new MemoryStream(content);
        var formFile = new FormFile(stream, 0, content.Length, "file", "image.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var responseData = new UploadMediaResponse(expectedUrl, "image.png", StorageBuckets.Products, content.Length);

        _senderMock
            .Setup(s => s.Send(It.Is<UploadMediaCommand>(c =>
                c.FileName == "image.png" &&
                c.ContentType == "image/png" &&
                c.Length == content.Length), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(responseData));

        // Act
        var result = await _controller.UploadMedia(formFile, StorageBuckets.Products, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);

        var apiResponse = objResult.Value.Should().BeAssignableTo<ApiResponse<UploadMediaResponse>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data!.Url.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task UploadMedia_WithNullFile_ShouldReturn400BadRequest()
    {
        // Act
        var result = await _controller.UploadMedia(null, StorageBuckets.Products, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(400);

        var apiResponse = objResult.Value.Should().BeAssignableTo<ApiResponse<UploadMediaResponse>>().Subject;
        apiResponse.Success.Should().BeFalse();
        apiResponse.Error!.Code.Should().Be("Media.Empty");
    }

    [Fact]
    public async Task UploadMedia_WithInvalidFormat_ShouldReturn400BadRequest()
    {
        // Arrange
        var content = "some malicious script"u8.ToArray();
        var stream = new MemoryStream(content);
        var formFile = new FormFile(stream, 0, content.Length, "file", "virus.exe")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/x-msdownload"
        };

        var validationError = Error.Validation(
            "File.InvalidExtension",
            "Định dạng file không được hỗ trợ. Chỉ chấp nhận các định dạng: .jpg, .jpeg, .png, .webp, .mp4.");

        _senderMock
            .Setup(s => s.Send(It.IsAny<UploadMediaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UploadMediaResponse>(validationError));

        // Act
        var result = await _controller.UploadMedia(formFile, StorageBuckets.Products, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(400);

        var apiResponse = objResult.Value.Should().BeAssignableTo<ApiResponse<UploadMediaResponse>>().Subject;
        apiResponse.Success.Should().BeFalse();
        apiResponse.Error!.Code.Should().Be("File.InvalidExtension");
    }

    [Fact]
    public async Task UploadMedia_ExceedingLimit_ShouldReturn400BadRequest()
    {
        // Arrange
        var content = new byte[100];
        var stream = new MemoryStream(content);
        var formFile = new FormFile(stream, 0, content.Length, "file", "large_video.mp4")
        {
            Headers = new HeaderDictionary(),
            ContentType = "video/mp4"
        };

        var sizeError = Error.Validation(
            "File.ExceedsLimit",
            "Dung lượng video vượt quá giới hạn cho phép (tối đa 100MB).");

        _senderMock
            .Setup(s => s.Send(It.IsAny<UploadMediaCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<UploadMediaResponse>(sizeError));

        // Act
        var result = await _controller.UploadMedia(formFile, StorageBuckets.Products, CancellationToken.None);

        // Assert
        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(400);

        var apiResponse = objResult.Value.Should().BeAssignableTo<ApiResponse<UploadMediaResponse>>().Subject;
        apiResponse.Success.Should().BeFalse();
        apiResponse.Error!.Code.Should().Be("File.ExceedsLimit");
    }

    [Fact]
    public void UploadController_ShouldBeDecoratedWithAuthorizeAttribute()
    {
        typeof(UploadController).Should().BeDecoratedWith<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>();
    }
}
