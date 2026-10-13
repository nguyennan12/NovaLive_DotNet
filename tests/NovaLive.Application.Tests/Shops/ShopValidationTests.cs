using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NovaLive.Application.UseCases.Shops;
using NovaLive.Application.UseCases.Shops.Commands.RegisterShop;
using NovaLive.Application.UseCases.Shops.Commands.AddShopAddress;
using NovaLive.Application.UseCases.Shops.Commands.UpdateShopInfo;
using NovaLive.Application.Common.Messaging;
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
        ShopWorkflowTests.Registration with { KycFrontUrl = url })).IsValid.Should().BeFalse();

    [Theory]
    [InlineData("https://cdn.example.com/kyc-docs/front.png?X-Amz-Signature=original")]
    [InlineData("http://localhost:9000/kyc-docs/owner/front.png")]
    [InlineData("https://cdn.example.com/kyc-docs/front%20photo.png")]
    public void Kyc_AcceptsConfiguredBucketAndPresignedQuery(string url) => Validator.Validate(new RegisterShopCommand(
        ShopWorkflowTests.Registration with { KycFrontUrl = url })).IsValid.Should().BeTrue();

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

    [Fact]
    public void EveryShopUseCase_HasValidator_AndCommandsAreTransactional()
    {
        var services = new ServiceCollection().AddApplication();
        var requests = typeof(RegisterShopCommand).Assembly.GetTypes().Where(t =>
            t.Namespace?.StartsWith("NovaLive.Application.UseCases.Shops.") == true && !t.IsAbstract &&
            t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(MediatR.IRequest<>))).ToList();
        requests.Should().HaveCount(10);
        foreach (var request in requests)
        {
            services.Should().Contain(s => s.ServiceType == typeof(IValidator<>).MakeGenericType(request));
            if (typeof(IBaseCommand).IsAssignableFrom(request)) typeof(ITransactionalCommand).IsAssignableFrom(request).Should().BeTrue();
        }
    }
}
