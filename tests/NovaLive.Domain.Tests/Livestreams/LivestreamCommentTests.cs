using FluentAssertions;
using NovaLive.Domain.Livestreams;
using Xunit;

namespace NovaLive.Domain.Tests.Livestreams;

public class LivestreamCommentTests
{
    [Fact]
    public void Constructor_WithValidData_InitializesPropertiesCorrectly()
    {
        var sessionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var displayName = "Nguyễn Văn A";
        var content = "Sản phẩm này còn size M không shop?";
        var isQuestion = true;
        var createdAt = DateTimeOffset.UtcNow;

        var comment = new LivestreamComment(
            sessionId,
            userId,
            displayName,
            content,
            isQuestion,
            createdAt);

        comment.Id.Should().NotBeEmpty();
        comment.SessionId.Should().Be(sessionId);
        comment.UserId.Should().Be(userId);
        comment.DisplayName.Should().Be(displayName);
        comment.Content.Should().Be(content);
        comment.IsQuestion.Should().BeTrue();
        comment.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Constructor_WithCustomId_PreservesProvidedId()
    {
        var customId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();

        var comment = new LivestreamComment(
            sessionId,
            null,
            "Khách",
            "Shop có mã giảm giá không?",
            isQuestion: false,
            createdAt: null,
            id: customId);

        comment.Id.Should().Be(customId);
        comment.UserId.Should().BeNull();
        comment.DisplayName.Should().Be("Khách");
        comment.IsQuestion.Should().BeFalse();
        comment.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }
}
