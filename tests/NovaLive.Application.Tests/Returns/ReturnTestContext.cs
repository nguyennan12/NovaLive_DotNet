using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Returns;
using NovaLive.Application.UseCases.Returns.Commands.RequestReturn;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;
namespace NovaLive.Application.Tests.Returns;
public sealed class ReturnTestContext
{
    public DateTimeOffset Now { get; } = DateTimeOffset.Parse("2026-10-17T00:00:00Z");
    public Guid Buyer { get; } = Guid.NewGuid();
    public Mock<ICurrentUser> User { get; } = new();
    public Mock<IDateTimeProvider> Clock { get; } = new();
    public Mock<IParentOrderRepository> Parents { get; } = new();
    public Mock<ISubOrderRepository> Subs { get; } = new();
    public Mock<IOrderItemRepository> Items { get; } = new();
    public Mock<IOrderReturnRepository> Returns { get; } = new();
    public Mock<IOrderReturnItemRepository> Lines { get; } = new();
    public Mock<IOrderStatusHistoryRepository> Histories { get; } = new();
    public List<OrderReturn> Saved { get; } = [];
    public List<OrderReturnItem> SavedLines { get; } = [];
    public List<OrderStatusHistory> History { get; } = [];
    public ParentOrder Parent { get; }
    public SubOrder Sub { get; }
    public OrderItem Item { get; }
    public CreateReturnRequest Request => new(Sub.Id, "NotAsDescribed", [new(Item.Id, 2, "Mismatch")], ["https://example.com/evidence.jpg"]);
    public RequestReturnCommandHandler Handler => new(Parents.Object, Subs.Object, Items.Object, Returns.Object, Lines.Object, Histories.Object, User.Object, Clock.Object);
    public ReturnReader Reader => new(Subs.Object, Lines.Object, Items.Object);
    public ReturnTestContext()
    {
        User.SetupGet(u => u.IsAuthenticated).Returns(true); User.SetupGet(u => u.UserId).Returns(Buyer);
        Clock.SetupGet(c => c.UtcNow).Returns(Now);
        Parent = ParentOrder.Create(Buyer, "PARENT", "{}", 360, 10, 40, 370, null, Now.AddDays(-8));
        Sub = SubOrder.Create(Parent.Id, "SUB", Guid.NewGuid(), new ShopCalculationResult(Guid.NewGuid(), 400, 10, 0, 40, 0, 360, 370, []), true, Now.AddDays(-8));
        Sub.MarkAsShipping(Now.AddDays(-8)); Sub.MarkAsDelivered(Now.AddDays(-7));
        Item = OrderItem.Create(Sub.Id, """{"spuName":"Snapshot","skuCode":"SKU-1"}""", 100,
            new(Guid.NewGuid(), Guid.NewGuid(), 4, 100, 400, 0, 40, 40, 360));
        Parents.Setup(p => p.GetForUpdateAsync(Parent.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Parent);
        Subs.Setup(s => s.FindByIdAsync(Sub.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Sub);
        Subs.Setup(s => s.GetForUpdateAsync(Sub.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Sub);
        Subs.Setup(s => s.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([Sub]);
        Items.Setup(i => i.GetBySubOrderIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([Item]);
        Returns.Setup(r => r.GetBySubOrderIdAsync(Sub.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Saved);
        Returns.Setup(r => r.FindAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => Saved.SingleOrDefault(r => r.Id == id));
        Returns.Setup(r => r.AddAsync(It.IsAny<OrderReturn>(), It.IsAny<CancellationToken>()))
            .Callback((OrderReturn r, CancellationToken _) => Saved.Add(r)).Returns(Task.CompletedTask);
        Lines.Setup(l => l.GetByReturnIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IEnumerable<Guid> ids, CancellationToken _) => SavedLines.Where(l => ids.Contains(l.ReturnId)).ToList());
        Lines.Setup(l => l.AddAsync(It.IsAny<OrderReturnItem>(), It.IsAny<CancellationToken>()))
            .Callback((OrderReturnItem l, CancellationToken _) => SavedLines.Add(l)).Returns(Task.CompletedTask);
        Histories.Setup(h => h.AddAsync(It.IsAny<OrderStatusHistory>(), It.IsAny<CancellationToken>()))
            .Callback((OrderStatusHistory h, CancellationToken _) => History.Add(h)).Returns(Task.CompletedTask);
        Returns.Setup(r => r.GetBuyerPagedAsync(Buyer, It.IsAny<ReturnStatus?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid _, ReturnStatus? status, int page, int size, CancellationToken _) =>
            { var rows = Saved.Where(r => status == null || r.Status == status).ToList(); return (rows.Skip((page - 1) * size).Take(size).ToList(), (long)rows.Count); });
    }
}
