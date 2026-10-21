using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Tests.UseCases.Orders;
using NovaLive.Application.UseCases.Inventory;
using NovaLive.Application.UseCases.Inventory.Commands.ConfirmInventorySale;
using NovaLive.Domain.Common;
using NovaLive.Domain.Inventory;
using NovaLive.Domain.Payments;

namespace NovaLive.Application.Tests.Inventory;

internal sealed class InventorySaleTestContext
{
    public OrderOperationsTestContext Orders { get; private set; } = null!;
    public CheckoutCommandTestContext Checkout => Orders.Checkout;
    public Payment Payment => Checkout.PaymentList.Single();
    public Mock<IInventoryHistoryRepository> Histories { get; } = new();
    public Mock<IFlashSaleStockService> FlashSales { get; } = new();
    public List<Guid> ChangedSkus { get; } = [];
    public Guid? FailSku { get; set; }

    public static async Task<InventorySaleTestContext> CreateAsync(bool paid = true)
    {
        var context = new InventorySaleTestContext { Orders = await OrderOperationsTestContext.Create(shops: 2) };
        if (paid) context.Payment.MarkSuccess("paid", context.Checkout.Draft.Now).IsSuccess.Should().BeTrue();
        context.Checkout.Payments.Setup(p => p.GetByIdAsync(context.Payment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(context.Payment);
        context.Histories.Setup(h => h.ExistsByReferenceAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<InventoryChangeType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string type, Guid id, Guid sku, InventoryChangeType change, CancellationToken _) =>
                context.Checkout.Ledger.Any(h => h.RefType == type && h.RefId == id && h.SkuId == sku && h.ChangeType == change));
        context.Histories.Setup(h => h.AddAsync(It.IsAny<InventoryHistory>(), It.IsAny<CancellationToken>()))
            .Callback((InventoryHistory h, CancellationToken _) => context.Checkout.Ledger.Add(h)).Returns(Task.CompletedTask);
        context.Checkout.Draft.Inventories.Setup(i => i.ConfirmReservedSaleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<int>(), It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid sku, Guid shop, int quantity, DateTimeOffset now, CancellationToken _) =>
                context.ChangeStock(sku, quantity, true, now));
        context.FlashSales.Setup(f => f.ConfirmSoldAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        return context;
    }

    public Result<InventoryStockResult> ChangeStock(Guid sku, int quantity, bool sale, DateTimeOffset now)
    {
        ChangedSkus.Add(sku);
        if (sku == FailSku) return InventoryErrors.ReservationMismatch;
        var stock = Checkout.Draft.Stocks[sku];
        var result = sale ? stock.TryConfirmSale(quantity, now) : stock.TryRelease(quantity, now);
        if (result.IsFailure) return result.Error;
        return new InventoryStockResult(stock.Id, sku, stock.QtyOnHand, stock.ReservedQty,
            stock.AvailableQty, stock.MinStock, stock.Version);
    }

    public ConfirmInventorySaleCommandHandler Handler => new(Checkout.Payments.Object, Checkout.Parents.Object,
        Checkout.Subs.Object, Checkout.Items.Object, Checkout.Draft.Inventories.Object, Histories.Object,
        FlashSales.Object, Checkout.Draft.Clock.Object, NullLogger<ConfirmInventorySaleCommandHandler>.Instance);
}
