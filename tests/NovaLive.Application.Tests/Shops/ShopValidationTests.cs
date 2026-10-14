using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Application.UseCases.Shops.Commands.RegisterShop;
using NovaLive.Application.UseCases.Shops.Commands.AddShopAddress;
using NovaLive.Application.UseCases.Shops.Commands.UpdateShopInfo;
using NovaLive.Application.Common.Messaging;
using NovaLive.Application.Common.Behaviors;
using NovaLive.Application.UseCases.Shops.Queries.GetMyShop;
using NovaLive.Application.UseCases.Shops.Queries.GetSellerShop;
using NovaLive.Application.UseCases.Shops.Queries.GetShopAddresses;
using NovaLive.Contracts.V1.Shops;
using NovaLive.Domain.Common;
using MediatR;
using Moq;
namespace NovaLive.Application.Tests.Shops;

public sealed class ShopValidationTests
{
    private static RegisterShopCommandValidator Validator => new(Options.Create(new KycUrlOptions
        { AllowedPrefixes = ["https://cdn.example.com/kyc-docs/", "http://localhost:9000/kyc-docs/"] }));

    [Theory]
    [InlineData("https://evil.example/kyc-docs/front.png")]
    [InlineData("https://cdn.example.com.evil.example/kyc-docs/front.png")]
    [InlineData("https://cdn.example.com/products/front.png")]
    [InlineData("https://cdn.example.com/kyc-docs-evil/front.png")]
    [InlineData("https://cdn.example.com:444/kyc-docs/front.png")]
    [InlineData("https://user@cdn.example.com/kyc-docs/front.png")]
    [InlineData("https://cdn.example.com/kyc-docs/../products/front.png")]
    [InlineData("https://cdn.example.com/kyc-docs/%2f../products/front.png")]
    [InlineData("https://cdn.example.com/kyc-docs/front.png#fragment")]
    [InlineData("file:///kyc-docs/front.png")]
    [InlineData("https://cdn.example.com/kyc-docs/")]
    public void Kyc_RejectsUntrustedUrls(string url) => Validator.Validate(new RegisterShopCommand(
        ShopWorkflowTests.Registration with { IdCardFront = url })).IsValid.Should().BeFalse();

    [Theory]
    [InlineData("https://cdn.example.com/kyc-docs/front.png?X-Amz-Signature=original")]
    [InlineData("http://localhost:9000/kyc-docs/owner/front.png")]
    [InlineData("https://cdn.example.com/kyc-docs/front%20photo.png")]
    public void Kyc_AcceptsConfiguredBucketAndPresignedQuery(string url) => Validator.Validate(new RegisterShopCommand(
        ShopWorkflowTests.Registration with { IdCardFront = url })).IsValid.Should().BeTrue();

    [Fact]
    public void Kyc_MissingConfigurationFailsClosed() => new RegisterShopCommandValidator(Options.Create(new KycUrlOptions()))
        .Validate(new RegisterShopCommand(ShopWorkflowTests.Registration)).IsValid.Should().BeFalse();

