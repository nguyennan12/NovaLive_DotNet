using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NovaLive.Api.Extensions;
namespace NovaLive.Api.Tests.Auth;

public sealed class AuthenticationConfigurationTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("too-short")]
    public void MissingOrShortSecret_FailsImmediately(string? secret)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Jwt:Secret"] = secret }).Build();
        Action configure = () => new ServiceCollection().AddNovaLiveAuthentication(config);
        configure.Should().Throw<InvalidOperationException>();
    }
    [Fact]
    public void Bearer_ValidatesAllSecurityProperties()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddNovaLiveAuthentication(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        { ["Jwt:Secret"] = new string('s', 32), ["Jwt:Issuer"] = "issuer", ["Jwt:Audience"] = "audience" }).Build());
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get("Bearer");
        options.MapInboundClaims.Should().BeFalse();
        options.TokenValidationParameters.ValidateIssuer.Should().BeTrue();
        options.TokenValidationParameters.ValidateAudience.Should().BeTrue();
        options.TokenValidationParameters.ValidateLifetime.Should().BeTrue();
        options.TokenValidationParameters.ValidateIssuerSigningKey.Should().BeTrue();
        options.TokenValidationParameters.ClockSkew.Should().Be(TimeSpan.Zero);
        options.TokenValidationParameters.RoleClaimType.Should().Be("role");
        options.TokenValidationParameters.NameClaimType.Should().Be("sub");
        options.TokenValidationParameters.ValidAlgorithms.Should().ContainSingle().Which.Should().Be(SecurityAlgorithms.HmacSha256);
    }
}

