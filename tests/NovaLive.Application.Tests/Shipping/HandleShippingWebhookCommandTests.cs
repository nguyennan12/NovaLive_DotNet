using FluentAssertions;
using NovaLive.Application.UseCases.Shipping.Commands.Webhook;
using NovaLive.Domain.Common;
using Xunit;

namespace NovaLive.Application.Tests.Shipping;

public sealed class HandleShippingWebhookCommandTests
{
    [Fact]
    public void Validator_AcceptsValidEvent() => new HandleShippingWebhookCommandValidator()
        .Validate(new HandleShippingWebhookCommand(ShippingProvider.GHN, "TRACK-1", ShippingStatus.Delivered)).IsValid.Should().BeTrue();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Validator_RejectsMissingTrackingCode(string? code) => new HandleShippingWebhookCommandValidator()
        .Validate(new HandleShippingWebhookCommand(ShippingProvider.GHN, code!, ShippingStatus.Delivered)).IsValid.Should().BeFalse();

    [Fact]
    public void Validator_RejectsLongTrackingAndUnknownEnums()
    {
        var result = new HandleShippingWebhookCommandValidator().Validate(new HandleShippingWebhookCommand(
            (ShippingProvider)99, new string('x', 101), (ShippingStatus)99));
        result.Errors.Select(e => e.PropertyName).Should().BeEquivalentTo("Provider", "TrackingCode", "Status");
    }

    [Fact]
    public void Key_IncludesProviderTrackingStatusAndUtcEventTime()
    {
        var at = DateTimeOffset.Parse("2026-10-17T01:00:00Z");
        var c = new HandleShippingWebhookCommand(ShippingProvider.GHN, "TRACK-1", ShippingStatus.Delivered, "{}", at);
        c.IdempotencyKey.Should().Be((c with { RawPayload = "different", EventTimestamp = at.ToOffset(TimeSpan.FromHours(7)) }).IdempotencyKey);
        foreach (var other in new[] { c with { Provider = ShippingProvider.GHTK }, c with { TrackingCode = "TRACK-2" },
            c with { Status = ShippingStatus.Picking }, c with { EventTimestamp = at.AddSeconds(1) }, c with { EventTimestamp = null } })
            other.IdempotencyKey.Should().NotBe(c.IdempotencyKey);
    }
}
