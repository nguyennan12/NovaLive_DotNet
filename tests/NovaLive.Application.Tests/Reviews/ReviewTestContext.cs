using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.UseCases.Reviews;
using NovaLive.Application.UseCases.Reviews.Commands.CreateReview;
using NovaLive.Application.UseCases.Reviews.Commands.UpdateReview;
using NovaLive.Application.UseCases.Reviews.Commands.ReplyReview;
using NovaLive.Application.UseCases.Reviews.Commands.HideReview;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Products;
using NovaLive.Domain.Reviews;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;

namespace NovaLive.Application.Tests.Reviews;

public sealed class ReviewTestContext
{
    public DateTimeOffset Now { get; } = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
    public Guid Buyer { get; } = Guid.NewGuid();
    public Guid Seller { get; } = Guid.NewGuid();
    public Mock<ICurrentUser> User { get; } = new();
    public Mock<ICurrentShop> CurrentShop { get; } = new();
    public Mock<IDateTimeProvider> Clock { get; } = new();
    public Mock<IOrderItemRepository> Items { get; } = new();
    public Mock<ISubOrderRepository> Subs { get; } = new();
    public Mock<IParentOrderRepository> Parents { get; } = new();
    public Mock<IReviewRepository> Reviews { get; } = new();
    public Mock<ISkuRepository> Skus { get; } = new();
    public Mock<IUserRepository> Users { get; } = new();
    public Mock<IShopRepository> Shops { get; } = new();
    public Mock<IAuditLogRepository> Audit { get; } = new();
    public SubOrder Sub { get; }
    public OrderItem Item { get; }
    public Sku Sku { get; }
    public Shop Shop { get; }
    public ParentOrder Parent { get; }
    public Review Review { get; }
    public List<Review> Saved { get; } = [];
    public ReviewResponseReader Reader => new(Skus.Object, Users.Object);
    public CreateReviewCommandHandler CreateHandler => new(Items.Object, Subs.Object, Parents.Object, Reviews.Object, User.Object, Clock.Object, Reader);
    public UpdateReviewCommandHandler UpdateHandler => new(Reviews.Object, User.Object, Clock.Object, Reader);
    public ReplyReviewCommandHandler ReplyHandler => new(Reviews.Object, CurrentShop.Object, User.Object, Shops.Object, Clock.Object, Reader);
    public HideReviewCommandHandler HideHandler => new(Reviews.Object, Audit.Object, User.Object, Clock.Object);
    public CreateReviewRequest CreateRequest => new(Item.Id, 5, "Title", "Content",
        ["https://cdn.example.com/reviews/a.jpg", "https://cdn.example.com/reviews/b.mp4"]);
    public UpdateReviewRequest UpdateRequest => new(2, "Updated", "New content", ["https://cdn.example.com/reviews/new.webp"]);

    public ReviewTestContext()
    {
        User.SetupGet(u => u.UserId).Returns(Buyer);
        Clock.SetupGet(c => c.UtcNow).Returns(Now);
        Shop = new Shop(Seller, "Shop", "shop");
        CurrentShop.SetupGet(s => s.ShopId).Returns(Shop.Id);
        Shops.Setup(s => s.GetByOwnerIdAsync(Seller, It.IsAny<CancellationToken>())).ReturnsAsync(Shop);
        Sku = new Sku(Guid.NewGuid(), Shop.Id, "SKU", "{}", 100, 100);
        Parent = ParentOrder.Create(Buyer, "PARENT", "{}", 100, 0, 0, 100, null, Now.AddDays(-1));
        Sub = SubOrder.Create(Parent.Id, "SUB", Guid.NewGuid(),
            new ShopCalculationResult(Shop.Id, 100, 0, 0, 0, 0, 100, 100, []), true, Now.AddDays(-1));
        Sub.MarkAsShipping(Now.AddHours(-2));
        Sub.MarkAsDelivered(Now.AddHours(-1));
        Item = OrderItem.Create(Sub.Id, "{}", 100, new OrderItemCalculationResult(Sku.Id, Sku.SpuId, 1, 100, 100, 0, 0, 0, 100));
        Review = Review.Create(Item.Id, Sku.Id, Shop.Id, Buyer, 5, "Old", "Old", ["https://cdn.example.com/reviews/old.png"], Now).Value!;
        Items.Setup(r => r.GetByIdAsync(Item.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Item);
        Subs.Setup(r => r.FindByIdAsync(Sub.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Sub);
        Parents.Setup(r => r.FindAsync(Parent.Id, It.IsAny<Guid?>(), It.IsAny<CancellationToken>())).ReturnsAsync(Parent);
        Reviews.Setup(r => r.GetForUpdateAsync(Review.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Review);
        Reviews.Setup(r => r.TryAddAsync(It.IsAny<Review>(), It.IsAny<CancellationToken>()))
            .Callback((Review review, CancellationToken _) => Saved.Add(review)).ReturnsAsync(true);
        Skus.Setup(r => r.GetByIdAsync(Sku.Id, It.IsAny<CancellationToken>())).ReturnsAsync(Sku);
        Skus.Setup(r => r.GetBySpuIdAsync(Sku.SpuId, It.IsAny<CancellationToken>())).ReturnsAsync([Sku]);
        Users.Setup(r => r.GetByIdAsync(Buyer, It.IsAny<CancellationToken>())).ReturnsAsync(new User("private@example.com", "hash", "Nguyễn Văn Buyer", "0901234567"));
        Users.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>())).ReturnsAsync([]);
    }
}
