using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Reviews.Commands.HideReview;
using NovaLive.Contracts.V1.Reviews;

namespace NovaLive.Api.Controllers.Reviews;

[Authorize(Roles = "Admin")]
[Route("api/v1/admin/reviews")]
public sealed class AdminReviewsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPut("{reviewId:guid}/hide")]
    public async Task<IActionResult> HideReview(Guid reviewId, [FromBody] HideReviewRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new HideReviewCommand(reviewId, request), ct));
}
