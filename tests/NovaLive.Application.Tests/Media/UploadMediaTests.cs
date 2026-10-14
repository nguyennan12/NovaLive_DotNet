using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Media.Commands.UploadMedia;
using Xunit;

namespace NovaLive.Application.Tests.Media;

public class UploadMediaTests
{
    private readonly UploadMediaCommandValidator _validator = new();
    private readonly Mock<IFileStorageService> _fileStorageMock = new();
    private readonly Mock<ILogger<UploadMediaCommandHandler>> _loggerMock = new();
    private readonly UploadMediaCommandHandler _handler;

    public UploadMediaTests()
    {
        _handler = new UploadMediaCommandHandler(_fileStorageMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("image.jpg", 1024)]
    [InlineData("photo.jpeg", 10 * 1024 * 1024)]
    [InlineData("banner.png", 5 * 1024 * 1024)]
    [InlineData("thumb.webp", 2 * 1024 * 1024)]
    [InlineData("video.mp4", 100 * 1024 * 1024)]
    public async Task Validator_ShouldPass_ForValidExtensionsAndSizes(string fileName, long length)
    {
        using var stream = new MemoryStream(new byte[10]);
        var command = new UploadMediaCommand(stream, fileName, "application/octet-stream", length, StorageBuckets.Products);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("script.sh")]
    [InlineData("program.exe")]
    [InlineData("animation.gif")]
    [InlineData("archive.zip")]
    public async Task Validator_ShouldFail_ForInvalidExtensions(string fileName)
    {
        using var stream = new MemoryStream(new byte[10]);
        var command = new UploadMediaCommand(stream, fileName, "application/octet-stream", 1024);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FileName");
    }

    [Fact]
    public async Task Validator_ShouldFail_WhenImageExceeds10MB()
    {
        using var stream = new MemoryStream(new byte[10]);
        long size11Mb = 11 * 1024 * 1024;
        var command = new UploadMediaCommand(stream, "large_photo.jpg", "image/jpeg", size11Mb);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Length" && e.ErrorMessage.Contains("10MB"));
    }

    [Fact]
    public async Task Validator_ShouldFail_WhenVideoExceeds100MB()
    {
        using var stream = new MemoryStream(new byte[10]);
        long size101Mb = 101 * 1024 * 1024;
        var command = new UploadMediaCommand(stream, "long_video.mp4", "video/mp4", size101Mb);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Length" && e.ErrorMessage.Contains("100MB"));
    }

    [Fact]
    public async Task Validator_ShouldFail_WhenFileIsEmpty()
    {
        using var stream = new MemoryStream();
        var command = new UploadMediaCommand(stream, "empty.png", "image/png", 0);

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Length");
    }

    [Fact]
    public async Task Validator_ShouldFail_WhenBucketIsInvalid()
    {
        using var stream = new MemoryStream(new byte[10]);
        var command = new UploadMediaCommand(stream, "valid.jpg", "image/jpeg", 1024, "invalid-bucket");

        var result = await _validator.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Bucket");
    }

    [Fact]
    public async Task Handler_ShouldReturnSuccess_WithCdnUrl()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[100]);
        const string expectedUrl = "https://cdn.novalive.vn/products/abc123_test.jpg";

        _fileStorageMock
            .Setup(s => s.UploadAsync(
                It.IsAny<Stream>(),
                "test.jpg",
                "image/jpeg",
                StorageBuckets.Products,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUrl);

        var command = new UploadMediaCommand(stream, "test.jpg", "image/jpeg", 100, StorageBuckets.Products);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Url.Should().Be(expectedUrl);
        result.Value.FileName.Should().Be("test.jpg");
        result.Value.Bucket.Should().Be(StorageBuckets.Products);
        result.Value.Size.Should().Be(100);
    }
}
