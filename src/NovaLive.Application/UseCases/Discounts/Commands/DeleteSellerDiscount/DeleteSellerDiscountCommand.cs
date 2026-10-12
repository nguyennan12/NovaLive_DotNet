using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Discounts.Commands.DeleteSellerDiscount;

public record DeleteSellerDiscountCommand(Guid Id)
    : ICommand, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Discounts.ManageOwnShop;
}
