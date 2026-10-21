using System.Text.Json;
using FluentAssertions;
using Moq;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Reviews;
using NovaLive.Domain.System;

namespace NovaLive.Application.Tests.Reviews;

public sealed class ReviewCommandTests
{
    [Fact]
    public async Task Create_SuccessAssignsPurchaseIdentityAndMediaAndMasksBuyer()
    {
        var c = new ReviewTestContext();
        var result = await c.CreateHandler.Handle(new(c.CreateRequest), default);
        result.IsSuccess.Should().BeTrue();
        var saved = c.Saved.Should().ContainSingle().Subject;
        saved.OrderItemId.Should().Be(c.Item.Id);
        saved.SkuId.Should().Be(c.Item.SkuId);
        saved.ShopId.Should().Be(c.Sub.ShopId);
        saved.BuyerId.Should().Be(c.Buyer);
        saved.Images.Should().HaveCount(2);
        result.Value!.SpuId.Should().Be(c.Sku.SpuId);
        result.Value.BuyerName.Should().Be("N*** V*** B***");
        result.Value.BuyerAvatarUrl.Should().BeNull();
        result.Value.MediaUrls.Should().BeEquivalentTo(c.CreateRequest.MediaUrls);
    }

    [Theory]
    [InlineData("foreign")]
    [InlineData("missing-item")]
    [InlineData("missing-sub")]
    [InlineData("missing-parent")]
    public async Task Create_MissingOrForeignPurchaseHasSameNotFound(string scenario)
    {
        var c = new ReviewTestContext();
        if (scenario == "foreign") c.User.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        if (scenario == "missing-item") c.Items.Setup(r => r.GetByIdAsync(c.Item.Id, default)).ReturnsAsync((OrderItem?)null);
        if (scenario == "missing-sub") c.Subs.Setup(r => r.FindByIdAsync(c.Sub.Id, default)).ReturnsAsync((SubOrder?)null);
        if (scenario == "missing-parent") c.Parents.Setup(r => r.FindAsync(c.Parent.Id, c.Buyer, default)).ReturnsAsync((ParentOrder?)null);
        (await c.CreateHandler.Handle(new(c.CreateRequest), default)).Error.Should().Be(ReviewErrors.OrderItemNotFound);
        c.Saved.Should().BeEmpty();
    }

    [Theory]
    [InlineData(SubOrderStatus.PendingPayment)]
    [InlineData(SubOrderStatus.Shipping)]
    [InlineData(SubOrderStatus.Cancelled)]
    [InlineData(SubOrderStatus.ReturnRequested)]
    [InlineData(SubOrderStatus.Returned)]
    [InlineData(SubOrderStatus.Refunded)]
    public async Task Create_RejectsIneligibleOrderStatus(SubOrderStatus status)
    {
        var c = new ReviewTestContext();
        typeof(SubOrder).GetProperty(nameof(SubOrder.Status))!.SetValue(c.Sub, status);
        (await c.CreateHandler.Handle(new(c.CreateRequest), default)).Error.Should().Be(ReviewErrors.NotDelivered);
        c.Saved.Should().BeEmpty();
    }

