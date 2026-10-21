using NovaLive.Application.Common.Messaging;

namespace NovaLive.Application.UseCases.Inventory.Commands.ConfirmInventorySale;

public sealed record ConfirmInventorySaleCommand(Guid PaymentId) : ICommand, ITransactionalCommand, IIdempotentCommand
{
    public string IdempotencyKey => $"inventory-sale:{PaymentId:N}";
}
