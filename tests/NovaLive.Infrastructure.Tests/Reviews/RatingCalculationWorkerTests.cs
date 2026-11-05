using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NovaLive.Domain.Reviews;
using NovaLive.Domain.Shops;
using NovaLive.Infrastructure.Persistence;
using NovaLive.Infrastructure.System;

namespace NovaLive.Infrastructure.Tests.Reviews;

public sealed class RatingCalculationWorkerTests
{
    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection = new("Data Source=:memory:");
        private readonly ServiceProvider services;
        public RatingCalculationWorker Worker { get; }
        public DateTimeOffset Now { get; } = DateTimeOffset.Parse("2026-11-02T00:00:00Z");
        public Shop Shop { get; } = new(Guid.NewGuid(), "Rating shop", "rating-shop");
        public Shop EmptyShop { get; } = new(Guid.NewGuid(), "Empty shop", "empty-shop");
        public Fixture()
        {
            connection.Open();
            services = new ServiceCollection().AddDbContext<AppDbContext>(o => o.UseSqlite(connection)
                .UseSnakeCaseNamingConvention()).BuildServiceProvider();
            Worker = new(services.GetRequiredService<IServiceScopeFactory>(), NullLogger<RatingCalculationWorker>.Instance);
        }
        public async Task SeedAsync()
        {
            await WithDbAsync(async db =>
            {
                await db.Database.EnsureCreatedAsync();
                db.Shops.AddRange(Shop, EmptyShop);
                await db.SaveChangesAsync();
            });
        }
        public Review NewReview(int rating, Guid? sku = null, bool hidden = false)
        {
            var review = Review.Create(Guid.NewGuid(), sku ?? Guid.NewGuid(), Shop.Id, Guid.NewGuid(),
                rating, null, null, [], Now).Value!;
            if (hidden) review.Hide("Spam", Now);
            return review;
        }
        public async Task WithDbAsync(Func<AppDbContext, Task> action)
        {
            await using var scope = services.CreateAsyncScope();
            await action(scope.ServiceProvider.GetRequiredService<AppDbContext>());
        }
        public async ValueTask DisposeAsync()
        {
            Worker.Dispose();
            await services.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task CalculatesWeightedAverageAcrossSkus_ExcludesHiddenReviews_AndOnlyWritesChanges()
    {
        await using var f = new Fixture(); await f.SeedAsync();
        var skuA = Guid.NewGuid(); var skuB = Guid.NewGuid();
        await f.WithDbAsync(async db =>
        {
            db.Reviews.AddRange(f.NewReview(5, skuA), f.NewReview(5, skuA), f.NewReview(1, skuB), f.NewReview(1, hidden: true));
            await db.SaveChangesAsync();
        });
        (await f.Worker.RunOnceAsync(default)).Should().Be(1);
        await f.WithDbAsync(async db =>
        {
            var shop = await db.Shops.SingleAsync(s => s.Id == f.Shop.Id);
            shop.RatingAvg.Should().Be(3.67m);
            shop.RatingCount.Should().Be(3);
            (await db.Shops.SingleAsync(s => s.Id == f.EmptyShop.Id)).RatingCount.Should().Be(0);
        });
        (await f.Worker.RunOnceAsync(default)).Should().Be(0);
    }

    [Fact]
    public async Task EditsAndHides_AreRecalculated_AndLastHiddenReviewResetsAverageToZero()
    {
        await using var f = new Fixture(); await f.SeedAsync();
        var review = f.NewReview(5);
        await f.WithDbAsync(async db => { db.Reviews.Add(review); await db.SaveChangesAsync(); });
        await f.Worker.RunOnceAsync(default);
        await f.WithDbAsync(async db =>
        {
            var tracked = await db.Reviews.SingleAsync();
            tracked.Update(2, null, null, [], f.Now.AddMinutes(1)).IsSuccess.Should().BeTrue();
            await db.SaveChangesAsync();
        });
        await f.Worker.RunOnceAsync(default);
        await f.WithDbAsync(async db =>
        {
            (await db.Shops.SingleAsync(s => s.Id == f.Shop.Id)).RatingAvg.Should().Be(2);
            (await db.Reviews.SingleAsync()).Hide("Moderated", f.Now.AddMinutes(2));
            await db.SaveChangesAsync();
        });
        await f.Worker.RunOnceAsync(default);
        await f.WithDbAsync(async db =>
        {
            var shop = await db.Shops.SingleAsync(s => s.Id == f.Shop.Id);
            shop.RatingAvg.Should().Be(0);
            shop.RatingCount.Should().Be(0);
        });
    }

    [Fact]
    public async Task Restart_DoesNotMissLateCommittedOrBackdatedReviews()
    {
        await using var f = new Fixture(); await f.SeedAsync();
        await f.Worker.RunOnceAsync(default);
        await f.WithDbAsync(async db =>
        {
            var review = Review.Create(Guid.NewGuid(), Guid.NewGuid(), f.Shop.Id, Guid.NewGuid(),
                4, null, null, [], f.Now.AddDays(-10)).Value!;
            db.Reviews.Add(review); await db.SaveChangesAsync();
        });
        // Each RunOnce creates a fresh DB scope and carries no timestamp cursor.
        (await f.Worker.RunOnceAsync(default)).Should().Be(1);
        await f.WithDbAsync(async db => (await db.Shops.SingleAsync(s => s.Id == f.Shop.Id)).RatingAvg.Should().Be(4));
    }
}
