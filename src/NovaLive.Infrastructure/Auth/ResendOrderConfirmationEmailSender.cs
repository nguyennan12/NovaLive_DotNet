using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.Services;

namespace NovaLive.Infrastructure.Auth;

public sealed class ResendOrderConfirmationEmailSender(HttpClient client, IOptions<EmailOptions> options)
    : IOrderConfirmationEmailSender
{
    public async Task SendAsync(string email, string html, CancellationToken ct = default)
    {
        var settings = options.Value;
        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        request.Content = JsonContent.Create(new
        {
            from = string.IsNullOrWhiteSpace(settings.FromName) ? settings.FromAddress
                : $"{settings.FromName} <{settings.FromAddress}>",
            to = new[] { email },
            subject = "NovaLive — Xác nhận đơn hàng",
            html
        });
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException("Order confirmation email delivery failed.", null, response.StatusCode);
    }
}
