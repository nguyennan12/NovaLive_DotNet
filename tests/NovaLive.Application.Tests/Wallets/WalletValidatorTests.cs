using FluentAssertions;
using NovaLive.Application.UseCases.Wallets.Commands.ApprovePayout;
using NovaLive.Application.UseCases.Wallets.Commands.CreatePayoutRequest;
using NovaLive.Application.UseCases.Wallets.Commands.RejectPayout;
using NovaLive.Application.UseCases.Wallets.Queries.GetAdminPayouts;
using NovaLive.Application.UseCases.Wallets.Queries.GetWalletTransactions;
using NovaLive.Contracts.V1.Shops;

namespace NovaLive.Application.Tests.Wallets;

public sealed class WalletValidatorTests
{
    private readonly CreatePayoutRequestCommandValidator _validator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(49_999)]
    [InlineData(-10_000)]
    public void Validator_AmountBelow50000_FailsValidation(decimal amount)
    {
        var command = new CreatePayoutRequestCommand(new CreatePayoutRequest(amount));
        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Amount");
    }

    [Theory]
    [InlineData(50_000)]
    [InlineData(100_000)]
    [InlineData(10_000_000)]
    public void Validator_AmountGreaterThanOrEqualTo50000_PassesValidation(decimal amount)
    {
        var command = new CreatePayoutRequestCommand(new CreatePayoutRequest(amount));
        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ApprovePayoutValidator_ValidRequest_Passes()
    {
        var validator = new ApprovePayoutCommandValidator();
        var command = new ApprovePayoutCommand(Guid.NewGuid(), new ApprovePayoutRequest("TXN-123"));
        validator.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void RejectPayoutValidator_EmptyReason_Fails()
    {
        var validator = new RejectPayoutCommandValidator();
        var command = new RejectPayoutCommand(Guid.NewGuid(), new RejectPayoutRequest(""));
        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Reason");
    }

    [Fact]
    public void GetAdminPayoutsValidator_InvalidStatus_Fails()
    {
        var validator = new GetAdminPayoutsQueryValidator();
        var query = new GetAdminPayoutsQuery(new GetAdminPayoutsRequest(Status: "InvalidStatus"));
        var result = validator.Validate(query);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Status");
    }

    [Theory]
    [InlineData("unknown")]
    [InlineData("999")]
    public void GetWalletTransactionsValidator_InvalidType_Fails(string type)
    {
        var validator = new GetWalletTransactionsQueryValidator();
        var query = new GetWalletTransactionsQuery(new GetWalletTransactionsRequest(Type: type));

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.Type");
    }

    [Fact]
    public void GetWalletTransactionsValidator_FromAfterTo_Fails()
    {
        var validator = new GetWalletTransactionsQueryValidator();
        var query = new GetWalletTransactionsQuery(new GetWalletTransactionsRequest(
            From: DateTimeOffset.Parse("2026-10-02T00:00:00Z"),
            To: DateTimeOffset.Parse("2026-10-01T00:00:00Z")));

        var result = validator.Validate(query);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Request.To");
    }
}
