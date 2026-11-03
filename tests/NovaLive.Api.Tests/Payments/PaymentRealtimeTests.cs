using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NovaLive.Api.Hubs;
using NovaLive.Api.Services;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Payments;
using NovaLive.Application.UseCases.Payments.Queries.GetPaymentStatus;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;

namespace NovaLive.Api.Tests.Payments;

public sealed class PaymentRealtimeTests
{
    [Fact]
    public async Task JoinPayment_ForAnotherBuyer_DoesNotSubscribe()
    {
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<GetPaymentStatusQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<PaymentStatusResponse>(PaymentErrors.PaymentNotFound));
        var groups = new Mock<IGroupManager>(MockBehavior.Strict);
        var hub = new PaymentNotificationHub(sender.Object) { Context = Mock.Of<HubCallerContext>(), Groups = groups.Object };
        await FluentActions.Awaiting(() => hub.JoinPayment(Guid.NewGuid())).Should().ThrowAsync<HubException>();
        groups.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task JoinPayment_ForOwner_SubscribesConnection()
    {
        var id = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(new GetPaymentStatusQuery(id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PaymentStatusResponse(id, Guid.NewGuid(), "VietQR", 100, "Pending", null)));
        var context = new Mock<HubCallerContext>(); context.SetupGet(c => c.ConnectionId).Returns("buyer-connection");
        var groups = new Mock<IGroupManager>();
        var hub = new PaymentNotificationHub(sender.Object) { Context = context.Object, Groups = groups.Object };
        await hub.JoinPayment(id);
        groups.Verify(g => g.AddToGroupAsync("buyer-connection", $"payment_{id:N}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConnectionWithoutValidBuyer_IsAborted()
    {
        var context = new Mock<HubCallerContext>();
        var hub = new PaymentNotificationHub(Mock.Of<ISender>()) { Context = context.Object };
        await FluentActions.Awaiting(() => hub.OnConnectedAsync()).Should().ThrowAsync<HubException>();
        context.Verify(c => c.Abort(), Times.Once);
    }

    [Fact]
    public async Task Notification_IsSentOnlyToPersistedBuyer_WithOrderAmountAndMethod()
    {
        var buyer = Guid.NewGuid();
        var order = ParentOrder.Create(buyer, "NOVA-1", "{}", 100, 0, 0, 100, null, DateTimeOffset.UtcNow);
        var orders = new Mock<IParentOrderRepository>();
        orders.Setup(o => o.FindAsync(order.Id, null, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        var proxy = new Mock<IClientProxy>();
        var clients = new Mock<IHubClients>(MockBehavior.Strict);
        clients.Setup(c => c.User(buyer.ToString())).Returns(proxy.Object);
        var hub = new Mock<IHubContext<PaymentNotificationHub>>(); hub.SetupGet(h => h.Clients).Returns(clients.Object);
        var payment = new PaymentSuccessEvent(Guid.NewGuid(), order.Id, order.OrderCode, 100, "VietQR", "TX", DateTimeOffset.UtcNow);
        await new PaymentNotificationService(hub.Object, orders.Object).NotifyPaymentSuccessAsync(payment);
        proxy.Verify(p => p.SendCoreAsync("PaymentSuccess", It.Is<object?[]>(args => args.Length == 1 &&
            args[0] is PaymentSuccessNotification && ((PaymentSuccessNotification)args[0]!).OrderId == order.Id &&
            ((PaymentSuccessNotification)args[0]!).Amount == 100 && ((PaymentSuccessNotification)args[0]!).PaymentMethod == "VietQR"),
            It.IsAny<CancellationToken>()), Times.Once);
        clients.VerifyNoOtherCalls();
    }
}
