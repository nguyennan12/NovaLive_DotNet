using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Products.Commands.CreateSpu;
using NovaLive.Application.UseCases.Products.Commands.DeleteSpu;
using NovaLive.Application.UseCases.Products.Commands.UpdateSkuPrice;
using NovaLive.Application.UseCases.Products.Commands.UpdateSpu;
using NovaLive.Application.UseCases.Products.Queries.GetSellerProducts;
using NovaLive.Application.UseCases.Products.Queries.GetSpuDetail;
using NovaLive.Contracts.V1.Products;

namespace NovaLive.Api.Controllers;

[Authorize]
[Route("api/v1/seller")]
public sealed class SellerProductsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts(
        [FromQuery] SearchProductsRequest data,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetSellerProductsQuery(data), ct));

    [HttpGet("products/{spuId:guid}")]
    public async Task<IActionResult> GetProductDetail(
        Guid spuId,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetSpuDetailQuery(spuId, IsSellerView: true), ct));

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct(
        [FromBody] CreateSpuRequest data,
        CancellationToken ct) =>
        Respond(await Sender.Send(new CreateSpuCommand(data), ct), StatusCodes.Status201Created);

    [HttpPut("products/{spuId:guid}")]
    public async Task<IActionResult> UpdateProduct(
        Guid spuId,
        [FromBody] UpdateSpuRequest data,
        CancellationToken ct) =>
        Respond(await Sender.Send(new UpdateSpuCommand(spuId, data), ct));

    [HttpDelete("products/{spuId:guid}")]
    public async Task<IActionResult> DeleteProduct(
        Guid spuId,
        CancellationToken ct) =>
        Respond(await Sender.Send(new DeleteSpuCommand(spuId), ct));

    [HttpPut("skus/{skuId:guid}")]
    public async Task<IActionResult> UpdateSku(
        Guid skuId,
        [FromBody] UpdateSkuPriceRequest data,
        CancellationToken ct) =>
        Respond(await Sender.Send(new UpdateSkuPriceCommand(skuId, data), ct));
}
