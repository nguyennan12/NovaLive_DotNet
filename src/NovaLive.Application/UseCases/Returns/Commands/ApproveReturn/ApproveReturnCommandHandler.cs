using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
namespace NovaLive.Application.UseCases.Returns.Commands.ApproveReturn;
public sealed class ApproveReturnCommandHandler(SellerReturnAccess access, IShopAddressRepository addresses, ReturnReader reader, IDateTimeProvider clock)
    : ICommandHandler<ApproveReturnCommand, OrderReturnResponse>
{
    public async Task<Result<OrderReturnResponse>> Handle(ApproveReturnCommand command, CancellationToken ct)
    {
        var loaded = await access.GetForUpdateAsync(command.Id, ct);
        if (loaded.IsFailure) return loaded.Error;
        var context = loaded.Value!;
        var address = await addresses.GetReturnAddressAsync(context.SubOrder.ShopId, ct);
        if (address is null || address.ShopId != context.SubOrder.ShopId || !address.IsReturn) return ReturnErrors.ReturnAddressRequired;
        var snapshot = new ShopAddressResponse(address.Id, address.WarehouseName, address.ContactName, address.ContactPhone,
            address.ProvinceName, address.DistrictName, address.WardName, address.DetailAddress, address.IsPrimary,
            address.IsReturn, address.ProvinceId, address.DistrictId, address.WardCode);
        var result = context.Request.Approve(ReturnMappings.SerializeDecision(new(command.Request.ReasonNote, [], snapshot)), clock.UtcNow);
        if (result.IsFailure) return result.Error;
        return (await reader.DetailAsync(context.Request, ct))!;
    }
}
