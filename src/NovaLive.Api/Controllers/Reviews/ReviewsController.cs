using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Reviews.Commands.CreateReview;
using NovaLive.Application.UseCases.Reviews.Commands.UpdateReview;
using NovaLive.Application.UseCases.Reviews.Queries.GetProductReviews;
using NovaLive.Contracts.V1.Reviews;
using NovaLive.Domain.Reviews;

namespace NovaLive.Api.Controllers.Reviews;

[Route("api/v1/reviews")]
public sealed class ReviewsController(ISender sender) : ApiControllerBase(sender)
{
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateReview([FromBody] CreateReviewRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new CreateReviewCommand(request), ct), StatusCodes.Status201Created);

    [Authorize]
    [HttpPut("{reviewId:guid}")]
    public async Task<IActionResult> UpdateReview(Guid reviewId, [FromBody] UpdateReviewRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new UpdateReviewCommand(reviewId, request), ct));

    [AllowAnonymous]
    [HttpGet("~/api/v1/products/{spuId:guid}/reviews")]
    public async Task<IActionResult> GetProductReviews(
        Guid spuId, [FromQuery] int? rating = null, [FromQuery] bool? hasMedia = null,
        [FromQuery] int page = 1, [FromQuery] int size = ReviewLimits.DefaultPageSize, CancellationToken ct = default) =>
        Respond(await Sender.Send(new GetProductReviewsQuery(spuId, rating, hasMedia, page, size), ct));
}
