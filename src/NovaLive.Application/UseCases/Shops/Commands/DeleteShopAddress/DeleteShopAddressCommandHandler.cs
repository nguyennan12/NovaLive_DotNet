using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shops.Commands.DeleteShopAddress;

public sealed class DeleteShopAddressCommandHandler(IShopRepository shops, ShopAccess access, IUnitOfWork unitOfWork) : ICommandHandler<DeleteShopAddressCommand>
{
    public async Task<Result> Handle(DeleteShopAddressCommand command, CancellationToken ct)
    {
        var owned = await access.GetSellerOwnedAsync(true, ct);
        if (owned.IsFailure) return owned.Error;
        var addresses = await shops.GetAddressesForDeletionAsync(owned.Value!.Id, ct);
        var address = addresses.SingleOrDefault(a => a.Id == command.Id);
        if (address is null) return ShopErrors.AddressNotFound;
        if (addresses.Count == 1 && address.IsPrimary) return ShopErrors.LastPrimary;
        var wasPrimary = address.IsPrimary;
        var wasReturn = address.IsReturn;
        shops.DeleteAddress(address);
        // Delete first to free the unique default indexes, still in the same transaction.
        await unitOfWork.SaveChangesAsync(ct);
        var remaining = addresses.Where(a => a.Id != address.Id).ToList();
        if (wasPrimary && remaining.Count > 0) remaining[0].SetPrimary(true);
        // Default returns fall back to the primary warehouse when the selected return warehouse is deleted.
        if (wasReturn && remaining.Count > 0)
            (remaining.FirstOrDefault(a => a.IsPrimary) ?? remaining[0]).SetReturn(true);
        return Result.Success();
    }
}
