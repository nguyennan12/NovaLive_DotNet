using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers.Common;
using NovaLive.Api.Controllers.FlashSales;
using NovaLive.Application.UseCases.FlashSales.Commands.ApproveFlashSaleItem;
using NovaLive.Application.UseCases.FlashSales.Commands.CreateFlashSaleCampaign;
using NovaLive.Application.UseCases.FlashSales.Commands.RegisterSkuForFlashSale;
using NovaLive.Application.UseCases.FlashSales.Commands.RejectFlashSaleItem;
using NovaLive.Application.UseCases.FlashSales.Queries.GetActiveFlashSales;
using NovaLive.Application.UseCases.FlashSales.Queries.GetFlashSaleProducts;
using NovaLive.Application.UseCases.FlashSales.Queries.GetSellerFlashSaleRegistrations;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.FlashSales;
using NovaLive.Domain.Common;
using Xunit;

namespace NovaLive.Api.Tests.FlashSales;

public class FlashSalesControllersTests
{
    private readonly Mock<ISender> _senderMock = new();
    private readonly FlashSalesController _publicController;
    private readonly SellerFlashSalesController _sellerController;
    private readonly AdminFlashSalesController _adminController;

    public FlashSalesControllersTests()
    {
        _publicController = new FlashSalesController(_senderMock.Object);
        _sellerController = new SellerFlashSalesController(_senderMock.Object);
        _adminController = new AdminFlashSalesController(_senderMock.Object);
    }

    [Fact]
    public async Task GetActiveFlashSales_ShouldReturnOk()
    {
        IReadOnlyList<FlashSaleCampaignResponse> list = [];
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetActiveFlashSalesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(list));

        var result = await _publicController.GetActiveFlashSales(CancellationToken.None);

        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetFlashSaleProducts_ShouldReturnOk()
    {
        var campaignId = Guid.NewGuid();
        var paged = PagedResult<FlashSaleItemResponse>.Create([], 1, 20, 0);
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetFlashSaleProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(paged));

        var result = await _publicController.GetFlashSaleProducts(campaignId, 1, 20, CancellationToken.None);

        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RegisterSku_ShouldReturnCreated()
    {
        var campaignId = Guid.NewGuid();
        var req = new RegisterFlashSaleSkuRequest(Guid.NewGuid(), 80_000m, 10, 2);
        var responseDto = new FlashSaleItemResponse(
            Guid.NewGuid(), campaignId, req.SkuId, Guid.NewGuid(), "SPU", "SKU", "thumb", 100_000m, 80_000m, 10, 0, 0, 2, "Pending");

        _senderMock
            .Setup(s => s.Send(It.IsAny<RegisterSkuForFlashSaleCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(responseDto));

        var result = await _sellerController.RegisterSku(campaignId, req, CancellationToken.None);

        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task GetSellerRegistrations_ShouldReturnOk()
    {
        var paged = PagedResult<FlashSaleItemResponse>.Create([], 1, 20, 0);
        _senderMock
            .Setup(s => s.Send(It.IsAny<GetSellerFlashSaleRegistrationsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(paged));

        var result = await _sellerController.GetRegistrations(null, null, 1, 20, CancellationToken.None);

        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task CreateCampaign_ShouldReturnCreated()
    {
        var req = new CreateFlashSaleCampaignRequest("11.11", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddHours(2), null);
        var responseDto = new FlashSaleCampaignResponse(Guid.NewGuid(), "11.11", req.StartAt, req.EndAt, null, "Scheduled");

        _senderMock
            .Setup(s => s.Send(It.IsAny<CreateFlashSaleCampaignCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(responseDto));

        var result = await _adminController.CreateCampaign(req, CancellationToken.None);

        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task ApproveItem_ShouldReturnOk()
    {
        var itemId = Guid.NewGuid();
        var responseDto = new FlashSaleItemResponse(
            itemId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "SPU", "SKU", "thumb", 100_000m, 80_000m, 10, 0, 0, 2, "Approved");

        _senderMock
            .Setup(s => s.Send(It.IsAny<ApproveFlashSaleItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(responseDto));

        var result = await _adminController.ApproveItem(itemId, CancellationToken.None);

        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task RejectItem_ShouldReturnOk()
    {
        var itemId = Guid.NewGuid();
        var req = new RejectFlashSaleItemRequest("Giá chưa tốt");
        var responseDto = new FlashSaleItemResponse(
            itemId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "SPU", "SKU", "thumb", 100_000m, 80_000m, 10, 0, 0, 2, "Rejected", "Giá chưa tốt");

        _senderMock
            .Setup(s => s.Send(It.IsAny<RejectFlashSaleItemCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(responseDto));

        var result = await _adminController.RejectItem(itemId, req, CancellationToken.None);

        var objResult = result.Should().BeOfType<ObjectResult>().Subject;
        objResult.StatusCode.Should().Be(200);
    }
}
