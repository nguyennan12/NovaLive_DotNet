using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.FlashSales.Queries.GetActiveFlashSales;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.FlashSales;
using Xunit;

namespace NovaLive.Application.Tests.FlashSales;

public class GetActiveFlashSalesTests
{
    private readonly Mock<IFlashSaleRepository> _repoMock = new();
    private readonly Mock<ICacheService> _cacheMock = new();
    private readonly Mock<IDateTimeProvider> _clockMock = new();
    private readonly GetActiveFlashSalesQueryHandler _handler;

    public GetActiveFlashSalesTests()
    {
        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, Task<IReadOnlyList<FlashSaleCampaignResponse>>>>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<CancellationToken>()))
            .Returns<string, Func<CancellationToken, Task<IReadOnlyList<FlashSaleCampaignResponse>>>, TimeSpan?, CancellationToken>(
                (key, factory, ttl, ct) => factory(ct));

        _handler = new GetActiveFlashSalesQueryHandler(_repoMock.Object, _cacheMock.Object, _clockMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsActiveCampaigns()
    {
        var now = DateTimeOffset.UtcNow;
        _clockMock.Setup(c => c.UtcNow).Returns(now);

        var campaigns = new List<FlashSaleCampaign>
        {
            new("Active Campaign 1", now.AddMinutes(-30), now.AddMinutes(30)),
            new("Active Campaign 2", now.AddMinutes(-10), now.AddMinutes(50))
        };

        _repoMock.Setup(r => r.GetActiveCampaignsAsync(now, It.IsAny<CancellationToken>()))
            .ReturnsAsync(campaigns);

        var result = await _handler.Handle(new GetActiveFlashSalesQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Name.Should().Be("Active Campaign 1");
    }
}
