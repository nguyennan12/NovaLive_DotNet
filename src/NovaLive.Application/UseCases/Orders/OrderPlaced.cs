using System.Globalization;
using System.Net;
using System.Reflection;
using NovaLive.Contracts.V1.Orders;

namespace NovaLive.Application.UseCases.Orders;

public sealed record OrderPlacedEvent(Guid ParentOrderId, string OrderCode, DateTimeOffset PlacedAt, Guid BuyerId,
    IReadOnlyList<OrderPlacedShop> SubOrders, string PaymentMethod, decimal GrandTotal,
    string IdempotencyKey, CheckoutResponse Receipt);

public sealed record OrderPlacedShop(Guid SubOrderId, string SubOrderCode, Guid ShopId, int ItemCount, decimal TotalAmount,
    IReadOnlyList<OrderPlacedItem>? Items);

public sealed record OrderPlacedItem(string ProductName, string SkuCode, string? ThumbnailUrl,
    int Quantity, decimal UnitPrice, decimal LineTotal);

public static class OrderPlacedEmailTemplate
{
    private const string ResourceName = "NovaLive.Application.Common.Templates.OrderPlaced.html";
    private static readonly CultureInfo VietnameseCulture = CultureInfo.GetCultureInfo("vi-VN");
    private static readonly string Template = LoadTemplate();

    public static string Render(OrderPlacedEvent order)
    {
        return Template
            .Replace("{{ORDER_CODE}}", Encode(order.OrderCode), StringComparison.Ordinal)
            .Replace("{{PLACED_AT}}", order.PlacedAt == default ? "" : order.PlacedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm", VietnameseCulture), StringComparison.Ordinal)
            .Replace("{{PAYMENT_METHOD}}", Encode(order.PaymentMethod), StringComparison.Ordinal)
            .Replace("{{GRAND_TOTAL}}", FormatMoney(order.GrandTotal), StringComparison.Ordinal)
            .Replace("{{SUBORDERS}}", RenderSubOrders(order.SubOrders), StringComparison.Ordinal);
    }

    private static string RenderSubOrders(IEnumerable<OrderPlacedShop> subOrders) =>
        string.Join("", subOrders.Select(subOrder =>
        {
            var items = string.Join("", (subOrder.Items ?? []).Select(RenderItem));
            return $"<table role=\"presentation\" width=\"100%\" cellspacing=\"0\" cellpadding=\"0\" style=\"border-collapse:collapse;margin:0 0 14px;background:#fafafa;\">" +
                $"<tr><td colspan=\"3\" style=\"padding:14px 16px 8px;font-size:13px;font-weight:bold;color:#555;\">Đơn shop {Encode(subOrder.SubOrderCode)}</td></tr>" +
                $"{items}" +
                $"<tr><td colspan=\"3\" align=\"right\" style=\"padding:12px 16px;font-size:14px;color:#555;\">Tổng shop: <strong style=\"color:#222;\">{FormatMoney(subOrder.TotalAmount)}</strong></td></tr></table>";
        }));

    private static string RenderItem(OrderPlacedItem item)
    {
        var image = TryGetSafeImageUrl(item.ThumbnailUrl) is { } imageUrl
            ? $"<img src=\"{Encode(imageUrl)}\" width=\"52\" height=\"52\" alt=\"\" style=\"display:block;width:52px;height:52px;object-fit:cover;border-radius:4px;\">"
            : "<span style=\"display:block;width:52px;height:52px;background:#eee;border-radius:4px;\"></span>";
        return $"<tr><td width=\"76\" style=\"padding:12px 8px 12px 16px;\">{image}</td>" +
            $"<td style=\"padding:12px 8px;font-size:14px;line-height:1.5;\"><strong>{Encode(item.ProductName)}</strong><br><span style=\"color:#888;font-size:12px;\">SKU: {Encode(item.SkuCode)} · SL: {item.Quantity}</span></td>" +
            $"<td align=\"right\" style=\"padding:12px 16px 12px 8px;white-space:nowrap;font-size:14px;font-weight:bold;\">{FormatMoney(item.LineTotal)}</td></tr>";
    }

    private static string? TryGetSafeImageUrl(string? value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
            ? uri.AbsoluteUri
            : null;

    private static string FormatMoney(decimal value) => $"{value.ToString("N0", VietnameseCulture)} ₫";
    private static string Encode(string value) => WebUtility.HtmlEncode(value);

    private static string LoadTemplate()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"Embedded email template '{ResourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
