using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Products.Queries.GetSpuDetail;

namespace NovaLive.Api.Controllers;

[AllowAnonymous]
[Route("api/v1/products")]
public sealed class ProductsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet("{spuId:guid}")]
    public async Task<IActionResult> GetProductDetail(
        Guid spuId,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetSpuDetailQuery(spuId, IsSellerView: false), ct));
}
