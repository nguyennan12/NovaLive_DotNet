using FluentAssertions;
using NovaLive.Application.UseCases.Auth.Queries.GetCurrentUser;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.UseCases.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserWorkflowTests
{
    [Fact]
    public async Task ActiveUser_ReturnsCurrentIdentityAndRolesThroughScannedHandler()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var id = await host.AddUser();
        host.Authenticate(id);
        var result = await host.Send(new GetCurrentUserQuery());
        result.IsSuccess.Should().BeTrue();
        result.Value!.UserId.Should().Be(id);
        result.Value.Email.Should().Be("buyer@example.com");
        result.Value.Roles.Should().BeEquivalentTo("Buyer");
    }
}
