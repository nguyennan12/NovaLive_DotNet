using FluentAssertions;
using Moq;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.UseCases.Shipping;
using NovaLive.Application.UseCases.Shipping.Commands.Webhook;
using NovaLive.Domain.Common;
using NovaLive.Infrastructure.Shipping;

namespace NovaLive.Infrastructure.Tests.Shipping;

public sealed class ShippingStatusPollingWorkerTests
{
    [Theory]
    [InlineData(ShippingStatus.Picking)]
    [InlineData(ShippingStatus.Delivering)]
    public async Task StaleShipment_UsesSharedCommandWithProviderTimestamp(ShippingStatus status)
    {
        using var c = new ShippingPollingTestContext(); var shipment = c.Add(4, status);
        await c.Worker.RunOnceAsync(default);
        c.Sender.Verify(s => s.Send(It.Is<HandleShippingWebhookCommand>(cmd => cmd.Provider == ShippingProvider.GHN
            && cmd.TrackingCode == shipment.TrackingCode && cmd.Status == ShippingStatus.Delivered
            && cmd.EventTimestamp == c.Now.AddHours(-1)), It.IsAny<CancellationToken>()), Times.Once);
        ShippingStatusPollingWorker.Interval.Should().Be(TimeSpan.FromHours(6));
    }

    [Fact]
    public async Task FreshBoundaryTerminalAndNonShippingSubOrders_AreSkipped()
    {
        using var c = new ShippingPollingTestContext(); c.Add(2); c.Add(3);
        c.Add(4, ShippingStatus.ReadyToPick); c.Add(4, ShippingStatus.Delivered);
        c.Add(4, ShippingStatus.Delivering, false);
        await c.Worker.RunOnceAsync(default);
        c.Provider.Verify(p => p.GetTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        c.Sender.Verify(s => s.Send(It.IsAny<HandleShippingWebhookCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CarrierFailure_IsRetriedAndDoesNotStopNextShipment(bool throws)
    {
        using var c = new ShippingPollingTestContext(); var failed = c.Add(4); var good = c.Add(5);
        var setup = c.Provider.Setup(p => p.GetTrackingAsync(failed.TrackingCode!, It.IsAny<CancellationToken>()));
        if (throws) setup.ThrowsAsync(new HttpRequestException("offline"));
        else setup.ReturnsAsync(ShippingErrors.ProviderError("GHN.Offline", "offline"));
        await c.Worker.RunOnceAsync(default);
        c.Provider.Verify(p => p.GetTrackingAsync(failed.TrackingCode!, It.IsAny<CancellationToken>()), Times.Exactly(3));
        c.Sender.Verify(s => s.Send(It.Is<HandleShippingWebhookCommand>(cmd => cmd.TrackingCode == good.TrackingCode),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task TemporaryFailure_RecoversDuringSameRun()
    {
        using var c = new ShippingPollingTestContext(); var shipment = c.Add(4);
        c.Provider.SetupSequence(p => p.GetTrackingAsync(shipment.TrackingCode!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ShippingErrors.ProviderError("GHN.Offline", "offline"))
            .ReturnsAsync(new ShippingTrackingResult("delivered", []));
        await c.Worker.RunOnceAsync(default);
        c.Sender.Verify(s => s.Send(It.Is<HandleShippingWebhookCommand>(cmd => cmd.EventTimestamp == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UnknownCarrierState_DoesNotInventDeliveredStatus()
    {
        using var c = new ShippingPollingTestContext(); c.Add(4);
        c.Provider.Setup(p => p.GetTrackingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ShippingTrackingResult("unknown-provider-status", []));
        await c.Worker.RunOnceAsync(default);
        c.Sender.Verify(s => s.Send(It.IsAny<HandleShippingWebhookCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Cancellation_IsPropagated()
    {
        using var c = new ShippingPollingTestContext(); c.Add(4);
        using var stop = new CancellationTokenSource(); stop.Cancel();
        var act = () => c.Worker.RunOnceAsync(stop.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
