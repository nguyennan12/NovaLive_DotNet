using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Products.Queries.GetPublicProductDetail;
using NovaLive.Application.UseCases.Products.Queries.SearchProducts;
using NovaLive.Contracts.V1.Products;

namespace NovaLive.Api.Controllers.Products;

[AllowAnonymous]
[Route("api/v1/products")]
public sealed class ProductsController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    public async Task<IActionResult> GetProducts(
        [FromQuery] ProductSearchRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new SearchProductsQuery(request), ct));

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] ProductSearchRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new SearchProductsQuery(request), ct));

    [HttpGet("{spuId:guid}")]
    public async Task<IActionResult> GetProductDetail(
        Guid spuId,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetPublicProductDetailQuery(spuId), ct));
}
