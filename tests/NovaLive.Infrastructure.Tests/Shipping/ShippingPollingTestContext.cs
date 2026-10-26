using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MediatR;
using Moq;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Abstractions.Services;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Domain.Orders;
using NovaLive.Domain.Pricing;
using NovaLive.Domain.Shipping;
using NovaLive.Infrastructure.Shipping;

namespace NovaLive.Infrastructure.Tests.Shipping;

public sealed class ShippingPollingTestContext : IDisposable
{
    public DateTimeOffset Now { get; } = DateTimeOffset.Parse("2026-10-17T00:00:00Z");
    public Mock<IShippingOrderRepository> Shipments { get; } = new();
    public Mock<ISubOrderRepository> Subs { get; } = new();
    public Mock<IShippingProvider> Provider { get; } = new();
    public Mock<ISender> Sender { get; } = new();
    public List<ShippingOrder> Candidates { get; } = [];
    public List<SubOrder> SubOrders { get; } = [];
    private readonly ServiceProvider services;
    public ShippingStatusPollingWorker Worker { get; }

    public ShippingPollingTestContext()
    {
        var clock = new Mock<IDateTimeProvider>(); clock.SetupGet(c => c.UtcNow).Returns(Now);
        Shipments.Setup(s => s.GetStaleInTransitAsync(Now.AddDays(-3), It.IsAny<CancellationToken>())).ReturnsAsync(Candidates);
        Subs.Setup(s => s.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => SubOrders.SingleOrDefault(s => s.Id == id));
        var resolver = new Mock<IShippingProviderResolver>();
        resolver.Setup(r => r.Resolve("GHN")).Returns(Result<IShippingProvider>.Success(Provider.Object));
        Provider.Setup(p => p.GetTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingTrackingResult("delivered", [new("delivered", Now.AddHours(-1))]));
        Sender.Setup(s => s.Send(It.IsAny<NovaLive.Application.UseCases.Shipping.Commands.Webhook.HandleShippingWebhookCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());
        services = new ServiceCollection().AddSingleton(Shipments.Object).AddSingleton(Subs.Object)
            .AddSingleton(resolver.Object).AddSingleton(Sender.Object).BuildServiceProvider();
        Worker = new ShippingStatusPollingWorker(services.GetRequiredService<IServiceScopeFactory>(), clock.Object,
            NullLogger<ShippingStatusPollingWorker>.Instance);
    }

    public ShippingOrder Add(int ageDays, ShippingStatus status = ShippingStatus.Delivering, bool shippingSub = true)
    {
        var sub = SubOrder.Create(Guid.NewGuid(), "SUB-" + SubOrders.Count, Guid.NewGuid(),
            new ShopCalculationResult(Guid.NewGuid(), 100, 10, 0, 0, 0, 100, 110, []), true, Now.AddDays(-5));
        if (shippingSub) sub.MarkAsShipping(Now.AddDays(-4));
        var shipment = new ShippingOrder(sub.Id, ShippingProvider.GHN, "2", "{}", "{}", 0, 10, 100,
            "TRACK-" + Candidates.Count, createdAt: Now.AddDays(-ageDays));
        shipment.UpdateStatusFromWebhook(status, "{}", Now.AddDays(-ageDays));
        SubOrders.Add(sub); Candidates.Add(shipment); return shipment;
    }

    public void Dispose() { Worker.Dispose(); services.Dispose(); }
}
