using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers.Shops;
using NovaLive.Application.UseCases.Products.Queries.SearchProducts;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Products;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Tests.Shops;

public sealed class ShopsControllerTests
{
    [Fact]
    public async Task Products_UsesRouteShopIdAndDocumentedSort_DelegatesToExistingSearch()
    {
        var routeShopId = Guid.NewGuid();
        var request = new ProductSearchRequest(Keyword: "shirt", ShopId: Guid.NewGuid(), Page: 2, Size: 5);
        var sender = new Mock<ISender>();
        sender.Setup(s => s.Send(It.Is<SearchProductsQuery>(q => q.Request.ShopId == routeShopId
                && q.Request.SortBy == "price_asc" && q.Request.Keyword == "shirt" && q.Request.Page == 2 && q.Request.Size == 5),
            It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success(PagedResult<ProductSummaryResponse>.Create([], 2, 5, 0)));
        var response = await new ShopsController(sender.Object).GetProducts(routeShopId, request, "price_asc", default);
        response.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(200);
        sender.VerifyAll();
    }
}