    [Fact]
    public void Registration_EnforcesLengthsAndNormalizedSlug()
    {
        var invalid = ShopWorkflowTests.Registration with { ShopName = new('a', 201), Slug = "💥", Phone = "1234",
            Email = "bad", TaxCode = new('a', 31), BankAccount = new('a', 31), BankName = new('a', 101),
            BankBranch = new('a', 201), BusinessLicense = "https://evil.example/license.png" };
        var result = Validator.Validate(new RegisterShopCommand(invalid));
        result.Errors.Should().HaveCountGreaterThanOrEqualTo(9);
        Validator.Validate(new RegisterShopCommand(ShopWorkflowTests.Registration with { Slug = "  ĐỒ ĐẸP  " })).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Addresses_RequireShippingCodesAndValidateLengths()
    {
        var result = new AddShopAddressCommandValidator().Validate(new AddShopAddressCommand(ShopWorkflowTests.Address with
        { ProvinceId = null, DistrictId = 0, WardCode = "", WarehouseName = new('a', 151),
          ContactName = new('a', 151), DetailAddress = new('a', 301), ContactPhone = "123" }));
        result.Errors.Should().HaveCount(7);
    }

    [Fact]
    public void UpdateContract_DoesNotExposeImmutableOrKycFields() => typeof(NovaLive.Contracts.V1.Shops.UpdateShopProfileRequest)
        .GetProperties().Select(p => p.Name).Should().BeEquivalentTo("ShopName", "Description", "LogoUrl", "BannerUrl", "Phone", "Email");

    [Theory]
    [InlineData(null, true)]
    [InlineData("https://cdn.example.com/shop-images/logo.png", true)]
    [InlineData("https://evil.example/logo.png", false)]
    [InlineData("https://cdn.example.com.evil.example/logo.png", false)]
    [InlineData("https://cdn.example.com:444/logo.png", false)]
    [InlineData("http://cdn.example.com/logo.png", false)]
    [InlineData("https://user@cdn.example.com/logo.png", false)]
    public void ShopImages_RequireConfiguredCdnOrigin(string? url, bool valid)
    {
        var validator = new UpdateShopInfoCommandValidator(Options.Create(new KycUrlOptions
            { AllowedPrefixes = ["https://cdn.example.com/kyc-docs/"] }));
        var request = new UpdateShopProfileRequest("Shop", "Description", url, url, "0912345678", "shop@example.com");
        validator.Validate(new UpdateShopInfoCommand(request)).IsValid.Should().Be(valid);
    }

    [Fact]
    public void ShopImages_RejectOversizedUrlsAndUnconfiguredCdn()
    {
        var request = new UpdateShopProfileRequest("Shop", "Description", "https://cdn.example.com/" + new string('a', 500),
            null, "0912345678", "shop@example.com");
        var options = new KycUrlOptions { AllowedPrefixes = ["https://cdn.example.com/kyc-docs/"] };
        new UpdateShopInfoCommandValidator(Options.Create(options)).Validate(new UpdateShopInfoCommand(request))
            .IsValid.Should().BeFalse();
        new UpdateShopInfoCommandValidator(Options.Create(new KycUrlOptions())).Validate(new UpdateShopInfoCommand(request with
            { LogoUrl = "https://cdn.example.com/logo.png" })).IsValid.Should().BeFalse();
    }

    [Fact]
    public void InputRequestsHaveNonemptyValidators_ParameterlessQueriesDoNot_CommandsAreTransactional()
    {
        var services = new ServiceCollection().AddApplication();
        var requests = typeof(RegisterShopCommand).Assembly.GetTypes().Where(t =>
            t.Namespace?.StartsWith("NovaLive.Application.UseCases.Shops.") == true && !t.IsAbstract &&
            t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(MediatR.IRequest<>))).ToList();
        requests.Should().HaveCount(11);
        foreach (var request in requests)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(request);
            if (request == typeof(GetMyShopQuery) || request == typeof(GetShopAddressesQuery) || request == typeof(GetSellerShopQuery))
                services.Should().NotContain(s => s.ServiceType == validatorType);
            else
            {
                services.Should().ContainSingle(s => s.ServiceType == validatorType);
                using var provider = services.BuildServiceProvider();
                ((IValidator)provider.GetRequiredService(validatorType)).CreateDescriptor()
                    .GetMembersWithValidators().Should().NotBeEmpty();
            }
            if (typeof(IBaseCommand).IsAssignableFrom(request)) typeof(ITransactionalCommand).IsAssignableFrom(request).Should().BeTrue();
        }
    }
    [Fact]
    public async Task RegisteredValidator_ValidationBehavior_BlocksInvalidRequestBeforeHandler()
    {
        var services = new ServiceCollection().AddApplication();
        services.Should().Contain(s => s.ImplementationType == typeof(ValidationBehavior<,>));
        using var provider = services.BuildServiceProvider();
        var behavior = ActivatorUtilities.CreateInstance<ValidationBehavior<RegisterShopCommand, Result<ShopResponse>>>(provider);
        var next = new Mock<RequestHandlerDelegate<Result<ShopResponse>>>();
        var result = await behavior.Handle(new RegisterShopCommand(ShopWorkflowTests.Registration with
            { ShopName = new string('x', 201), IdCardFront = "https://evil.example/front.png" }), next.Object, default);
        result.IsFailure.Should().BeTrue();
        result.Should().BeAssignableTo<IValidationResult>();
        ((IValidationResult)result).Errors.Should().Contain(error => error.Code == "Request.ShopName");
        next.Verify(handler => handler(It.IsAny<CancellationToken>()), Times.Never);
    }

}
