using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Orders.Queries.CalculateCheckoutDraft;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Orders;

namespace NovaLive.Api.Controllers.Orders;

[Route("api/v1/orders")]
public sealed class OrdersController(ISender sender) : ApiControllerBase(sender)
{
    // T23 will extend this controller with order submission and history.
    [Authorize]
    [HttpPost("calculate-checkout")]
    [ProducesResponseType(typeof(ApiResponse<CheckoutPreviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CalculateCheckout([FromBody] CalculateCheckoutRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new CalculateCheckoutDraftQuery(request), ct));
}
