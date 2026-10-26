using System.Reflection;
using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NovaLive.Api.Controllers.Reviews;
using NovaLive.Api.Tests.Auth;
using NovaLive.Application.UseCases.Reviews.Commands.CreateReview;
using NovaLive.Application.UseCases.Reviews.Commands.UpdateReview;
using NovaLive.Application.UseCases.Reviews.Commands.ReplyReview;
using NovaLive.Application.UseCases.Reviews.Commands.HideReview;
using NovaLive.Application.UseCases.Reviews.Queries.GetProductReviews;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Common;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Reviews;

namespace NovaLive.Api.Tests.Reviews;

[Collection("API host lifecycle")]
public sealed class ReviewsControllerTests
{
    private static ReviewResponse Response(Guid id) => new(id, Guid.NewGuid(), "N***", null, Guid.NewGuid(), Guid.NewGuid(),
        "{}", 5, null, null, null, null, new DateTime(2026, 10, 1), []);
    private static string? Route<T>() => typeof(T).GetCustomAttribute<RouteAttribute>()!.Template;

    [Fact]
    public void RoutesAuthorizationRolesAndPermissions_MatchT44()
    {
        Route<ReviewsController>().Should().Be("api/v1/reviews");
        Route<SellerReviewsController>().Should().Be("api/v1/seller/reviews");
        Route<AdminReviewsController>().Should().Be("api/v1/admin/reviews");
        var publicMethod = typeof(ReviewsController).GetMethod(nameof(ReviewsController.GetProductReviews))!;
        publicMethod.GetCustomAttribute<HttpGetAttribute>()!.Template.Should().Be("~/api/v1/products/{spuId:guid}/reviews");
        publicMethod.GetCustomAttribute<AllowAnonymousAttribute>().Should().NotBeNull();
        typeof(ReviewsController).GetMethod(nameof(ReviewsController.CreateReview))!.GetCustomAttribute<AuthorizeAttribute>().Should().NotBeNull();
        typeof(ReviewsController).GetMethod(nameof(ReviewsController.UpdateReview))!.GetCustomAttribute<AuthorizeAttribute>().Should().NotBeNull();
        typeof(ReviewsController).GetMethod(nameof(ReviewsController.UpdateReview))!.GetCustomAttribute<HttpPutAttribute>()!.Template.Should().Be("{reviewId:guid}");
        typeof(SellerReviewsController).GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().Be("Seller");
        typeof(SellerReviewsController).GetMethod(nameof(SellerReviewsController.ReplyReview))!.GetCustomAttribute<HttpPostAttribute>()!.Template.Should().Be("{reviewId:guid}/reply");
        typeof(AdminReviewsController).GetCustomAttribute<AuthorizeAttribute>()!.Roles.Should().Be("Admin");
        typeof(AdminReviewsController).GetMethod(nameof(AdminReviewsController.HideReview))!.GetCustomAttribute<HttpPutAttribute>()!.Template.Should().Be("{reviewId:guid}/hide");
        new CreateReviewCommand(new(Guid.NewGuid(), 5, null, null, null)).RequiredPermission.Should().Be(PermissionCodes.Reviews.CreateOwn);
        new UpdateReviewCommand(Guid.NewGuid(), new(5, null, null, null)).RequiredPermission.Should().Be(PermissionCodes.Reviews.UpdateOwn);
        new ReplyReviewCommand(Guid.NewGuid(), new("Reply")).RequiredPermission.Should().Be(PermissionCodes.Reviews.ReplyOwnShop);
        new HideReviewCommand(Guid.NewGuid(), new("Reason")).RequiredPermission.Should().Be(PermissionCodes.Reviews.HideAll);
    }

