using FluentAssertions;
using NovaLive.Application.UseCases.Shipping.Queries.CalculateFee;
using NovaLive.Contracts.V1.Shipping;
using Xunit;

namespace NovaLive.Application.Tests.Shipping;

public class CalculateShippingFeeQueryValidatorTests
{
    private readonly CalculateShippingFeeQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenValidRequest_ShouldNotHaveErrors()
    {
        var request = new CalculateShippingFeeRequest(
            ShopId: Guid.NewGuid(),
            WarehouseAddressId: Guid.NewGuid(),
            ShippingAddressId: Guid.NewGuid(),
            WeightGram: 500);

        var query = new CalculateShippingFeeQuery(request);
        var result = _validator.Validate(query);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenEmptyShopId_ShouldHaveError()
    {
        var request = new CalculateShippingFeeRequest(
            ShopId: Guid.Empty,
            WarehouseAddressId: Guid.NewGuid(),
            ShippingAddressId: Guid.NewGuid(),
            WeightGram: 500);

        var query = new CalculateShippingFeeQuery(request);
        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("ShopId"));
    }

    [Fact]
    public void Validate_WhenNegativeOrZeroWeight_ShouldHaveError()
    {
        var request = new CalculateShippingFeeRequest(
            ShopId: Guid.NewGuid(),
            WarehouseAddressId: Guid.NewGuid(),
            ShippingAddressId: Guid.NewGuid(),
            WeightGram: 0);

        var query = new CalculateShippingFeeQuery(request);
        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("WeightGram"));
    }

    [Fact]
    public void Validate_WhenWeightExceeds50Kg_ShouldHaveError()
    {
        var request = new CalculateShippingFeeRequest(
            ShopId: Guid.NewGuid(),
            WarehouseAddressId: Guid.NewGuid(),
            ShippingAddressId: Guid.NewGuid(),
            WeightGram: 60_000);

        var query = new CalculateShippingFeeQuery(request);
        var result = _validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("WeightGram"));
    }
}
