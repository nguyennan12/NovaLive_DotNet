using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Shipping.Commands.CreateShipmentOrder;
using NovaLive.Application.UseCases.Shipping.Queries.GetOrderTracking;
using NovaLive.Application.UseCases.Shipping.Queries.GetShippingLabel;
using NovaLive.Contracts.V1.Shipping;

namespace NovaLive.Api.Controllers.Shipping;

[Authorize]
[Route("api/v1/seller/shipping")]
public sealed class SellerShippingController(ISender sender) : ApiControllerBase(sender)
{
    [HttpPost("create-order")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateShippingOrderRequest request, CancellationToken ct) =>
        Respond(await Sender.Send(new CreateShipmentOrderCommand(request), ct), StatusCodes.Status201Created);

    [HttpGet("{subOrderId:guid}/label")]
    public async Task<IActionResult> GetLabel(Guid subOrderId, CancellationToken ct)
    {
        var result = await Sender.Send(new GetShippingLabelQuery(subOrderId), ct);
        if (result.IsFailure) return Respond(result);
        return File(result.Value!, "application/pdf", $"shipping-label-{subOrderId}.pdf");
    }

    [HttpGet("{subOrderId:guid}/tracking")]
    public async Task<IActionResult> GetTracking(Guid subOrderId, CancellationToken ct) =>
        Respond(await Sender.Send(new GetOrderTrackingQuery(subOrderId, IsSeller: true), ct));
}
