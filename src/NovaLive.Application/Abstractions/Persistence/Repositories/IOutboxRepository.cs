using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.System;
namespace NovaLive.Application.Abstractions.Persistence.Repositories;
public interface IOutboxRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken ct);
    Task<bool> HasVerifiedShipmentDeliveredEventAsync(Guid shipmentId, CancellationToken ct);
    Task<CheckoutResponse?> FindCheckoutReceiptAsync(string key, CancellationToken ct);
}
