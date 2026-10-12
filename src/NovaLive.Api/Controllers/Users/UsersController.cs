using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NovaLive.Api.Controllers.Common;
using NovaLive.Application.UseCases.Users.Addresses.Commands.CreateUserAddress;
using NovaLive.Application.UseCases.Users.Addresses.Commands.DeleteUserAddress;
using NovaLive.Application.UseCases.Users.Addresses.Commands.SetDefaultAddress;
using NovaLive.Application.UseCases.Users.Addresses.Commands.UpdateUserAddress;
using NovaLive.Application.UseCases.Users.Addresses.Queries.GetUserAddresses;
using NovaLive.Application.UseCases.Users.Profile.Commands.UpdateUserProfile;
using NovaLive.Application.UseCases.Users.Profile.Queries.GetUserProfile;
using NovaLive.Contracts.V1.Users;

namespace NovaLive.Api.Controllers.Users;

[Authorize]
[Route("api/v1/users")]
public sealed class UsersController(ISender sender) : ApiControllerBase(sender)
{
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(CancellationToken ct) =>
        Respond(await Sender.Send(new GetUserProfileQuery(), ct));

    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new UpdateUserProfileCommand(request), ct));

    [HttpGet("me/addresses")]
    public async Task<IActionResult> GetAddresses(CancellationToken ct) =>
        Respond(await Sender.Send(new GetUserAddressesQuery(), ct));

    [HttpPost("me/addresses")]
    public async Task<IActionResult> CreateAddress(
        [FromBody] CreateAddressRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new CreateUserAddressCommand(request), ct), StatusCodes.Status201Created);

    [HttpPut("me/addresses/{id:guid}")]
    public async Task<IActionResult> UpdateAddress(
        Guid id,
        [FromBody] UpdateAddressRequest request,
        CancellationToken ct) =>
        Respond(await Sender.Send(new UpdateUserAddressCommand(id, request), ct));

    [HttpDelete("me/addresses/{id:guid}")]
    public async Task<IActionResult> DeleteAddress(
        Guid id,
        CancellationToken ct) =>
        Respond(await Sender.Send(new DeleteUserAddressCommand(id), ct));

    [HttpPut("me/addresses/{id:guid}/default")]
    public async Task<IActionResult> SetDefaultAddress(
        Guid id,
        CancellationToken ct) =>
        Respond(await Sender.Send(new SetDefaultAddressCommand(id), ct));
}
