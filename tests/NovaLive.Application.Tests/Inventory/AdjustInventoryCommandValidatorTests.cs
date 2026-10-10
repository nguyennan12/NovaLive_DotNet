using FluentAssertions;
using NovaLive.Application.UseCases.Inventory.Commands.AdjustInventory;
using NovaLive.Contracts.V1.Inventory;

namespace NovaLive.Application.Tests.Inventory;

public sealed class AdjustInventoryCommandValidatorTests
{
    private readonly AdjustInventoryCommandValidator validator = new();

    [Fact]
    public void Validate_WhenCommandIsValid_ShouldPass()
    {
        var command = new AdjustInventoryCommand(new AdjustInventoryRequest(
            SkuId: Guid.NewGuid(),
            QtyChange: 10,
            ChangeType: "Import",
            Note: "Nhập thêm hàng từ xưởng"));

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenSkuIdIsEmpty_ShouldFail()
    {
        var command = new AdjustInventoryCommand(new AdjustInventoryRequest(
            SkuId: Guid.Empty,
            QtyChange: 10,
            ChangeType: "Import",
            Note: null));

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.SkuId");
    }

    [Fact]
    public void Validate_WhenQtyChangeIsZero_ShouldFail()
    {
        var command = new AdjustInventoryCommand(new AdjustInventoryRequest(
            SkuId: Guid.NewGuid(),
            QtyChange: 0,
            ChangeType: "ManualAdjust",
            Note: null));

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.QtyChange");
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("ReserveAdd")]
    [InlineData("SaleConfirmed")]
    public void Validate_WhenChangeTypeIsNotAllowedForSeller_ShouldFail(string changeType)
    {
        var command = new AdjustInventoryCommand(new AdjustInventoryRequest(
            SkuId: Guid.NewGuid(),
            QtyChange: -5,
            ChangeType: changeType,
            Note: null));

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.ChangeType");
    }

    [Theory]
    [InlineData("Import")]
    [InlineData("ManualAdjust")]
    [InlineData("import")]
    [InlineData("manualadjust")]
    public void Validate_WhenChangeTypeIsAllowedForSeller_ShouldPass(string changeType)
    {
        var command = new AdjustInventoryCommand(new AdjustInventoryRequest(
            SkuId: Guid.NewGuid(),
            QtyChange: -5,
            ChangeType: changeType,
            Note: "Kiểm kê kho"));

        var result = validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
