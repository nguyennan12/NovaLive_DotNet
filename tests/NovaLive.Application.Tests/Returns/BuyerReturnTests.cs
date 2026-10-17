using FluentAssertions;
using NovaLive.Application.UseCases.Returns;
using NovaLive.Application.UseCases.Returns.Commands.RequestReturn;
using NovaLive.Application.UseCases.Returns.Queries.GetMyReturns;
using NovaLive.Application.UseCases.Returns.Queries.GetReturnDetail;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
namespace NovaLive.Application.Tests.Returns;
public sealed class BuyerReturnTests
{
    [Fact]
    public async Task SeventhDay_CreateHistorySnapshotRefundAndQueryableOwnReturn()
    {
        var c = new ReturnTestContext(); var result = await c.Handler.Handle(new(c.Request), default);
        result.IsSuccess.Should().BeTrue(); result.Value!.RequestedRefundAmount.Should().Be(160);
        c.Sub.Status.Should().Be(SubOrderStatus.ReturnRequested); c.Saved.Should().ContainSingle(); c.SavedLines.Should().ContainSingle();
        c.History.Should().ContainSingle().Which.ChangedByRole.Should().Be(OrderRole.Buyer);
        var detail = await new GetReturnDetailQueryHandler(c.Returns.Object, c.User.Object, c.Reader).Handle(new(c.Saved[0].Id), default);
        detail.Value!.Items.Single().SpuName.Should().Be("Snapshot");
        var page = await new GetMyReturnsQueryHandler(c.Returns.Object, c.User.Object, c.Reader).Handle(new("Pending"), default);
        page.Value!.Total.Should().Be(1); page.Value.Items.Should().ContainSingle();
        (await c.Handler.Handle(new(c.Request), default)).Error.Should().Be(ReturnErrors.ActiveReturnExists);
    }
    [Fact]
    public async Task PastSeventhDay_DoesNotWrite()
    {
        var c = new ReturnTestContext(); c.Clock.SetupGet(t => t.UtcNow).Returns(c.Now.AddTicks(1));
        (await c.Handler.Handle(new(c.Request), default)).Error.Should().Be(ReturnErrors.Expired);
        c.Saved.Should().BeEmpty(); c.Sub.Status.Should().Be(SubOrderStatus.Delivered);
    }
    [Fact]
    public async Task UndeliveredAndForeignSubOrder_AreRejected()
    {
        var c = new ReturnTestContext(); typeof(SubOrder).GetProperty(nameof(SubOrder.Status))!.SetValue(c.Sub, SubOrderStatus.Shipping);
        (await c.Handler.Handle(new(c.Request), default)).Error.Should().Be(ReturnErrors.NotDelivered);
        c.User.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        (await c.Handler.Handle(new(c.Request), default)).Error.Should().Be(ReturnErrors.SubOrderNotFound);
        c.Saved.Should().BeEmpty();
    }
    [Fact]
    public async Task ForeignItemAndExcessQuantity_AreSeparateErrors()
    {
        var c = new ReturnTestContext();
        (await c.Handler.Handle(new(c.Request with { Items = [new(Guid.NewGuid(), 1, null)] }), default)).Error.Should().Be(ReturnErrors.ItemNotInSubOrder);
        (await c.Handler.Handle(new(c.Request with { Items = [new(c.Item.Id, 5, null)] }), default)).Error.Should().Be(ReturnErrors.QuantityExceeded);
        var previous = OrderReturn.Create(c.Sub.Id, c.Buyer, ReturnReason.Defective, [], 1, c.Now);
        typeof(OrderReturn).GetProperty(nameof(OrderReturn.Status))!.SetValue(previous, ReturnStatus.Completed);
        c.Saved.Add(previous); c.SavedLines.Add(OrderReturnItem.Create(previous.Id, c.Item.Id, 3, null));
        (await c.Handler.Handle(new(c.Request), default)).Error.Should().Be(ReturnErrors.QuantityExceeded);
        c.Sub.Status.Should().Be(SubOrderStatus.Delivered);
    }
    [Fact]
    public async Task RejectedReturns_DoNotConsumeQuantity_AndForeignBuyerCannotRead()
    {
        var c = new ReturnTestContext(); var rejected = OrderReturn.Create(c.Sub.Id, c.Buyer, ReturnReason.Defective, [], 1, c.Now);
        typeof(OrderReturn).GetProperty(nameof(OrderReturn.Status))!.SetValue(rejected, ReturnStatus.SellerRejected);
        c.Saved.Add(rejected); c.SavedLines.Add(OrderReturnItem.Create(rejected.Id, c.Item.Id, 4, null));
        (await c.Handler.Handle(new(c.Request), default)).IsSuccess.Should().BeTrue();
        c.User.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        (await new GetReturnDetailQueryHandler(c.Returns.Object, c.User.Object, c.Reader).Handle(new(rejected.Id), default)).Error.Should().Be(ReturnErrors.NotFound);
    }
    [Fact]
    public void AllBuyerValidators_ValidateReasonsEvidenceItemsAndPagination()
    {
        var c = new ReturnTestContext(); var validator = new RequestReturnCommandValidator();
        validator.Validate(new RequestReturnCommand(c.Request)).IsValid.Should().BeTrue();
        foreach (var request in new[] { c.Request with { Reason = "0" }, c.Request with { Reason = "Other" }, c.Request with { EvidenceUrls = [] },
            c.Request with { EvidenceUrls = ["file:///local"] }, c.Request with { Items = [] }, c.Request with { Items = [new(c.Item.Id, 0, null)] },
            c.Request with { Items = [new(c.Item.Id, 1, null), new(c.Item.Id, 1, null)] } })
            validator.Validate(new RequestReturnCommand(request)).IsValid.Should().BeFalse();
        new GetMyReturnsQueryValidator().Validate(new GetMyReturnsQuery("Pending", 1, 10)).IsValid.Should().BeTrue();
        new GetMyReturnsQueryValidator().Validate(new GetMyReturnsQuery("0", 0, 101)).Errors.Should().HaveCount(3);
        new GetReturnDetailQueryValidator().Validate(new GetReturnDetailQuery(Guid.Empty)).IsValid.Should().BeFalse();
    }
}
