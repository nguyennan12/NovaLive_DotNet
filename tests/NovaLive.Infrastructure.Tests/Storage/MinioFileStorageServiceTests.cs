using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.DataModel.Response;
using Moq;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Infrastructure.Storage;
using Xunit;

namespace NovaLive.Infrastructure.Tests.Storage;

public class MinioFileStorageServiceTests
{
    private readonly Mock<IMinioClient> _minioClientMock = new();
    private readonly Mock<ILogger<MinioFileStorageService>> _loggerMock = new();
    private readonly IOptions<MinioOptions> _options;
    private readonly MinioFileStorageService _service;

    public MinioFileStorageServiceTests()
    {
        _options = Options.Create(new MinioOptions
        {
            Endpoint = "localhost:9000",
            AccessKey = "minio_admin",
            SecretKey = "SuperPass@123",
            UseSsl = false,
            CdnBaseUrl = "https://cdn.novalive.vn"
        });

        _service = new MinioFileStorageService(
            _minioClientMock.Object,
            _options,
            _loggerMock.Object);
    }

    [Theory]
    [InlineData("sample.pdf")]
    [InlineData("malicious.exe")]
    [InlineData("script.sh")]
    [InlineData("animation.gif")]
    public async Task UploadAsync_ShouldThrowArgumentException_ForInvalidExtensions(string fileName)
    {
        using var stream = new MemoryStream(new byte[100]);

        var act = () => _service.UploadAsync(stream, fileName, "application/octet-stream", StorageBuckets.Products);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*không được hỗ trợ*");
    }

    [Fact]
    public async Task UploadAsync_ShouldThrowArgumentException_WhenImageExceeds10MB()
    {
        // 11MB stream
        var streamMock = new Mock<Stream>();
        streamMock.Setup(s => s.CanSeek).Returns(true);
        streamMock.Setup(s => s.Length).Returns(11 * 1024 * 1024);

        var act = () => _service.UploadAsync(streamMock.Object, "large_photo.jpg", "image/jpeg", StorageBuckets.Products);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*tối đa 10MB*");
    }

    [Fact]
    public async Task UploadAsync_ShouldThrowArgumentException_WhenVideoExceeds100MB()
    {
        // 101MB stream
        var streamMock = new Mock<Stream>();
        streamMock.Setup(s => s.CanSeek).Returns(true);
        streamMock.Setup(s => s.Length).Returns(101 * 1024 * 1024);

        var act = () => _service.UploadAsync(streamMock.Object, "large_movie.mp4", "video/mp4", StorageBuckets.Products);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*tối đa 100MB*");
    }

    [Fact]
    public async Task UploadAsync_ShouldThrowArgumentException_WhenStreamIsEmpty()
    {
        using var stream = new MemoryStream();

        var act = () => _service.UploadAsync(stream, "empty.png", "image/png", StorageBuckets.Products);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*rỗng*");
    }

    [Fact]
    public async Task UploadAsync_ShouldAttachUuid_AndReturnCdnUrl()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[100]);

        _minioClientMock
            .Setup(m => m.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _minioClientMock
            .Setup(m => m.PutObjectAsync(It.IsAny<PutObjectArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PutObjectResponse)null!);

        // Act
        var cdnUrl = await _service.UploadAsync(stream, "avatar.png", "image/png", StorageBuckets.Products);

        // Assert
        cdnUrl.Should().StartWith("https://cdn.novalive.vn/products/");
        cdnUrl.Should().EndWith("_avatar.png");

        // Verify UUID portion: length should be Guid length (32 chars) + 1 ('_') + "avatar.png"
        var urlParts = cdnUrl.Split('/');
        var uniqueName = urlParts[^1];
        var uuidPart = uniqueName.Substring(0, uniqueName.IndexOf('_'));
        uuidPart.Length.Should().Be(32); // Guid:N format

        _minioClientMock.Verify(m => m.PutObjectAsync(It.IsAny<PutObjectArgs>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnsureBucketExistsAsync_ShouldCreateBucketAndSetPolicy_WhenBucketDoesNotExist()
    {
        // Arrange
        _minioClientMock
            .Setup(m => m.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _minioClientMock
            .Setup(m => m.MakeBucketAsync(It.IsAny<MakeBucketArgs>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _minioClientMock
            .Setup(m => m.SetPolicyAsync(It.IsAny<SetPolicyArgs>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.EnsureBucketExistsAsync(StorageBuckets.Products);

        // Assert
        _minioClientMock.Verify(m => m.MakeBucketAsync(It.IsAny<MakeBucketArgs>(), It.IsAny<CancellationToken>()), Times.Once);
        _minioClientMock.Verify(m => m.SetPolicyAsync(It.IsAny<SetPolicyArgs>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EnsureBucketExistsAsync_ShouldNotSetPublicPolicy_ForPrivateBucket()
    {
        // Arrange
        _minioClientMock
            .Setup(m => m.BucketExistsAsync(It.IsAny<BucketExistsArgs>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _minioClientMock
            .Setup(m => m.MakeBucketAsync(It.IsAny<MakeBucketArgs>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.EnsureBucketExistsAsync(StorageBuckets.KycDocs);

        // Assert
        _minioClientMock.Verify(m => m.MakeBucketAsync(It.IsAny<MakeBucketArgs>(), It.IsAny<CancellationToken>()), Times.Once);
        _minioClientMock.Verify(m => m.SetPolicyAsync(It.IsAny<SetPolicyArgs>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
