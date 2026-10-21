using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.System;
using NovaLive.Infrastructure.Persistence;

namespace NovaLive.Api.Tests.Reviews;

[Collection("API host lifecycle")]
public sealed class ReviewWorkflowHttpTests
{
    [Theory]
    [InlineData("ix_reviews_order_item_id", HttpStatusCode.Conflict)]
    [InlineData("reviews_order_item_id_key", HttpStatusCode.Conflict)]
    [InlineData("pk_reviews", HttpStatusCode.Conflict)]
    public async Task Create_TranslatesOnlyTheOrderItemUniqueViolationAndRollsBack(string constraint, HttpStatusCode expected)
    {
        using var c = new ReviewHttpTestContext(services =>
            services.AddDbContext<AppDbContext>(options =>
                options.AddInterceptors(new ReviewUniqueViolationInterceptor(constraint))));
        await c.SeedAsync();
        var result = await c.Client.PostAsJsonAsync("/api/v1/reviews",
            new CreateReviewRequest(c.ItemId, 5, null, null, ["https://cdn.novalive.vn/reviews/a.jpg"]));
        result.StatusCode.Should().Be(expected);
        if (constraint != "pk_reviews")
            (await result.Content.ReadAsStringAsync()).Should().Contain("Reviews.AlreadyReviewed");
        else
            (await result.Content.ReadAsStringAsync()).Should().Contain("DATA_CONFLICT").And.NotContain("Reviews.AlreadyReviewed");
        using var scope = c.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Reviews.CountAsync()).Should().Be(0);
        (await db.ReviewImages.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task BuyerEditsOnce_SellerReplies_AdminHidesWithAudit_PublicExcludesHidden()
    {
        using var c = new ReviewHttpTestContext();
        await c.SeedAsync();
        var created = await c.Client.PostAsJsonAsync("/api/v1/reviews",
            new CreateReviewRequest(c.ItemId, 5, "Title", "Content", ["https://cdn.novalive.vn/reviews/old.jpg"]));
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var response = (await created.Content.ReadFromJsonAsync<ApiResponse<ReviewResponse>>())!.Data!;
        response.BuyerName.Should().Be("N*** V*** B***");
        (await created.Content.ReadAsStringAsync()).Should().NotContain("private-review-buyer@example.com").And.NotContain("0901234567");
        var id = response.Id;
        (await c.Client.PostAsJsonAsync("/api/v1/reviews", new CreateReviewRequest(c.ItemId, 5, null, null, null))).StatusCode.Should().Be(HttpStatusCode.Conflict);
        c.Factory.Clock.UtcNow = c.Factory.Clock.UtcNow.AddDays(1);
        var updatedAt = c.Factory.Clock.UtcNow;
        var update = new UpdateReviewRequest(2, "New", "New content", ["https://cdn.novalive.vn/reviews/new.mp4"]);
        (await c.Client.PutAsJsonAsync($"/api/v1/reviews/{id}", update)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.Client.PutAsJsonAsync($"/api/v1/reviews/{id}", update)).StatusCode.Should().Be(HttpStatusCode.Conflict);
        using (var scope = c.App.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var review = await db.Reviews.SingleAsync(r => r.Id == id);
            review.UpdatedAt.Should().Be(updatedAt);
            review.UpdatedBy.Should().Be(c.BuyerId);
            (await db.ReviewImages.Where(image => image.ReviewId == id).ToListAsync())
                .Should().ContainSingle().Which.MediaUrl.Should().EndWith("new.mp4");
            (await db.ReviewImages.CountAsync(i => i.ReviewId == id && i.MediaUrl.EndsWith("old.jpg"))).Should().Be(0);
        }
        c.Authenticate(c.SellerId, "Seller", c.ShopId);
        (await c.Client.PostAsJsonAsync($"/api/v1/seller/reviews/{id}/reply", new SellerReplyReviewRequest("Thanks"))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.Client.PostAsJsonAsync($"/api/v1/seller/reviews/{id}/reply", new SellerReplyReviewRequest("Again"))).StatusCode.Should().Be(HttpStatusCode.Conflict);
        c.Authenticate(c.AdminId, "Admin");
        c.Factory.Clock.UtcNow = c.Factory.Clock.UtcNow.AddHours(1);
        var hiddenAt = c.Factory.Clock.UtcNow;
        (await c.Client.PutAsJsonAsync($"/api/v1/admin/reviews/{id}/hide", new HideReviewRequest("Spam"))).StatusCode.Should().Be(HttpStatusCode.OK);
        (await c.Client.PutAsJsonAsync($"/api/v1/admin/reviews/{id}/hide", new HideReviewRequest("Again"))).StatusCode.Should().Be(HttpStatusCode.OK);
        using (var scope = c.App.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.Reviews.SingleAsync(r => r.Id == id)).UpdatedAt.Should().Be(hiddenAt);
            var log = await db.AuditLogs.SingleAsync(l => l.EntityId == id);
            log.UserId.Should().Be(c.AdminId);
            using var json = JsonDocument.Parse(log.NewDataJson!);
            json.RootElement.GetProperty("reason").GetString().Should().Be("Spam");
            json.RootElement.GetProperty("is_visible").GetBoolean().Should().BeFalse();
        }
        c.Client.DefaultRequestHeaders.Authorization = null;
        var page = await c.Client.GetFromJsonAsync<ApiResponse<PagedResult<ReviewResponse>>>($"/api/v1/products/{c.SpuId}/reviews");
        page!.Data!.Total.Should().Be(0);
    }

