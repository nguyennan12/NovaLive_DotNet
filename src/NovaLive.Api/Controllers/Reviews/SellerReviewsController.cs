using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Reviews.Commands.ReplyReview;
using NovaLive.Contracts.V1.Reviews;

namespace NovaLive.Api.Controllers.Reviews;

[Authorize(Roles = "Seller")]
[Route("api/v1/seller/reviews")]
public sealed class SellerReviewsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost("{reviewId:guid}/reply")]
    public async Task<IActionResult> ReplyReview(Guid reviewId, [FromBody] SellerReplyReviewRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new ReplyReviewCommand(reviewId, request), ct));
}
