using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Orders.Commands.ConfirmSubOrder;
using NovaLive.Application.UseCases.Orders.Commands.CancelSubOrder;
using NovaLive.Application.UseCases.Orders.Queries.GetSellerOrders;
using NovaLive.Application.UseCases.Orders.Queries.GetSellerOrderDetail;
using NovaLive.Contracts.V1.Orders;
namespace NovaLive.Api.Controllers.Orders;
[Authorize]
[Route("api/v1/seller/orders")]
public sealed class SellerOrdersController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] string? status = null,
        [FromQuery] DateTimeOffset? from = null, [FromQuery] DateTimeOffset? to = null,
        [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default) =>
        Respond(await Sender.Send(new GetSellerOrdersQuery(status, from, to, page, size), ct));

    [HttpGet("{subOrderId:guid}")]
    public async Task<IActionResult> GetDetail(Guid subOrderId, CancellationToken ct) =>
        Respond(await Sender.Send(new GetSellerOrderDetailQuery(subOrderId), ct));

    [HttpPut("{subOrderId:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid subOrderId, [FromBody] ConfirmSubOrderRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new ConfirmSubOrderCommand(subOrderId, request), ct));

    [HttpPost("{subOrderId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid subOrderId, [FromBody] CancelOrderRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new CancelSubOrderCommand(subOrderId, request), ct));
}