    [Fact]
    public async Task PublicListing_FiltersVisibilityRatingMediaAndPaginatesStably()
    {
        using var c = new ReviewHttpTestContext();
        await c.SeedAsync();
        var older = await c.AddReviewAsync(5, true, now: c.Factory.Clock.UtcNow.AddDays(-2));
        var sameTimeA = await c.AddReviewAsync(5, true);
        var sameTimeB = await c.AddReviewAsync(5, true);
        await c.AddReviewAsync(1);
        await c.AddReviewAsync(5, true, true);
        c.Client.DefaultRequestHeaders.Authorization = null;
        async Task<PagedResult<ReviewResponse>> Page(string query) =>
            (await c.Client.GetFromJsonAsync<ApiResponse<PagedResult<ReviewResponse>>>($"/api/v1/products/{c.SpuId}/reviews{query}"))!.Data!;
        var first = await Page("?rating=5&hasMedia=true&page=1&size=2");
        var second = await Page("?rating=5&hasMedia=true&page=2&size=2");
        first.Total.Should().Be(3);
        var expected = new[] { sameTimeA, sameTimeB }.OrderByDescending(r => r.Id).Select(r => r.Id);
        first.Items.Select(r => r.Id).Should().Equal(expected);
        second.Items.Should().ContainSingle().Which.Id.Should().Be(older.Id);
        var withoutMedia = await Page("?hasMedia=false");
        withoutMedia.Total.Should().Be(1);
        withoutMedia.Items.Single().Rating.Should().Be(1);
        (await Page("")).Total.Should().Be(4);
        (await c.Client.GetAsync($"/api/v1/products/{c.SpuId}/reviews?size=51")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await c.Client.GetFromJsonAsync<ApiResponse<PagedResult<ReviewResponse>>>($"/api/v1/products/{Guid.NewGuid()}/reviews"))!.Data!.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Hide_AuditFailureRollsBackReviewMutation()
    {
        var audit = new Mock<IAuditLogRepository>();
        audit.Setup(a => a.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Injected persistence failure"));
        using var c = new ReviewHttpTestContext(services =>
        {
            services.RemoveAll<IAuditLogRepository>();
            services.AddSingleton(audit.Object);
        });
        await c.SeedAsync();
        var review = await c.AddReviewAsync(5);
        c.Authenticate(c.AdminId, "Admin");
        (await c.Client.PutAsJsonAsync($"/api/v1/admin/reviews/{review.Id}/hide", new HideReviewRequest("Spam")))
            .StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        using var scope = c.App.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Reviews.SingleAsync(r => r.Id == review.Id)).IsVisible.Should().BeTrue();
        (await db.AuditLogs.CountAsync(l => l.EntityId == review.Id)).Should().Be(0);
    }

    [Fact]
    public async Task PrivateEndpoints_EnforceRolesOwnershipAndTrustedMediaBucket()
    {
        using var c = new ReviewHttpTestContext();
        await c.SeedAsync();
        var review = await c.AddReviewAsync(5);
        (await c.Client.PostAsJsonAsync($"/api/v1/seller/reviews/{review.Id}/reply", new SellerReplyReviewRequest("Reply"))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await c.Client.PutAsJsonAsync($"/api/v1/admin/reviews/{review.Id}/hide", new HideReviewRequest("Spam"))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await c.Client.PostAsJsonAsync("/api/v1/reviews", new CreateReviewRequest(c.ItemId, 5, null, null, ["https://cdn.novalive.vn/products/a.jpg"]))).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        c.Authenticate(c.SellerId, "Seller", Guid.NewGuid());
        (await c.Client.PostAsJsonAsync($"/api/v1/seller/reviews/{review.Id}/reply", new SellerReplyReviewRequest("Reply"))).StatusCode.Should().Be(HttpStatusCode.NotFound);
        // Even a signed seller JWT plus a spoofed context pointing to this shop cannot replace persisted ownership.
        c.Authenticate(c.BuyerId, "Seller", c.ShopId);
        (await c.Client.PostAsJsonAsync($"/api/v1/seller/reviews/{review.Id}/reply", new SellerReplyReviewRequest("Reply"))).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
