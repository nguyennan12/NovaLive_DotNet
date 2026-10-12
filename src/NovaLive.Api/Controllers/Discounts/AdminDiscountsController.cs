using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Discounts.Commands.CreatePlatformDiscount;
using NovaLive.Application.UseCases.Discounts.Commands.DeletePlatformDiscount;
using NovaLive.Application.UseCases.Discounts.Commands.UpdatePlatformDiscount;
using NovaLive.Application.UseCases.Discounts.Queries.GetPlatformDiscountsAdmin;
using NovaLive.Contracts.V1.Discounts;

namespace NovaLive.Api.Controllers.Discounts;

[Authorize]
[Route("api/v1/admin/discounts")]
public sealed class AdminDiscountsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetDiscounts(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        CancellationToken ct = default) =>
        Respond(await Sender.Send(new GetPlatformDiscountsAdminQuery(status, page, size), ct));

    [HttpPost]
    public async Task<IActionResult> CreatePlatformDiscount(
        [FromBody] CreateDiscountRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new CreatePlatformDiscountCommand(request), ct), StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdatePlatformDiscount(
        Guid id,
        [FromBody] UpdateDiscountRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new UpdatePlatformDiscountCommand(id, request), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeletePlatformDiscount(
        Guid id,
        CancellationToken ct) =>
        Respond(await Sender.Send(new DeletePlatformDiscountCommand(id), ct));
}