    [Fact]
    public async Task Actions_DispatchExactRequestsThroughSenderAndRespond()
    {
        var sender = new Mock<ISender>(MockBehavior.Strict);
        var id = Guid.NewGuid();
        var spuId = Guid.NewGuid();
        var response = Response(id);
        var create = new CreateReviewRequest(Guid.NewGuid(), 5, null, null, []);
        var update = new UpdateReviewRequest(4, "New", null, []);
        var reply = new SellerReplyReviewRequest("Thanks");
        var hide = new HideReviewRequest("Spam");
        sender.Setup(s => s.Send(It.Is<CreateReviewCommand>(c => c.Request == create), default)).ReturnsAsync(Result.Success(response));
        sender.Setup(s => s.Send(It.Is<UpdateReviewCommand>(c => c.ReviewId == id && c.Request == update), default)).ReturnsAsync(Result.Success(response));
        sender.Setup(s => s.Send(It.Is<ReplyReviewCommand>(c => c.ReviewId == id && c.Request == reply), default)).ReturnsAsync(Result.Success(response));
        sender.Setup(s => s.Send(It.Is<HideReviewCommand>(c => c.ReviewId == id && c.Request == hide), default)).ReturnsAsync(Result.Success());
        sender.Setup(s => s.Send(It.Is<GetProductReviewsQuery>(q => q.SpuId == spuId && q.Rating == 5 && q.HasMedia == true && q.Page == 2 && q.Size == 10), default))
            .ReturnsAsync(Result.Success(PagedResult<ReviewResponse>.Create([response], 2, 10, 11)));
        var controller = new ReviewsController(sender.Object);
        ((ObjectResult)await controller.CreateReview(create, default)).StatusCode.Should().Be(201);
        ((ObjectResult)await controller.UpdateReview(id, update, default)).StatusCode.Should().Be(200);
        ((ObjectResult)await new SellerReviewsController(sender.Object).ReplyReview(id, reply, default)).StatusCode.Should().Be(200);
        ((ObjectResult)await new AdminReviewsController(sender.Object).HideReview(id, hide, default)).StatusCode.Should().Be(200);
        ((ObjectResult)await controller.GetProductReviews(spuId, 5, true, 2, 10)).StatusCode.Should().Be(200);
        sender.VerifyAll();
    }

    [Theory]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.AlreadyExists, 409)]
    [InlineData(ErrorType.Forbidden, 403)]
    public async Task Respond_MapsBusinessErrors(ErrorType type, int expectedStatus)
    {
        var error = type switch
        {
            ErrorType.NotFound => ReviewErrors.NotFound,
            ErrorType.AlreadyExists => ReviewErrors.AlreadyReviewed,
            _ => Error.Forbidden("test", "test")
        };
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.IsAny<UpdateReviewCommand>(), default)).ReturnsAsync(Result<ReviewResponse>.Failure(error));
        var result = (ObjectResult)await new ReviewsController(sender.Object).UpdateReview(Guid.NewGuid(), new(5, null, null, null), default);
        result.StatusCode.Should().Be(expectedStatus);
    }

    [Fact]
    public async Task PublicAbsoluteRoute_BindsQueryAndAllowsAnonymous_PrivateRoutesRequireJwt()
    {
        var id = Guid.NewGuid();
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.Is<GetProductReviewsQuery>(q => q.SpuId == id && q.Rating == 4 && q.HasMedia == false && q.Page == 2 && q.Size == 5), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(PagedResult<ReviewResponse>.Create([], 2, 5, 0)));
        using var factory = new AuthApplicationFactory();
        using var app = factory.WithWebHostBuilder(b => b.ConfigureTestServices(services =>
        {
            services.RemoveAll<ISender>();
            services.AddSingleton(sender.Object);
        }));
        using var client = app.CreateClient();
        (await client.GetAsync($"/api/v1/products/{id}/reviews?rating=4&hasMedia=false&page=2&size=5")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync($"/api/v1/reviews/products/{id}/reviews")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.PostAsJsonAsync("/api/v1/reviews", new CreateReviewRequest(id, 5, null, null, null))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.PutAsJsonAsync($"/api/v1/reviews/{id}", new UpdateReviewRequest(5, null, null, null))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.PostAsJsonAsync($"/api/v1/seller/reviews/{id}/reply", new SellerReplyReviewRequest("Reply"))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.PutAsJsonAsync($"/api/v1/admin/reviews/{id}/hide", new HideReviewRequest("Reason"))).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        sender.Verify(s => s.Send(It.IsAny<GetProductReviewsQuery>(), It.IsAny<CancellationToken>()), Times.Once());
    }
}
