using System.Net.Http.Json;
using System.Text.Json;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;

namespace NovaLive.Infrastructure.Shipping.Ghn;

internal static class GhnTrackingReader
{
    public static async Task<Result<ShippingTrackingResult>> GetAsync(
        HttpClient client, string trackingCode, CancellationToken ct)
    {
        try
        {
            using var response = await client.GetAsync(
                "/shiip/public-api/v2/shipping-order/detail?order_code=" + Uri.EscapeDataString(trackingCode), ct);
            if (!response.IsSuccessStatusCode)
                return ShippingErrors.ProviderError("Shipping.ProviderError", "Không thể tra cứu hành trình GHN.");
            using var json = await response.Content.ReadFromJsonAsync<JsonDocument>(ct);
            if (json is null || !json.RootElement.TryGetProperty("code", out var code) || code.GetInt32() != 200
                || !json.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object
                || !data.TryGetProperty("status", out var status) || string.IsNullOrWhiteSpace(status.GetString()))
                return ShippingErrors.ProviderError("Shipping.ProviderError", "GHN trả về hành trình không hợp lệ.");
            var events = new List<TrackingEventResponse>();
            // Some GHN versions include a log; detail always exposes the current status.
            if (data.TryGetProperty("log", out var log) && log.ValueKind == JsonValueKind.Array)
                foreach (var entry in log.EnumerateArray())
                    if (entry.TryGetProperty("status", out var state) && state.ValueKind == JsonValueKind.String
                        && entry.TryGetProperty("updated_date", out var time) && time.TryGetDateTimeOffset(out var at))
                        events.Add(new(state.GetString()!, at.ToUniversalTime()));
            if (events.Count == 0 && data.TryGetProperty("updated_date", out var updated)
                && updated.TryGetDateTimeOffset(out var updatedAt))
                events.Add(new(status.GetString()!, updatedAt.ToUniversalTime()));
            return new ShippingTrackingResult(status.GetString()!, events.OrderBy(e => e.OccurredAt).ToArray());
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception)
        {
            return ShippingErrors.ProviderError("Shipping.ProviderError", "Không thể tra cứu hành trình GHN.");
        }
    }
}
