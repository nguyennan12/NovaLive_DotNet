using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Wallets.Commands.CreatePayoutRequest;
using NovaLive.Application.UseCases.Wallets.Queries.GetShopWallet;
using NovaLive.Application.UseCases.Wallets.Queries.GetWalletTransactions;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Shops;

namespace NovaLive.Api.Controllers.Shops;

[Authorize(Roles = "Seller")]
[Route("api/v1/seller/wallet")]
[Route("seller/wallet")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
public sealed class WalletController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ShopWalletResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShopWallet(CancellationToken ct) =>
        Respond(await Sender.Send(new GetShopWalletQuery(), ct));

    [HttpGet("transactions")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<WalletTransactionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWalletTransactions(
        [FromQuery] GetWalletTransactionsRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new GetWalletTransactionsQuery(request), ct));

    [HttpPost("payout")]
    [ProducesResponseType(typeof(ApiResponse<SellerPayoutResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePayoutRequest(
        [FromBody] CreatePayoutRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new CreatePayoutRequestCommand(request), ct));
}