    [Fact]
    public async Task Create_AllowsCompletedButRejectsReturnedItem()
    {
        var c = new ReviewTestContext();
        typeof(SubOrder).GetProperty(nameof(SubOrder.Status))!.SetValue(c.Sub, SubOrderStatus.Completed);
        (await c.CreateHandler.Handle(new(c.CreateRequest), default)).IsSuccess.Should().BeTrue();
        typeof(OrderItem).GetProperty(nameof(OrderItem.Status))!.SetValue(c.Item, OrderItemStatus.Returned);
        (await c.CreateHandler.Handle(new(c.CreateRequest), default)).Error.Should().Be(ReviewErrors.NotDelivered);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Create_PrecheckAndUniqueRaceReturnAlreadyReviewed(bool exists)
    {
        var c = new ReviewTestContext();
        c.Reviews.Setup(r => r.ExistsByOrderItemAsync(c.Item.Id, default)).ReturnsAsync(exists);
        c.Reviews.Setup(r => r.TryAddAsync(It.IsAny<Review>(), default)).ReturnsAsync(false);
        var result = await c.CreateHandler.Handle(new(c.CreateRequest), default);
        result.Error.Should().Be(ReviewErrors.AlreadyReviewed);
        c.Reviews.Verify(r => r.TryAddAsync(It.IsAny<Review>(), default), exists ? Times.Never() : Times.Once());
    }

    [Fact]
    public async Task Create_RejectsUnknownExtensionEvenWithoutValidationPipeline()
    {
        var c = new ReviewTestContext();
        var result = await c.CreateHandler.Handle(new(c.CreateRequest with { MediaUrls = ["https://cdn.example.com/x.exe"] }), default);
        result.Error.Should().Be(ReviewErrors.InvalidMedia);
        c.Saved.Should().BeEmpty();
    }

    [Fact]
    public async Task Update_SuccessReplacesAllMediaAndRejectsSecondEdit()
    {
        var c = new ReviewTestContext();
        var result = await c.UpdateHandler.Handle(new(c.Review.Id, c.UpdateRequest), default);
        result.IsSuccess.Should().BeTrue();
        result.Value!.MediaUrls.Should().Equal(c.UpdateRequest.MediaUrls!);
        c.Review.EditCount.Should().Be(1);
        (await c.UpdateHandler.Handle(new(c.Review.Id, c.UpdateRequest), default)).Error.Should().Be(ReviewErrors.AlreadyEdited);
    }

    [Theory]
    [InlineData("expired")]
    [InlineData("hidden")]
    [InlineData("foreign")]
    [InlineData("missing")]
    public async Task Update_RejectsInvalidStateOrOwnership(string scenario)
    {
        var c = new ReviewTestContext();
        var expected = ReviewErrors.NotFound;
        var id = c.Review.Id;
        if (scenario == "expired") { c.Clock.SetupGet(t => t.UtcNow).Returns(c.Now.AddDays(30).AddTicks(1)); expected = ReviewErrors.EditExpired; }
        if (scenario == "hidden") { c.Review.Hide("spam", c.Now); expected = ReviewErrors.Hidden; }
        if (scenario == "foreign") c.User.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        if (scenario == "missing") id = Guid.NewGuid();
        (await c.UpdateHandler.Handle(new(id, c.UpdateRequest), default)).Error.Should().Be(expected);
        c.Review.EditCount.Should().Be(0);
    }

    [Fact]
    public async Task Reply_OnceIncludingHiddenReview()
    {
        var c = new ReviewTestContext();
        c.User.SetupGet(u => u.UserId).Returns(c.Seller);
        c.Review.Hide("spam", c.Now);
        var result = await c.ReplyHandler.Handle(new(c.Review.Id, new("Thanks")), default);
        result.IsSuccess.Should().BeTrue();
        result.Value!.SellerReply.Should().Be("Thanks");
        (await c.ReplyHandler.Handle(new(c.Review.Id, new("Again")), default)).Error.Should().Be(ReviewErrors.AlreadyReplied);
    }

    [Theory]
    [InlineData("foreign-shop")]
    [InlineData("spoofed-shop")]
    [InlineData("missing")]
    public async Task Reply_HidesForeignAndMissingResources(string scenario)
    {
        var c = new ReviewTestContext();
        c.User.SetupGet(u => u.UserId).Returns(c.Seller);
        if (scenario == "foreign-shop") c.CurrentShop.SetupGet(s => s.ShopId).Returns(Guid.NewGuid());
        if (scenario == "spoofed-shop") c.User.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        var id = scenario == "missing" ? Guid.NewGuid() : c.Review.Id;
        (await c.ReplyHandler.Handle(new(id, new("Reply")), default)).Error.Should().Be(ReviewErrors.NotFound);
        c.Review.SellerReply.Should().BeNull();
    }

    [Fact]
    public async Task Hide_WritesAuditOnceWithAdminReasonAndBothStates()
    {
        var c = new ReviewTestContext();
        AuditLog? log = null;
        c.Audit.Setup(r => r.AddAsync(It.IsAny<AuditLog>(), default)).Callback((AuditLog value, CancellationToken _) => log = value);
        (await c.HideHandler.Handle(new(c.Review.Id, new("Spam")), default)).IsSuccess.Should().BeTrue();
        log.Should().NotBeNull();
        log!.UserId.Should().Be(c.Buyer);
        log.Action.Should().Be("Review.Hidden");
        log.EntityType.Should().Be("Review");
        log.EntityId.Should().Be(c.Review.Id);
        using var oldData = JsonDocument.Parse(log.OldDataJson!);
        using var newData = JsonDocument.Parse(log.NewDataJson!);
        oldData.RootElement.GetProperty("is_visible").GetBoolean().Should().BeTrue();
        newData.RootElement.GetProperty("is_visible").GetBoolean().Should().BeFalse();
        newData.RootElement.GetProperty("reason").GetString().Should().Be("Spam");
        oldData.RootElement.GetProperty("reason").GetString().Should().Be("Spam");
        (await c.HideHandler.Handle(new(c.Review.Id, new("Again")), default)).IsSuccess.Should().BeTrue();
        c.Audit.Verify(r => r.AddAsync(It.IsAny<AuditLog>(), default), Times.Once());
    }

    [Fact]
    public async Task Hide_MissingReviewDoesNotWriteAudit()
    {
        var c = new ReviewTestContext();
        (await c.HideHandler.Handle(new(Guid.NewGuid(), new("Spam")), default)).Error.Should().Be(ReviewErrors.NotFound);
        c.Audit.Verify(r => r.AddAsync(It.IsAny<AuditLog>(), default), Times.Never());
    }
}
