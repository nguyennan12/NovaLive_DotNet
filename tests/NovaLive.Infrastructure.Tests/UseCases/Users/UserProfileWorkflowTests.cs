using FluentAssertions;
using NovaLive.Application.UseCases.Users.Common;
using NovaLive.Application.UseCases.Users.Profile.Commands.UpdateUserProfile;
using NovaLive.Application.UseCases.Users.Profile.Queries.GetUserProfile;
using NovaLive.Contracts.V1.Users;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.UseCases.Users;

public sealed class UserProfileWorkflowTests
{
    [Fact]
    public async Task GetUserProfile_WhenActive_ReturnsProfile()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var userId = await host.AddUser(email: "profile@example.com", active: true, phone: "0901234567");
        host.Authenticate(userId);

        var result = await host.Send(new GetUserProfileQuery());

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(userId);
        result.Value.Email.Should().Be("profile@example.com");
        result.Value.Phone.Should().Be("0901234567");
        result.Value.FullName.Should().Be("Test User");
    }

    [Fact]
    public async Task UpdateUserProfile_ValidData_UpdatesUserAndReturnsProfile()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var userId = await host.AddUser(email: "update@example.com", active: true, phone: "0901234567");
        host.Authenticate(userId);

        var updateRequest = new UpdateProfileRequest(
            FullName: "Nguyễn Văn A",
            Phone: "0988888888",
            Birthday: new DateOnly(1995, 5, 20),
            Gender: "Male",
            AvatarUrl: "https://example.com/avatar.png");

        var result = await host.Send(new UpdateUserProfileCommand(updateRequest));

        result.IsSuccess.Should().BeTrue();
        result.Value!.FullName.Should().Be("Nguyễn Văn A");
        result.Value.Phone.Should().Be("0988888888");
        result.Value.Birthday.Should().Be(new DateOnly(1995, 5, 20));
        result.Value.Gender.Should().Be("Male");
        result.Value.AvatarUrl.Should().Be("https://example.com/avatar.png");
    }

    [Fact]
    public async Task UpdateUserProfile_DuplicatePhone_ReturnsConflict()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var user1Id = await host.AddUser(email: "user1@example.com", active: true, phone: "0911111111");
        var user2Id = await host.AddUser(email: "user2@example.com", active: true, phone: "0922222222");
        host.Authenticate(user2Id);

        var updateRequest = new UpdateProfileRequest(
            FullName: "User 2 Updated",
            Phone: "0911111111", // Same as user1
            Birthday: null,
            Gender: null,
            AvatarUrl: null);

        var result = await host.Send(new UpdateUserProfileCommand(updateRequest));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(UserErrors.PhoneAlreadyInUse);
    }

    [Fact]
    public async Task GetUserProfile_WithoutPermission_ReturnsForbidden()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var userId = await host.AddUser(email: "noperm@example.com", active: true, phone: "0933333333");
        host.Authenticate(userId);
        host.CurrentUser.SetupGet(x => x.Roles).Returns(Array.Empty<string>());

        var result = await host.Send(new GetUserProfileQuery());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Authorization.Forbidden");
    }
}
