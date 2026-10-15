using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Orders.Commands.Checkout;
using NovaLive.Application.UseCases.Orders.Commands.CancelOrder;
using NovaLive.Application.UseCases.Orders.Queries.CalculateCheckoutDraft;
using NovaLive.Application.UseCases.Orders.Queries.GetMyOrders;
using NovaLive.Application.UseCases.Orders.Queries.GetOrderDetail;
using NovaLive.Contracts.V1.Orders;
namespace NovaLive.Api.Controllers.Orders;
[Authorize]
[Route("api/v1/orders")]
public sealed class OrdersController(ISender sender) : ApiControllerBase(sender)
{
    [Authorize]
    [HttpPost("calculate-checkout")]
    public async Task<IActionResult> CalculateCheckout([FromBody] CalculateCheckoutRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new CalculateCheckoutDraftQuery(request), ct));

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request,
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey, CancellationToken ct) =>
        Respond(await Sender.Send(new CheckoutCommand(request, idempotencyKey), ct), StatusCodes.Status201Created);

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] string? status = null,
        [FromQuery] DateTimeOffset? from = null, [FromQuery] DateTimeOffset? to = null,
        [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default) =>
        Respond(await Sender.Send(new GetMyOrdersQuery(status, from, to, page, size), ct));

    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetDetail(Guid orderId, CancellationToken ct) =>
        Respond(await Sender.Send(new GetOrderDetailQuery(orderId), ct));

    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid orderId, [FromBody] CancelOrderRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new CancelOrderCommand(orderId, request), ct));
}
