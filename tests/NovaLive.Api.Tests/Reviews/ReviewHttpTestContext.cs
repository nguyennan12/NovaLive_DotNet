using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NovaLive.Api.Tests.Auth;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Products;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Reviews;
using NovaLive.Domain.Shops;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.Persistence.Interceptors;

namespace NovaLive.Api.Tests.Reviews;

internal sealed class ReviewHttpTestContext : IDisposable
{
    private readonly DateTimeOffset tokenIssuedAt;
    public AuthApplicationFactory Factory { get; } = new();
    public WebApplicationFactory<Program> App { get; }
    public HttpClient Client { get; }
    public Guid BuyerId { get; private set; }
    public Guid SellerId { get; private set; }
    public Guid AdminId { get; private set; }
    public Guid ShopId { get; private set; }
    public Guid SpuId { get; private set; }
    public Guid SkuId { get; private set; }
    public Guid ItemId { get; private set; }

    public ReviewHttpTestContext(Action<IServiceCollection>? configure = null)
    {
        var now = Factory.Clock.UtcNow;
        tokenIssuedAt = new(now.Ticks - now.Ticks % TimeSpan.TicksPerMillisecond, now.Offset);
        Factory.Clock.UtcNow = tokenIssuedAt;
        App = Factory.WithWebHostBuilder(b => b.ConfigureTestServices(services =>
        {
            services.AddDbContext<AppDbContext>((provider, options) =>
                options.AddInterceptors(provider.GetRequiredService<AuditableEntityInterceptor>()));
            configure?.Invoke(services);
        }));
        Client = App.CreateClient();
    }

    public async Task SeedAsync()
    {
        using var scope = App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Mock.Get(scope.ServiceProvider.GetRequiredService<ICacheService>())
            .Setup(cache => cache.GetAsync<string[]>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string[]?)null);
        var buyer = new User("private-review-buyer@example.com", "hash", "Nguyễn Văn Buyer", "0901234567"); buyer.Activate();
        var seller = new User("review-seller@example.com", "hash", "Seller"); seller.Activate();
        var admin = new User("review-admin@example.com", "hash", "Admin"); admin.Activate();
        var shop = new Shop(seller.Id, "Review shop", "review-shop"); shop.Approve();
        var spu = new Spu(shop.Id, Guid.NewGuid(), "Review product", null, null, null, null);
        var sku = new Sku(spu.Id, shop.Id, "REVIEW-SKU", "{}", 100, 100);
        var parent = ParentOrder.Create(buyer.Id, "REVIEW-PARENT", "{}", 100, 0, 0, 100, null, Factory.Clock.UtcNow.AddDays(-1));
        var sub = SubOrder.Create(parent.Id, "REVIEW-SUB", Guid.NewGuid(),
            new ShopCalculationResult(shop.Id, 100, 0, 0, 0, 0, 100, 100, []), true, Factory.Clock.UtcNow.AddDays(-1));
        sub.MarkAsShipping(Factory.Clock.UtcNow.AddHours(-2)); sub.MarkAsDelivered(Factory.Clock.UtcNow.AddHours(-1));
        var item = OrderItem.Create(sub.Id, "{}", 100, new OrderItemCalculationResult(sku.Id, spu.Id, 1, 100, 100, 0, 0, 0, 100));
        db.Users.AddRange(buyer, seller, admin);
        db.UserRoles.AddRange(new UserRole(buyer.Id, SystemRoleIds.Buyer, Factory.Clock.UtcNow),
            new UserRole(seller.Id, SystemRoleIds.Seller, Factory.Clock.UtcNow), new UserRole(admin.Id, SystemRoleIds.Admin, Factory.Clock.UtcNow));
        db.Shops.Add(shop); db.Spus.Add(spu); db.Skus.Add(sku);
        db.ParentOrders.Add(parent); db.SubOrders.Add(sub); db.OrderItems.Add(item);
        await db.SaveChangesAsync();
        BuyerId = buyer.Id; SellerId = seller.Id; AdminId = admin.Id; ShopId = shop.Id;
        SpuId = spu.Id; SkuId = sku.Id; ItemId = item.Id;
        Authenticate(BuyerId, "Buyer");
    }

    public void Authenticate(Guid id, string role, Guid? shop = null)
    {
        using var scope = App.Services.CreateScope();
        // JWT validation uses the system clock; keep token time stable while advancing the domain clock.
        var domainNow = Factory.Clock.UtcNow;
        Factory.Clock.UtcNow = tokenIssuedAt;
        var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(id, "user@example.com", [role], shop);
        Factory.Clock.UtcNow = domainNow;
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        Client.DefaultRequestHeaders.Remove("X-Shop-Id");
        if (shop.HasValue) Client.DefaultRequestHeaders.Add("X-Shop-Id", shop.ToString());
    }

    public async Task<Review> AddReviewAsync(int rating, bool media = false, bool hidden = false, DateTimeOffset? now = null)
    {
        var review = Review.Create(Guid.NewGuid(), SkuId, ShopId, BuyerId, rating, null, null,
            media ? ["https://cdn.novalive.vn/reviews/a.jpg"] : [], now ?? Factory.Clock.UtcNow).Value!;
        if (hidden) review.Hide("Spam", Factory.Clock.UtcNow);
        using var scope = App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Reviews.Add(review);
        db.ReviewImages.AddRange(review.Images);
        await db.SaveChangesAsync();
        return review;
    }

    public void Dispose()
    {
        Client.Dispose();
        if (App != Factory) App.Dispose();
        Factory.Dispose();
    }
}
