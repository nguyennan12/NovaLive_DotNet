using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Inventory;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Inventory.Commands.AdjustInventory;

public record AdjustInventoryCommand(AdjustInventoryRequest Request)
    : ICommand<InventoryResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Inventory.ManageOwn;
}
