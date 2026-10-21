using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NovaLive.Api.Controllers.Shops;
using NovaLive.Application.UseCases.Wallets.Commands.CreatePayoutRequest;
using NovaLive.Application.UseCases.Wallets.Queries.GetShopWallet;
using NovaLive.Application.UseCases.Wallets.Queries.GetWalletTransactions;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;

namespace NovaLive.Api.Tests.Shops;

public sealed class WalletControllerTests
{
    private readonly Mock<ISender> _sender = new();
    private readonly WalletController _controller;

    public WalletControllerTests()
    {
        _controller = new WalletController(_sender.Object);
    }

    [Fact]
    public async Task GetShopWallet_DispatchesQuery_ReturnsOk()
    {
        var expectedResponse = new ShopWalletResponse(Guid.NewGuid(), 1_000_000m, 50_000m, DateTime.UtcNow);
        _sender
            .Setup(s => s.Send(It.IsAny<GetShopWalletQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedResponse));

        var result = await _controller.GetShopWallet(default);

        var okResult = result.Should().BeOfType<ObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<ShopWalletResponse>>().Subject;
        apiResponse.Data.Should().BeEquivalentTo(expectedResponse);
    }

    [Fact]
    public async Task GetWalletTransactions_DispatchesQuery_ReturnsOk()
    {
        var request = new GetWalletTransactionsRequest(Type: "PayoutLock", Page: 1, Size: 10);
        var pagedResult = PagedResult<WalletTransactionResponse>.Create([], 1, 10, 0);

        _sender
            .Setup(s => s.Send(It.Is<GetWalletTransactionsQuery>(q => q.Request == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(pagedResult));

        var result = await _controller.GetWalletTransactions(request, default);

        var okResult = result.Should().BeOfType<ObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<PagedResult<WalletTransactionResponse>>>().Subject;
        apiResponse.Data.Should().BeEquivalentTo(pagedResult);
    }

    [Fact]
    public async Task CreatePayoutRequest_DispatchesCommand_ReturnsOk()
    {
        var request = new CreatePayoutRequest(100_000m);
        var expectedResponse = new SellerPayoutResponse(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100_000m,
            "Vietcombank",
            "123456789",
            "Chủ Shop",
            "Pending",
            null,
            null,
            DateTime.UtcNow);

        _sender
            .Setup(s => s.Send(It.Is<CreatePayoutRequestCommand>(c => c.Request == request), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(expectedResponse));

        var result = await _controller.CreatePayoutRequest(request, default);

        var okResult = result.Should().BeOfType<ObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
        var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<SellerPayoutResponse>>().Subject;
        apiResponse.Data.Should().BeEquivalentTo(expectedResponse);
    }
}
