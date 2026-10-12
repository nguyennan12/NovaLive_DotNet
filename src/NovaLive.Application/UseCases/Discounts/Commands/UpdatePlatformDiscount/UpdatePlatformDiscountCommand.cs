using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Discounts;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Discounts.Commands.UpdatePlatformDiscount;

public record UpdatePlatformDiscountCommand(Guid Id, UpdateDiscountRequest Request)
    : ICommand<DiscountResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Discounts.ManagePlatform;
}
