using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.System;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class OutboxRepository(AppDbContext db) : IOutboxRepository
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public async Task AddAsync(OutboxMessage entity, CancellationToken ct) => await db.OutboxMessages.AddAsync(entity, ct);
    public Task<bool> HasCodSettlementEligibleShipmentDeliveredEventAsync(Guid shipmentId, CancellationToken ct)
    {
        var shipmentIdText = shipmentId.ToString();

        if (db.Database.IsNpgsql())
        {
            // Payload is jsonb in PostgreSQL. Using string.Contains here makes Npgsql
            // translate the predicate as a text operation against jsonb and fails at
            // runtime. Query the JSON properties explicitly instead.
            return db.OutboxMessages.FromSqlInterpolated($"""
                SELECT *
                FROM public.outbox_messages
                WHERE event_type = 'ShipmentDeliveredEvent'
                  AND payload->>'shippingOrderId' = {shipmentIdText}
                  AND (
                    COALESCE((payload->>'isVerifiedWebhook')::boolean, false)
                    OR COALESCE((payload->>'isSellerConfirmedDelivery')::boolean, false)
                  )
                """)
                .AsNoTracking()
                .AnyAsync(ct);
        }

        const string verifiedSignal = "\"isVerifiedWebhook\":true";
        const string sellerConfirmedSignal = "\"isSellerConfirmedDelivery\":true";
        return db.OutboxMessages.AsNoTracking().AnyAsync(message =>
            message.EventType == nameof(ShipmentDeliveredEvent)
            && message.Payload.Contains(shipmentIdText)
            && (message.Payload.Contains(verifiedSignal) || message.Payload.Contains(sellerConfirmedSignal)), ct);
    }

    public async Task<CheckoutResponse?> FindCheckoutReceiptAsync(string key, CancellationToken ct)
    {
        IQueryable<OutboxMessage> query = db.OutboxMessages.AsNoTracking();
        if (db.Database.IsNpgsql())
            query = db.OutboxMessages.FromSqlInterpolated(
                $"SELECT * FROM public.outbox_messages WHERE event_type = 'OrderPlacedEvent' AND payload->>'idempotencyKey' = {key}").AsNoTracking();
        else query = query.Where(m => m.EventType == "OrderPlacedEvent" && m.Payload.Contains(key));
        var payload = await query.Select(m => m.Payload).FirstOrDefaultAsync(ct);
        return payload is null ? null : JsonSerializer.Deserialize<OrderPlacedEvent>(payload, Json)?.Receipt;
    }
}
