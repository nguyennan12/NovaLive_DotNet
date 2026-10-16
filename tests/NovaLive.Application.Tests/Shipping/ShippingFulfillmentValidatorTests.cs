using FluentAssertions;
using NovaLive.Application.UseCases.Shipping.Commands.CreateShipmentOrder;
using NovaLive.Application.UseCases.Shipping.Queries.GetOrderTracking;
using NovaLive.Application.UseCases.Shipping.Queries.GetShippingLabel;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.Tests.Shipping;

public sealed class ShippingFulfillmentValidatorTests
{
    [Theory]
    [InlineData("", "2")]
    [InlineData("INVALID", "2")]
    [InlineData("0", "2")]
    [InlineData("GHN", "")]
    [InlineData("GHN", "express")]
    [InlineData("GHN", "-1")]
    public void Create_RejectsInvalidProviderOrService(string provider, string service) =>
        new CreateShipmentOrderCommandValidator().Validate(new CreateShipmentOrderCommand(new(Guid.NewGuid(), provider, service, Guid.NewGuid())))
            .IsValid.Should().BeFalse();

    [Fact]
    public void AllUseCases_RequireNonEmptyGuids()
    {
        new CreateShipmentOrderCommandValidator().Validate(new CreateShipmentOrderCommand(new(Guid.Empty, "GHN", "2", Guid.Empty)))
            .Errors.Should().HaveCount(2);
        new CreateShipmentOrderCommandValidator().Validate(new CreateShipmentOrderCommand(null!)).IsValid.Should().BeFalse();
        new GetShippingLabelQueryValidator().Validate(new GetShippingLabelQuery(Guid.Empty)).IsValid.Should().BeFalse();
        new GetOrderTrackingQueryValidator().Validate(new GetOrderTrackingQuery(Guid.Empty)).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("GHN")]
    [InlineData("GHTK")]
    [InlineData("ViettelPost")]
    public void Create_AcceptsSupportedProvider(string provider) =>
        new CreateShipmentOrderCommandValidator().Validate(new CreateShipmentOrderCommand(new(Guid.NewGuid(), provider, "2", Guid.NewGuid())))
            .IsValid.Should().BeTrue();

    [Fact]
    public void UseCases_DeclareSeededPermissions()
    {
        new CreateShipmentOrderCommand(new(Guid.NewGuid(), "GHN", "2", Guid.NewGuid())).RequiredPermission
            .Should().Be(PermissionCodes.Orders.FulfillmentOwn);
        new GetShippingLabelQuery(Guid.NewGuid()).RequiredPermission.Should().Be(PermissionCodes.Shipping.PrintOwnLabel);
        new GetOrderTrackingQuery(Guid.NewGuid()).RequiredPermission.Should().Be(PermissionCodes.Shipping.TrackOrder);
    }
}
