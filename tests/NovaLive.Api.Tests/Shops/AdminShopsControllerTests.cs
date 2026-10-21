using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers.Shops;
using NovaLive.Application.UseCases.Shops.Commands.ApproveShop;
using NovaLive.Application.UseCases.Shops.Commands.BanShop;
using NovaLive.Application.UseCases.Shops.Queries.GetAdminShops;
using NovaLive.Application.UseCases.Wallets.Commands.ApprovePayout;
using NovaLive.Application.UseCases.Wallets.Commands.RejectPayout;
using NovaLive.Application.UseCases.Wallets.Queries.GetAdminPayouts;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Tests.Shops;

public sealed class AdminShopsControllerTests
{
    private readonly Mock<ISender> _sender = new();
    private readonly AdminShopsController _controller;

    public AdminShopsControllerTests()
    {
        _controller = new AdminShopsController(_sender.Object);
    }

    [Fact]
    public async Task GetShops_DispatchesQuery_ReturnsOk()
    {
        var request = new GetAdminShopsRequest(Status: "Pending", Page: 1, Size: 10);
        var pagedResult = PagedResult<ShopResponse>.Create([], 1, 10, 0);

        _sender
            .Setup(s => s.Send(It.Is<GetAdminShopsQuery>(q => q.Request == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedResult));

        var result = await _controller.GetShops(request, default);

        var okResult = result.Should().BeOfType<ObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<PagedResult<ShopResponse>>>().Subject;
        apiResponse.Data.Should().BeEquivalentTo(pagedResult);
    }

    [Fact]
    public async Task ApproveShop_DispatchesCommand_ReturnsOk()
    {
        var shopId = Guid.NewGuid();
        var request = new ApproveShopRequest(Note: "Hồ sơ hợp lệ");
        var expectedResponse = new ShopResponse(
            shopId,
            Guid.NewGuid(),
            "Shop Test",
            "shop-test",
            "Mô tả",
            null,
            null,
            "0912345678",
            "test@example.com",
            "Active",
            5.0,
            0,
            DateTime.UtcNow,
            "Approved",
            "Hồ sơ hợp lệ");

        _sender
            .Setup(s => s.Send(It.Is<ApproveShopCommand>(c => c.ShopId == shopId && c.Request == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedResponse));

        var result = await _controller.ApproveShop(shopId, request, default);

        var okResult = result.Should().BeOfType<ObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<ShopResponse>>().Subject;
        apiResponse.Data.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task BanShop_DispatchesCommand_ReturnsOk()
    {
        var shopId = Guid.NewGuid();
        var request = new BanShopRequest(Reason: "Vi phạm chính sách");
        var expectedResponse = new ShopResponse(
            shopId,
            Guid.NewGuid(),
            "Shop Test",
            "shop-test",
            "Mô tả",
            null,
            null,
            "0912345678",
            "test@example.com",
            "Banned",
            5.0,
            0,
            DateTime.UtcNow);

        _sender
            .Setup(s => s.Send(It.Is<BanShopCommand>(c => c.ShopId == shopId && c.Request == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedResponse));

        var result = await _controller.BanShop(shopId, request, default);

        var okResult = result.Should().BeOfType<ObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<ShopResponse>>().Subject;
        apiResponse.Data.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetPayouts_DispatchesQuery_ReturnsOk()
    {
        var request = new GetAdminPayoutsRequest(Status: "Pending", Page: 1, Size: 10);
        var pagedResult = PagedResult<SellerPayoutResponse>.Create([], 1, 10, 0);

        _sender
            .Setup(s => s.Send(It.Is<GetAdminPayoutsQuery>(q => q.Request == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedResult));

        var result = await _controller.GetPayouts(request, default);

        var okResult = result.Should().BeOfType<ObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<PagedResult<SellerPayoutResponse>>>().Subject;
        apiResponse.Data.Should().BeEquivalentTo(pagedResult);
    }

    [Fact]
    public async Task ApprovePayout_DispatchesCommand_ReturnsOk()
    {
        var payoutId = Guid.NewGuid();
        var request = new ApprovePayoutRequest(TransferRef: "BANK-TXN-123");
        var expectedResponse = new SellerPayoutResponse(
            payoutId,
            Guid.NewGuid(),
            100_000m,
            "Vietcombank",
            "0123456789",
            "Chủ Shop",
            "Completed",
            "BANK-TXN-123",
            null,
            DateTime.UtcNow);

        _sender
            .Setup(s => s.Send(It.Is<ApprovePayoutCommand>(c => c.PayoutId == payoutId && c.Request == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedResponse));

        var result = await _controller.ApprovePayout(payoutId, request, default);

        var okResult = result.Should().BeOfType<ObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<SellerPayoutResponse>>().Subject;
        apiResponse.Data.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task RejectPayout_DispatchesCommand_ReturnsOk()
    {
        var payoutId = Guid.NewGuid();
        var request = new RejectPayoutRequest(Reason: "Tài khoản ngân hàng không tồn tại");
        var expectedResponse = new SellerPayoutResponse(
            payoutId,
            Guid.NewGuid(),
            100_000m,
            "Vietcombank",
            "0123456789",
            "Chủ Shop",
            "Failed",
            null,
            "Tài khoản ngân hàng không tồn tại",
            DateTime.UtcNow);

        _sender
            .Setup(s => s.Send(It.Is<RejectPayoutCommand>(c => c.PayoutId == payoutId && c.Request == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedResponse));

        var result = await _controller.RejectPayout(payoutId, request, default);

        var okResult = result.Should().BeOfType<ObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<SellerPayoutResponse>>().Subject;
        apiResponse.Data.Should().BeEquivalentTo(expectedResponse);
    }
}
