using FluentAssertions;
using NovaLive.Domain.Common;
using NovaLive.Domain.FlashSales;
using Xunit;

namespace NovaLive.Domain.Tests.FlashSales;

public class FlashSaleCampaignTests
{
    [Fact]
    public void Constructor_ValidInput_ShouldCreateScheduledCampaign()
    {
        var startAt = DateTimeOffset.UtcNow.AddHours(1);
        var endAt = startAt.AddHours(2);

        var campaign = new FlashSaleCampaign("Flash Sale 12.12", startAt, endAt, "https://banner.png");

        campaign.Name.Should().Be("Flash Sale 12.12");
        campaign.StartAt.Should().Be(startAt);
        campaign.EndAt.Should().Be(endAt);
        campaign.BannerUrl.Should().Be("https://banner.png");
        campaign.Status.Should().Be(FlashSaleCampaignStatus.Scheduled);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_EmptyName_ShouldThrowArgumentException(string? invalidName)
    {
        var startAt = DateTimeOffset.UtcNow.AddHours(1);
        var endAt = startAt.AddHours(2);

        var act = () => new FlashSaleCampaign(invalidName!, startAt, endAt);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_EndAtBeforeOrEqualToStartAt_ShouldThrowArgumentException()
    {
        var startAt = DateTimeOffset.UtcNow.AddHours(2);
        var endAt = DateTimeOffset.UtcNow.AddHours(1);

        var act = () => new FlashSaleCampaign("Campaign", startAt, endAt);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        var startAt = DateTimeOffset.UtcNow.AddHours(1);
        var endAt = startAt.AddHours(2);
        var campaign = new FlashSaleCampaign("Campaign", startAt, endAt);

        campaign.Cancel();

        campaign.Status.Should().Be(FlashSaleCampaignStatus.Cancelled);
        campaign.IsActiveAt(startAt.AddMinutes(30)).Should().BeFalse();
    }

    [Fact]
    public void TimeHelperMethods_ShouldEvaluateCorrectly()
    {
        var startAt = DateTimeOffset.UtcNow.AddHours(1);
        var endAt = startAt.AddHours(2);
        var campaign = new FlashSaleCampaign("Campaign", startAt, endAt);

        // Before start
        campaign.IsUpcomingAt(startAt.AddMinutes(-10)).Should().BeTrue();
        campaign.IsActiveAt(startAt.AddMinutes(-10)).Should().BeFalse();
        campaign.IsEndedAt(startAt.AddMinutes(-10)).Should().BeFalse();

        // During campaign
        campaign.IsUpcomingAt(startAt.AddMinutes(30)).Should().BeFalse();
        campaign.IsActiveAt(startAt.AddMinutes(30)).Should().BeTrue();
        campaign.IsEndedAt(startAt.AddMinutes(30)).Should().BeFalse();

        // After end
        campaign.IsUpcomingAt(endAt.AddMinutes(10)).Should().BeFalse();
        campaign.IsActiveAt(endAt.AddMinutes(10)).Should().BeFalse();
        campaign.IsEndedAt(endAt.AddMinutes(10)).Should().BeTrue();
    }

    [Theory]
    [InlineData(FlashSaleCampaignStatus.Scheduled, true)]
    [InlineData(FlashSaleCampaignStatus.Active, true)]
    [InlineData(FlashSaleCampaignStatus.Ended, false)]
    [InlineData(FlashSaleCampaignStatus.Cancelled, false)]
    public void EffectiveActive_RequiresEligibleStatusAndStartInclusiveEndExclusive(FlashSaleCampaignStatus status, bool active)
    {
        var now = DateTimeOffset.UtcNow;
        var campaign = new FlashSaleCampaign("Window", now, now.AddHours(1));
        typeof(FlashSaleCampaign).GetProperty(nameof(FlashSaleCampaign.Status))!.SetValue(campaign, status);
        campaign.IsActiveAt(now).Should().Be(active);
        campaign.IsActiveAt(now.AddTicks(-1)).Should().BeFalse();
        campaign.IsActiveAt(now.AddHours(1)).Should().BeFalse();
    }
}
