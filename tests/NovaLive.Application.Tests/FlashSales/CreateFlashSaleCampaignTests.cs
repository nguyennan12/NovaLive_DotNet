using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.FlashSales.Commands.CreateFlashSaleCampaign;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.FlashSales;
using Xunit;

namespace NovaLive.Application.Tests.FlashSales;

public class CreateFlashSaleCampaignTests
{
    private readonly Mock<IFlashSaleRepository> _repoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly CreateFlashSaleCampaignCommandHandler _handler;

    public CreateFlashSaleCampaignTests()
    {
        _handler = new CreateFlashSaleCampaignCommandHandler(_repoMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateCampaignAndInvalidateCache()
    {
        var startAt = DateTimeOffset.UtcNow.AddHours(1);
        var endAt = startAt.AddHours(2);
        var req = new CreateFlashSaleCampaignRequest("11.11 Flash Sale", startAt, endAt, "https://banner.jpg");

        var result = await _handler.Handle(new CreateFlashSaleCampaignCommand(req), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("11.11 Flash Sale");
        result.Value.Status.Should().Be("Scheduled");

        _repoMock.Verify(r => r.AddCampaignAsync(It.IsAny<FlashSaleCampaign>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("flashsales:active", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Validator_WhenEndAtBeforeStartAt_ShouldHaveValidationError()
    {
        var now = DateTimeOffset.UtcNow;
        _clockMock.Setup(c => c.UtcNow).Returns(now);
        var validator = new CreateFlashSaleCampaignCommandValidator(_clockMock.Object);

        var req = new CreateFlashSaleCampaignRequest("Sale", now.AddHours(2), now.AddHours(1), null);
        var validationResult = validator.Validate(new CreateFlashSaleCampaignCommand(req));

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName.Contains("EndAt"));
    }

    [Fact]
    public void Validator_WhenStartAtInPastOver5Minutes_ShouldHaveValidationError()
    {
        var now = DateTimeOffset.UtcNow;
        _clockMock.Setup(c => c.UtcNow).Returns(now);
        var validator = new CreateFlashSaleCampaignCommandValidator(_clockMock.Object);

        var req = new CreateFlashSaleCampaignRequest("Sale", now.AddMinutes(-10), now.AddHours(1), null);
        var validationResult = validator.Validate(new CreateFlashSaleCampaignCommand(req));

        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e => e.PropertyName.Contains("StartAt"));
    }
}
