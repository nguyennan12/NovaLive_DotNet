using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Discounts.Commands.CreateSellerDiscount;
using NovaLive.Application.UseCases.Discounts.Commands.DeleteSellerDiscount;
using NovaLive.Application.UseCases.Discounts.Commands.UpdateDiscount;
using NovaLive.Application.UseCases.Discounts.Queries.GetSellerDiscounts;
using NovaLive.Contracts.V1.Discounts;

namespace NovaLive.Api.Controllers.Discounts;

[Authorize]
[Route("api/v1/seller/discounts")]
public sealed class SellerDiscountsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetDiscounts(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default) =>
        Respond(await Sender.Send(new GetSellerDiscountsQuery(status, page, size), ct));

    [HttpPost]
    public async Task<IActionResult> CreateDiscount(
        [FromBody] CreateDiscountRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new CreateSellerDiscountCommand(request), ct), StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateDiscount(
        Guid id,
        [FromBody] UpdateDiscountRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new UpdateDiscountCommand(id, request), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDiscount(
        Guid id,
        CancellationToken ct) =>
        Respond(await Sender.Send(new DeleteSellerDiscountCommand(id), ct));
}
