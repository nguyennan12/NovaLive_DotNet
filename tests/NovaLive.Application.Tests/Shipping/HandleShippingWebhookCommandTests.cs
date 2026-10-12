using FluentAssertions;
using NovaLive.Application.UseCases.Shipping.Commands.Webhook;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;
using NovaLive.Domain.Shipping;
using Xunit;

namespace NovaLive.Application.Tests.Shipping;

public class HandleShippingWebhookCommandTests
{
    private readonly HandleShippingWebhookCommandValidator _validator = new();

    [Fact]
    public void Validator_WhenValidRequest_ShouldBeValid()
    {
        var request = new GhnWebhookRequest("GHN123", "delivered");
        var command = new HandleShippingWebhookCommand(request, "{}");

        var result = _validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_WhenMissingOrderCode_ShouldHaveError()
    {
        var request = new GhnWebhookRequest("", "delivering");
        var command = new HandleShippingWebhookCommand(request, "{}");

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("OrderCode"));
    }

    [Fact]
    public void Validator_WhenMissingStatus_ShouldHaveError()
    {
        var request = new GhnWebhookRequest("GHN123", "");
        var command = new HandleShippingWebhookCommand(request, "{}");

        var result = _validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Status"));
    }

    [Fact]
    public void ShippingOrder_UpdateStatusFromWebhook_SetsDeliveredAtWhenDelivered()
    {
        var order = new ShippingOrder(
            subOrderId: Guid.NewGuid(),
            provider: ShippingProvider.GHN,
            serviceCode: "2",
            pickupAddressJson: "{}",
            deliveryAddressJson: "{}",
            codAmount: 0,
            shippingFee: 25000,
            weightGram: 500,
            trackingCode: "GHN_TEST_01");

        order.Status.Should().Be(ShippingStatus.ReadyToPick);

        order.UpdateStatusFromWebhook(ShippingStatus.Delivered, "{\"status\":\"delivered\"}");

        order.Status.Should().Be(ShippingStatus.Delivered);
        order.DeliveredAt.Should().NotBeNull();
        order.WebhookPayload.Should().Be("{\"status\":\"delivered\"}");
    }

    [Fact]
    public void ShippingOrder_UpdateStatusFromWebhook_PreventsStatusRegression()
    {
        var order = new ShippingOrder(
            subOrderId: Guid.NewGuid(),
            provider: ShippingProvider.GHN,
            serviceCode: "2",
            pickupAddressJson: "{}",
            deliveryAddressJson: "{}",
            codAmount: 0,
            shippingFee: 25000,
            weightGram: 500,
            trackingCode: "GHN_TEST_02");

        // Đơn đã giao thành công
        order.UpdateStatusFromWebhook(ShippingStatus.Delivered, "{\"status\":\"delivered\"}");
        order.Status.Should().Be(ShippingStatus.Delivered);

        // Giả lập webhook cũ "delivering" bắn lại trễ do network retry
        order.UpdateStatusFromWebhook(ShippingStatus.Delivering, "{\"status\":\"delivering\"}");

        // Trạng thái vẫn phải giữ nguyên là Delivered, không được thụt lùi
        order.Status.Should().Be(ShippingStatus.Delivered);
    }
}
