using Moq;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Application.UseCases.Orders.Commands.Checkout;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.System;
using NovaLive.Domain.Discounts;
namespace NovaLive.Application.Tests.UseCases.Orders;
public sealed class CheckoutCommandTestContext
{
    // Reuse read setups; its Dispose asserts preview-only writes, so it is not invoked for command tests.
    public CheckoutTestContext Draft { get; } = new();
    public Mock<ITransactionManager> Transactions { get; } = new(MockBehavior.Strict);
    public Mock<IParentOrderRepository> Parents { get; } = new();
    public Mock<ISubOrderRepository> Subs { get; } = new();
    public Mock<IOrderItemRepository> Items { get; } = new();
    public Mock<IOrderStatusHistoryRepository> Histories { get; } = new();
    public Mock<IPaymentRepository> Payments { get; } = new();
    public Mock<IOutboxRepository> Outbox { get; } = new();
    public List<ParentOrder> ParentList { get; } = [];
    public List<SubOrder> SubList { get; } = [];
    public List<OrderItem> ItemList { get; } = [];
    public List<OrderStatusHistory> HistoryList { get; } = [];
    public List<InventoryHistory> Ledger { get; } = [];
    public List<Payment> PaymentList { get; } = [];
    public List<OutboxMessage> Messages { get; } = [];
    public List<DiscountUsage> Usages { get; } = [];
    public bool Committed { get; private set; }
    public CheckoutCommandTestContext()
    {
        Draft.Shipping.SetupGet(s => s.ProviderCode).Returns("GHN");
        Draft.Carts.Setup(c => c.LockUserAsync(Draft.UserId, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        Draft.Carts.Setup(c => c.RemoveItems(It.IsAny<IEnumerable<NovaLive.Domain.Carts.CartItem>>()));
        Draft.Inventories.Setup(i => i.GetByIdsForUpdateAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => Draft.Stocks.Values.Where(i => ids.Contains(i.SkuId)).ToList());
        Draft.Inventories.Setup(i => i.AddHistoryAsync(It.IsAny<InventoryHistory>(), It.IsAny<CancellationToken>()))
            .Callback((InventoryHistory h, CancellationToken _) => Ledger.Add(h)).Returns(Task.CompletedTask);
        Parents.Setup(p => p.TryAddAsync(It.IsAny<ParentOrder>(), It.IsAny<CancellationToken>()))
            .Callback((ParentOrder p, CancellationToken _) => ParentList.Add(p)).ReturnsAsync(true);
        Subs.Setup(p => p.AddAsync(It.IsAny<SubOrder>(), It.IsAny<CancellationToken>()))
            .Callback((SubOrder p, CancellationToken _) => SubList.Add(p)).Returns(Task.CompletedTask);
        Items.Setup(p => p.AddAsync(It.IsAny<OrderItem>(), It.IsAny<CancellationToken>()))
            .Callback((OrderItem p, CancellationToken _) => ItemList.Add(p)).Returns(Task.CompletedTask);
        Histories.Setup(p => p.AddAsync(It.IsAny<OrderStatusHistory>(), It.IsAny<CancellationToken>()))
            .Callback((OrderStatusHistory p, CancellationToken _) => HistoryList.Add(p)).Returns(Task.CompletedTask);
        Payments.Setup(p => p.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback((Payment p, CancellationToken _) => PaymentList.Add(p)).Returns(Task.CompletedTask);
        Outbox.Setup(p => p.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Callback((OutboxMessage p, CancellationToken _) => Messages.Add(p)).Returns(Task.CompletedTask);
        Draft.Discounts.Setup(d => d.TryConsumeAsync(It.IsAny<Guid>(), Draft.UserId, Draft.Now, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        Draft.Discounts.Setup(d => d.AddUsageAsync(It.IsAny<DiscountUsage>(), It.IsAny<CancellationToken>()))
            .Callback((DiscountUsage d, CancellationToken _) => Usages.Add(d)).Returns(Task.CompletedTask);
        Transactions.Setup(t => t.ExecuteAsync(It.IsAny<Func<Task<Result<CheckoutResponse>>>>(), false, It.IsAny<CancellationToken>()))
            .Returns(async (Func<Task<Result<CheckoutResponse>>> action, bool _, CancellationToken _) =>
            { var result = await action(); Committed = result.IsSuccess; return result; });
    }
    public CheckoutRequest Request => new(Draft.Request.CartItemIds, Draft.Address.Id,
        Draft.ShopList.Select(s => new ShopShippingSelectionDto(s.Id, "GHN", "2")).ToArray(), null, null, null, "MoMo", null);
    public CheckoutCommandHandler Handler => new(new CheckoutDraftBuilder(Draft.Carts.Object, Draft.Skus.Object,
        Draft.Spus.Object, Draft.Shops.Object, Draft.Inventories.Object, Draft.Addresses.Object,
        Draft.Warehouses.Object, Draft.Discounts.Object, Draft.Shipping.Object, Draft.User.Object, Draft.Clock.Object),
        Transactions.Object, Draft.Carts.Object, Draft.Inventories.Object, Parents.Object, Subs.Object,
        Items.Object, Histories.Object, Payments.Object, Draft.Discounts.Object, Outbox.Object, Draft.User.Object, Draft.Clock.Object);
    public Task<Result<CheckoutResponse>> Run(CheckoutRequest? request = null) => Handler.Handle(new(request ?? Request, "test-key"), default);
}
