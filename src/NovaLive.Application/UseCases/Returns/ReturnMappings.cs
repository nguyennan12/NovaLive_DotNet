using System.Text.Json;
using NovaLive.Contracts.V1.Returns;
using NovaLive.Domain.Orders;
namespace NovaLive.Application.UseCases.Returns;
public static class ReturnMappings
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public static string SerializeDecision(ReturnDecisionSnapshot response) => JsonSerializer.Serialize(response, Json);
    private static ReturnDecisionSnapshot Decision(OrderReturn entity)
    {
        if (entity.SellerResponse is null) return new(null, [], null);
        try { return JsonSerializer.Deserialize<ReturnDecisionSnapshot>(entity.SellerResponse, Json) ?? new(null, [], null); }
        catch (JsonException) { return new(entity.SellerResponse, [], null); }
    }
    public static OrderReturnResponse ToResponse(OrderReturn entity, SubOrder sub,
        IEnumerable<OrderReturnItem> lines, IEnumerable<OrderItem> purchased)
    {
        var byId = purchased.ToDictionary(i => i.Id);
        var decision = Decision(entity);
        return new(entity.Id, sub.Id, sub.SubOrderCode, entity.BuyerId, entity.Reason.ToString(), entity.Status.ToString(),
            entity.RefundAmount, entity.Status is NovaLive.Domain.Common.ReturnStatus.SellerApproved or NovaLive.Domain.Common.ReturnStatus.Completed
                ? entity.RefundAmount : null,
            entity.CreatedAt.UtcDateTime, lines.Where(i => i.ReturnId == entity.Id).Select(i =>
            {
                byId.TryGetValue(i.OrderItemId, out var orderItem);
                var name = ""; var code = "";
                if (orderItem != null)
                {
                    try
                    {
                        using var snapshot = JsonDocument.Parse(orderItem.SkuSnapshotJson);
                        foreach (var property in snapshot.RootElement.EnumerateObject())
                        {
                            if (property.Name.Equals("spuName", StringComparison.OrdinalIgnoreCase)) name = property.Value.GetString() ?? "";
                            if (property.Name.Equals("skuCode", StringComparison.OrdinalIgnoreCase)) code = property.Value.GetString() ?? "";
                        }
                    }
                    catch (JsonException) { }
                }
                var amount = orderItem == null ? 0 : ReturnRefundCalculator.Calculate(orderItem, i.Quantity).Value;
                return new ReturnItemDto(i.OrderItemId, name, code, i.Quantity, amount);
            }).ToArray(), entity.EvidenceUrls.ToArray(), decision.ReturnAddress, decision.Note, decision.EvidenceUrls);
    }
}
