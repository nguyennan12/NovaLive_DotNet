using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Api.Tests.Auth;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Contracts.Common;
using NovaLive.Contracts.V1.Payments;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Payments;
using NovaLive.Domain.Rbac;
using NovaLive.Domain.Users;
using NovaLive.Infrastructure.Persistence;

namespace NovaLive.Api.Tests.Payments;

[Collection("API host lifecycle")]
public sealed class PaymentWorkflowHttpTests
{
    [Theory]
    [InlineData("/payments")]
    [InlineData("/api/v1/payments")]
    public async Task InitiatePollAndOwnership_WorkThroughRealPipeline(string route)
    {
        using var factory = new AuthApplicationFactory();
        using var client = factory.CreateClient();
        var buyer = new User("payment-buyer@example.com", "hash", "Buyer"); buyer.Activate();
        var other = new User("other-payment-buyer@example.com", "hash", "Other"); other.Activate();
        var now = factory.Clock.UtcNow;
        now = new DateTimeOffset(now.Ticks - now.Ticks % TimeSpan.TicksPerMillisecond, now.Offset);
        factory.Clock.UtcNow = now;
        var order = ParentOrder.Create(buyer.Id, "NOVA-PAY-1", "{}", 100, 0, 0, 100, null, now);
        var payment = Payment.Create(order.Id, PaymentMethod.VietQR, 100, now);
        var codOrder = ParentOrder.Create(buyer.Id, "NOVA-COD-1", "{}", 100, 0, 0, 100, null, now);
        var cod = Payment.Create(codOrder.Id, PaymentMethod.COD, 100, now);
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Users.AddRange(buyer, other);
            db.UserRoles.AddRange(new UserRole(buyer.Id, SystemRoleIds.Buyer, now), new UserRole(other.Id, SystemRoleIds.Buyer, now));
            db.ParentOrders.AddRange(order, codOrder); db.Payments.AddRange(payment, cod);
            await db.SaveChangesAsync();
        }
        void Authenticate(Guid id)
        {
            using var scope = factory.Services.CreateScope();
            var token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>().Create(id, "buyer@example.com", ["Buyer"], null);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);
        }
        (await client.GetAsync($"{route}/{payment.Id}/status")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        Authenticate(buyer.Id);
        var initiate = await client.PostAsJsonAsync($"{route}/initiate", new InitiatePaymentRequest(order.Id, "VietQR"));
        initiate.StatusCode.Should().Be(HttpStatusCode.OK);
        var init = (await initiate.Content.ReadFromJsonAsync<ApiResponse<PaymentInitResponse>>())!.Data!;
        init.PaymentId.Should().Be(payment.Id);
        init.QrPayload.Should().Contain("NOVA-PAY-1");
        init.QrImageBase64.Should().NotBeNullOrEmpty();
        init.ExpiredAt.Should().Be(now.AddMinutes(15));
        var status = await client.GetAsync($"{route}/{payment.Id}/status");
        status.Headers.CacheControl!.NoStore.Should().BeTrue();
        (await status.Content.ReadFromJsonAsync<ApiResponse<PaymentStatusResponse>>())!.Data!.Status.Should().Be("Pending");
        var codResult = await client.PostAsJsonAsync($"{route}/initiate", new InitiatePaymentRequest(codOrder.Id, "COD"));
        var codInit = (await codResult.Content.ReadFromJsonAsync<ApiResponse<PaymentInitResponse>>())!.Data!;
        codInit.Status.Should().Be("Pending"); codInit.ExpiredAt.Should().BeNull(); codInit.PaymentUrl.Should().BeNull();
        Authenticate(other.Id);
        (await client.PostAsJsonAsync($"{route}/initiate", new InitiatePaymentRequest(order.Id, "VietQR")))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.GetAsync($"{route}/{payment.Id}/status")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        Authenticate(buyer.Id);
        factory.Clock.UtcNow = now.AddMinutes(15);
        (await client.PostAsJsonAsync($"{route}/initiate", new InitiatePaymentRequest(order.Id, "VietQR")))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task QueryStringToken_IsAcceptedOnlyOnHubRoutes()
    {
        using var factory = new AuthApplicationFactory();
        using var client = factory.CreateClient();
        string token;
        using (var scope = factory.Services.CreateScope())
            token = scope.ServiceProvider.GetRequiredService<IJwtTokenService>()
                .Create(Guid.NewGuid(), "buyer@example.com", ["Buyer"], null).Value;
        var negotiate = await client.PostAsync($"/hubs/payment/negotiate?negotiateVersion=1&access_token={token}", null);
        negotiate.StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync($"/api/v1/payments/{Guid.NewGuid()}/status?access_token={token}"))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await client.PostAsync("/hubs/payment/negotiate?negotiateVersion=1&access_token=invalid", null))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
