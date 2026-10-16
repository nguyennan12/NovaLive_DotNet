using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.UseCases.Orders;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.System;
namespace NovaLive.Infrastructure.Persistence.Repositories;
public sealed class OutboxRepository(AppDbContext db) : IOutboxRepository
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public async Task AddAsync(OutboxMessage entity, CancellationToken ct) => await db.OutboxMessages.AddAsync(entity, ct);
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
