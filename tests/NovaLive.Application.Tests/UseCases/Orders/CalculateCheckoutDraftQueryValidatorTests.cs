using FluentAssertions;
using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.UseCases.Orders.Queries.CalculateCheckoutDraft;
using NovaLive.Contracts.V1.Orders;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.Tests.UseCases.Orders;

public sealed class CalculateCheckoutDraftQueryValidatorTests
{
    public static IEnumerable<object[]> InvalidRequests()
    {
        var id = Guid.NewGuid();
        var valid = new CalculateCheckoutRequest([id], Guid.NewGuid(), null, null, null);
        yield return [valid with { CartItemIds = [] }];
        yield return [valid with { CartItemIds = null! }];
        yield return [valid with { CartItemIds = [id, id] }];
        yield return [valid with { CartItemIds = [Guid.Empty] }];
        yield return [valid with { CartItemIds = Enumerable.Range(0, 101).Select(_ => Guid.NewGuid()).ToList() }];
        yield return [valid with { ShippingAddressId = Guid.Empty }];
        foreach (var code in new[] { "", "   ", new string('x', 51) })
        {
            yield return [valid with { PlatformProductVoucherCode = code }];
            yield return [valid with { PlatformFreeshipVoucherCode = code }];
            yield return [valid with { ShopVouchers = [new(id, code)] }];
        }
        yield return [valid with { ShopVouchers = [new(id, "CODE"), new(id, "code")] }];
        yield return [valid with { ShopVouchers = [new(id, "FIRST"), new(id, "SECOND")] }];
        yield return [valid with { ShopVouchers = [new(Guid.Empty, "CODE")] }];
        yield return [valid with { ShopVouchers = [new(id, null!)] }];
        yield return [valid with { ShopVouchers = [null!] }];
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public void InvalidInput_IsRejected(CalculateCheckoutRequest request) =>
        new CalculateCheckoutDraftQueryValidator().Validate(new CalculateCheckoutDraftQuery(request)).IsValid.Should().BeFalse();

    [Fact]
    public void NullRequest_IsRejected() =>
        new CalculateCheckoutDraftQueryValidator().Validate(new CalculateCheckoutDraftQuery(null!)).IsValid.Should().BeFalse();

    [Fact]
    public void ValidInputAndPermission_AreAccepted()
    {
        var query = new CalculateCheckoutDraftQuery(new([Guid.NewGuid()], Guid.NewGuid(),
            [new(Guid.NewGuid(), "SHOP")], "PLATFORM", "SHIP"));
        new CalculateCheckoutDraftQueryValidator().Validate(query).IsValid.Should().BeTrue();
        query.RequiredPermission.Should().Be(PermissionCodes.Orders.Checkout);
        query.Should().NotBeAssignableTo<ITransactionalCommand>();
        query.Should().NotBeAssignableTo<IIdempotentCommand>();
        typeof(CalculateCheckoutDraftQuery).GetCustomAttributes(typeof(RequireAuthenticatedAttribute), true).Should().ContainSingle();
    }
}
