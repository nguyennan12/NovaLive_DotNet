using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Dashboards.Seller.GetSellerDashboard;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Contracts.V1.Dashboards;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shops;

namespace NovaLive.Application.Tests.Dashboards.Seller;

public sealed class GetSellerDashboardHandlerTests
{
    private readonly Mock<ISellerDashboardReadRepository> _readRepo = new();
    private readonly Mock<IShopRepository> _shopRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly ShopAccess _shopAccess;
    private readonly GetSellerDashboardQueryHandler _handler;

    public GetSellerDashboardHandlerTests()
    {
        _shopAccess = new ShopAccess(_shopRepo.Object, _currentUser.Object);
        _handler = new GetSellerDashboardQueryHandler(_readRepo.Object, _shopAccess);
    }

    [Fact]
    public async Task Handle_WhenUserNotAuthenticated_ReturnsUnauthorizedError()
    {
        _currentUser.SetupGet(x => x.IsAuthenticated).Returns(false);

        var query = new GetSellerDashboardQuery();
        var result = await _handler.Handle(query, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ShopErrors.Unauthorized.Code);
    }

    [Fact]
    public async Task Handle_WhenShopNotFound_ReturnsNotFoundError()
    {
        var userId = Guid.NewGuid();
        _currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        _currentUser.SetupGet(x => x.UserId).Returns(userId);
        _shopRepo.Setup(x => x.GetByOwnerIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shop?)null);

        var query = new GetSellerDashboardQuery();
        var result = await _handler.Handle(query, default);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(ShopErrors.NotFound.Code);
    }

    [Fact]
    public async Task Handle_WhenValidSeller_ReturnsExpectedDashboardData()
    {
        var userId = Guid.NewGuid();
        var shopId = Guid.NewGuid();
        _currentUser.SetupGet(x => x.IsAuthenticated).Returns(true);
        _currentUser.SetupGet(x => x.UserId).Returns(userId);

        var shop = new Shop(userId, "Test Shop", "test-shop");
        typeof(Shop).GetProperty("Id")?.SetValue(shop, shopId);
        shop.Approve();

        _shopRepo.Setup(x => x.GetByOwnerIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(shop);

        var mockReadModel = new SellerDashboardReadModel(
            TotalSalesRevenue: 15_000_000m,
            TotalOrdersCount: 42,
            WalletAvailableBalance: 12_500_000m,
            PendingOrdersCount: 3,
            DailyRevenue:
            [
                new DailyRevenuePointReadModel("2026-10-24", 5_000_000m, 15),
                new DailyRevenuePointReadModel("2026-10-25", 10_000_000m, 27)
            ],
            OrderStatuses:
            [
                new OrderStatusBreakdownReadModel("Confirmed", 3),
                new OrderStatusBreakdownReadModel("Delivered", 35)
            ],
            TopSkus:
            [
                new TopSellingSkuReadModel(Guid.NewGuid(), "Ao Thun", "SKU-AO-01", 30, 9_000_000m)
            ],
            RecentLives:
            [
                new LivestreamPerformanceSummaryReadModel(Guid.NewGuid(), "Live Khai Truong", 1500, 20, 6_000_000m, "2026-10-25T10:00:00Z")
            ],
            Actions: new SellerActionSummaryReadModel(3, 1, 2),
            WalletOverview: new WalletOverviewReadModel(
                12_500_000m,
                500_000m,
                "VND",
                [
                    new WalletTransactionSummaryReadModel(Guid.NewGuid(), "OrderRevenue", 500_000m, 12_500_000m, "Doanh thu don hang", DateTime.UtcNow)
                ]));

        _readRepo.Setup(x => x.GetAsync(shopId, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockReadModel);

        var query = new GetSellerDashboardQuery();
        var result = await _handler.Handle(query, default);

        result.IsSuccess.Should().BeTrue();
        var data = result.Value!;
        data.TotalSalesRevenue.Should().Be(15_000_000m);
        data.TotalOrdersCount.Should().Be(42);
        data.WalletAvailableBalance.Should().Be(12_500_000m);
        data.PendingOrdersCount.Should().Be(3);
        data.DailyRevenue.Should().HaveCount(2);
        data.OrderStatuses.Should().HaveCount(2);
        data.TopSkus.Should().HaveCount(1);
        data.RecentLives.Should().HaveCount(1);
        data.Actions.PendingOrders.Should().Be(3);
        data.Actions.ExpiringReturns.Should().Be(1);
        data.Actions.LowStockSkus.Should().Be(2);
        data.WalletOverview.Should().NotBeNull();
        data.WalletOverview!.RecentTransactions.Should().HaveCount(1);
    }

    [Fact]
    public void Validator_WhenFromGreaterThanTo_FailsValidation()
    {
        var validator = new GetSellerDashboardQueryValidator();
        var from = DateTimeOffset.UtcNow;
        var to = from.AddDays(-1);

        var query = new GetSellerDashboardQuery(from, to);
        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_WhenFromLessThanOrEqualToTo_PassesValidation()
    {
        var validator = new GetSellerDashboardQueryValidator();
        var from = DateTimeOffset.UtcNow;
        var to = from.AddDays(7);

        var query = new GetSellerDashboardQuery(from, to);
        var result = validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }
}
