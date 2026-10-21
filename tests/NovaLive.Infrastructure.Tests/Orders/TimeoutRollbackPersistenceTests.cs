using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Inventory;
using NovaLive.Application.UseCases.Orders.Commands.ExpireUnpaidPayment;
using NovaLive.Domain.Common;
using NovaLive.Domain.Payments;
using NovaLive.Infrastructure.Tests.Auth;
using NovaLive.Infrastructure.Tests.Inventory;

namespace NovaLive.Infrastructure.Tests.Orders;

public sealed class TimeoutRollbackPersistenceTests
{
    [Fact]
    public async Task MidTransactionFailure_RollsBackPaymentOrdersStockHistoriesAndOutbox_ThenRetrySucceeds()
    {
        var flash = new Mock<IFlashSaleStockService>();
        flash.SetupSequence(f => f.ReleaseReservedAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(InventoryErrors.ReservationMismatch)).ReturnsAsync(Result.Success());
        await using var host = new AuthTestHost();
        await host.InitializeAsync(s => { InventoryLifecycleFixture.Register(s); s.AddSingleton(flash.Object); });
        var payment = await InventoryLifecycleFixture.SeedAsync(host, paid: false);
        var command = new ExpireUnpaidPaymentCommand(payment.Id);
        (await host.Send(command)).IsFailure.Should().BeTrue();
        await host.WithDb(async db =>
        {
            (await db.Payments.SingleAsync()).Status.Should().Be(PaymentStatus.Pending);
            (await db.ParentOrders.SingleAsync()).PaymentStatus.Should().Be(ParentOrderPaymentStatus.Pending);
            (await db.SubOrders.ToListAsync()).Should().OnlyContain(s => s.Status == SubOrderStatus.PendingPayment && s.CancelledAt == null);
            (await db.Inventories.ToListAsync()).Should().OnlyContain(i => i.QtyOnHand == 10 && i.ReservedQty == 2);
            (await db.InventoryHistories.CountAsync()).Should().Be(0);
            (await db.OrderStatusHistories.CountAsync()).Should().Be(0);
            (await db.OutboxMessages.CountAsync()).Should().Be(0);
        });
        (await host.Send(command)).IsSuccess.Should().BeTrue();
        (await host.Send(command)).IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            (await db.Payments.SingleAsync()).Status.Should().Be(PaymentStatus.Expired);
            (await db.ParentOrders.SingleAsync()).PaymentStatus.Should().Be(ParentOrderPaymentStatus.Cancelled);
            (await db.SubOrders.ToListAsync()).Should().OnlyContain(s => s.Status == SubOrderStatus.Cancelled
                && s.CancelReason == "PaymentTimeout" && s.CancelledBy == null);
            (await db.Inventories.ToListAsync()).Should().OnlyContain(i => i.QtyOnHand == 10 && i.ReservedQty == 0);
            (await db.InventoryHistories.CountAsync()).Should().Be(2);
            (await db.OrderStatusHistories.CountAsync()).Should().Be(2);
            (await db.OutboxMessages.CountAsync()).Should().Be(1);
        });
    }

    [Theory]
    [InlineData(PaymentMethod.COD, false)]
    [InlineData(PaymentMethod.MoMo, true)]
    public async Task CodOrSuccessfulPayment_CannotBeExpired(PaymentMethod method, bool paid)
    {
        await using var host = new AuthTestHost(); await host.InitializeAsync(InventoryLifecycleFixture.Register);
        var payment = await InventoryLifecycleFixture.SeedAsync(host, paid, method);
        (await host.Send(new ExpireUnpaidPaymentCommand(payment.Id))).IsSuccess.Should().BeTrue();
        await host.WithDb(async db =>
        {
            (await db.Payments.SingleAsync()).Status.Should().Be(paid ? PaymentStatus.Success : PaymentStatus.Pending);
            (await db.Inventories.ToListAsync()).Should().OnlyContain(i => i.ReservedQty == 2 && i.QtyOnHand == 10);
            (await db.InventoryHistories.CountAsync()).Should().Be(0);
        });
    }
}
