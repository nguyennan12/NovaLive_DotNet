using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Api.Hubs;
using NovaLive.Api.Tests.Auth;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Orders;
using NovaLive.Infrastructure.Persistence;

namespace NovaLive.Api.Tests.Payments;

[Collection("API host lifecycle")]
public sealed class PaymentHubWebSocketTests
{
    [Fact]
    public async Task JwtSubjectMapsBuyerConnections_AndOtherBuyersDoNotReceivePaymentSuccess()
    {
        using var factory = new AuthApplicationFactory();
        using var app = factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
            // Exercise the real WebSocket transport without an external Redis server.
            services.AddSingleton<HubLifetimeManager<PaymentNotificationHub>, DefaultHubLifetimeManager<PaymentNotificationHub>>()));
        using var http = app.CreateClient();
        var buyer = Guid.NewGuid(); var otherBuyer = Guid.NewGuid();
        var order = ParentOrder.Create(buyer, "NOVA-WS-1", "{}", 100, 0, 0, 100, null, factory.Clock.UtcNow);
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ParentOrders.Add(order); await db.SaveChangesAsync();
        }
        async Task<WebSocket> Connect(Guid userId)
        {
            using var scope = app.Services.CreateScope();
            var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>()
                .Create(userId, "buyer@example.com", ["Buyer"], null).Value;
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var socket = await app.Server.CreateWebSocketClient()
                .ConnectAsync(new Uri($"ws://localhost/hubs/payment?access_token={token}"), timeout.Token);
            await socket.SendAsync(Encoding.UTF8.GetBytes("{\"protocol\":\"json\",\"version\":1}\u001e"),
                WebSocketMessageType.Text, true, timeout.Token);
            await ReadFrame(socket, timeout.Token); // SignalR handshake acknowledgement
            return socket;
        }
        using var buyerSocket = await Connect(buyer);
        using var secondBuyerSocket = await Connect(buyer);
        using var otherSocket = await Connect(otherBuyer);
        var paymentId = Guid.NewGuid();
        using (var scope = app.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<IPaymentNotificationService>().NotifyPaymentSuccessAsync(
                new PaymentSuccessEvent(paymentId, order.Id, order.OrderCode, 100, "VietQR", "TX", factory.Clock.UtcNow));
        using var receiveTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        foreach (var socket in new[] { buyerSocket, secondBuyerSocket })
        {
            var frame = await ReadFrame(socket, receiveTimeout.Token);
            using var json = JsonDocument.Parse(frame.TrimEnd('\u001e'));
            json.RootElement.GetProperty("target").GetString().Should().Be("PaymentSuccess");
            var payload = json.RootElement.GetProperty("arguments")[0];
            payload.GetProperty("orderId").GetGuid().Should().Be(order.Id);
            payload.GetProperty("paymentId").GetGuid().Should().Be(paymentId);
        }
        using var noMessageTimeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        await FluentActions.Awaiting(() => ReadFrame(otherSocket, noMessageTimeout.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    private static async Task<string> ReadFrame(WebSocket socket, CancellationToken ct)
    {
        var bytes = new byte[8192];
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(bytes), ct);
        return Encoding.UTF8.GetString(bytes, 0, result.Count);
    }
}
