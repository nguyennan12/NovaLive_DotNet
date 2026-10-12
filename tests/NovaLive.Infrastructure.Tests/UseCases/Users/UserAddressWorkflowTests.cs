using FluentAssertions;
using NovaLive.Application.UseCases.Users.Addresses.Commands.CreateUserAddress;
using NovaLive.Application.UseCases.Users.Addresses.Commands.DeleteUserAddress;
using NovaLive.Application.UseCases.Users.Addresses.Commands.SetDefaultAddress;
using NovaLive.Application.UseCases.Users.Addresses.Commands.UpdateUserAddress;
using NovaLive.Application.UseCases.Users.Addresses.Queries.GetUserAddresses;
using NovaLive.Application.UseCases.Users.Common;
using NovaLive.Contracts.V1.Users;
using NovaLive.Domain.Common.Exceptions;
using NovaLive.Infrastructure.Tests.Auth;

namespace NovaLive.Infrastructure.Tests.UseCases.Users;

public sealed class UserAddressWorkflowTests
{
    private static CreateAddressRequest BuildRequest(string recipient = "Người Nhận", bool isDefault = false) =>
        new(
            RecipientName: recipient,
            Phone: "0901234567",
            ProvinceId: 1,
            ProvinceName: "Hà Nội",
            DistrictId: 2,
            DistrictName: "Cầu Giấy",
            WardCode: "W001",
            WardName: "Dịch Vọng",
            DetailAddress: "Số 1 Duy Tân",
            IsDefault: isDefault);

    [Fact]
    public async Task CreateFirstAddress_AutomaticallySetAsDefault_EvenWhenRequestedFalse()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var userId = await host.AddUser();
        host.Authenticate(userId);

        var req = BuildRequest(recipient: "Địa chỉ 1", isDefault: false);
        var result = await host.Send(new CreateUserAddressCommand(req));

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsDefault.Should().BeTrue();
        result.Value.RecipientName.Should().Be("Địa chỉ 1");
    }

    [Fact]
    public async Task CreateSecondAddress_WithIsDefaultTrue_UnsetsPreviousDefault()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var userId = await host.AddUser();
        host.Authenticate(userId);

        var addr1 = (await host.Send(new CreateUserAddressCommand(BuildRequest("Địa chỉ 1", false)))).Value!;
        var addr2 = (await host.Send(new CreateUserAddressCommand(BuildRequest("Địa chỉ 2", true)))).Value!;

        addr1.IsDefault.Should().BeTrue(); // Was default when created
        addr2.IsDefault.Should().BeTrue();

        var list = (await host.Send(new GetUserAddressesQuery())).Value!;
        list.Should().HaveCount(2);

        var firstInList = list[0];
        firstInList.Id.Should().Be(addr2.Id);
        firstInList.IsDefault.Should().BeTrue();

        var secondInList = list[1];
        secondInList.Id.Should().Be(addr1.Id);
        secondInList.IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task SetDefaultAddress_SetsTargetAsDefaultAndUnsetsOthers()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var userId = await host.AddUser();
        host.Authenticate(userId);

        var addr1 = (await host.Send(new CreateUserAddressCommand(BuildRequest("Địa chỉ 1", false)))).Value!;
        var addr2 = (await host.Send(new CreateUserAddressCommand(BuildRequest("Địa chỉ 2", false)))).Value!;

        // Addr1 is default, Addr2 is not default
        var setDefaultResult = await host.Send(new SetDefaultAddressCommand(addr2.Id));
        setDefaultResult.IsSuccess.Should().BeTrue();
        setDefaultResult.Value!.IsDefault.Should().BeTrue();

        var list = (await host.Send(new GetUserAddressesQuery())).Value!;
        list.First(a => a.Id == addr2.Id).IsDefault.Should().BeTrue();
        list.First(a => a.Id == addr1.Id).IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAddress_WhenSetDefaultTrue_UnsetsOtherDefaults()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var userId = await host.AddUser();
        host.Authenticate(userId);

        var addr1 = (await host.Send(new CreateUserAddressCommand(BuildRequest("Địa chỉ 1", false)))).Value!;
        var addr2 = (await host.Send(new CreateUserAddressCommand(BuildRequest("Địa chỉ 2", false)))).Value!;

        var updateReq = new UpdateAddressRequest(
            RecipientName: "Địa chỉ 2 Đã Sửa",
            Phone: "0999999999",
            ProvinceId: 1,
            ProvinceName: "Hà Nội",
            DistrictId: 2,
            DistrictName: "Cầu Giấy",
            WardCode: "W001",
            WardName: "Dịch Vọng Hậu",
            DetailAddress: "Số 99 Trần Thái Tông",
            IsDefault: true);

        var updateResult = await host.Send(new UpdateUserAddressCommand(addr2.Id, updateReq));
        updateResult.IsSuccess.Should().BeTrue();
        updateResult.Value!.IsDefault.Should().BeTrue();
        updateResult.Value.RecipientName.Should().Be("Địa chỉ 2 Đã Sửa");

        var list = (await host.Send(new GetUserAddressesQuery())).Value!;
        list.First(a => a.Id == addr2.Id).IsDefault.Should().BeTrue();
        list.First(a => a.Id == addr1.Id).IsDefault.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAddress_WhenDefaultAndOtherAddressesExist_ReturnsCannotDeleteDefaultAddressError()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var userId = await host.AddUser();
        host.Authenticate(userId);

        var addr1 = (await host.Send(new CreateUserAddressCommand(BuildRequest("Địa chỉ 1 (Default)", false)))).Value!;
        var addr2 = (await host.Send(new CreateUserAddressCommand(BuildRequest("Địa chỉ 2", false)))).Value!;

        addr1.IsDefault.Should().BeTrue();

        // Deleting addr1 (default) when addr2 exists must return CannotDeleteDefaultAddress error
        var deleteResult = await host.Send(new DeleteUserAddressCommand(addr1.Id));
        deleteResult.IsFailure.Should().BeTrue();
        deleteResult.Error.Should().Be(UserAddressErrors.CannotDeleteDefaultAddress);
        deleteResult.Error.Message.Should().Be("Không thể xóa địa chỉ mặc định khi còn địa chỉ khác trong sổ địa chỉ");
    }

    [Fact]
    public async Task DeleteAddress_WhenDefaultAndOnlyAddress_DeletesSuccessfully()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var userId = await host.AddUser();
        host.Authenticate(userId);

        var addr1 = (await host.Send(new CreateUserAddressCommand(BuildRequest("Địa chỉ duy nhất", false)))).Value!;
        addr1.IsDefault.Should().BeTrue();

        var deleteResult = await host.Send(new DeleteUserAddressCommand(addr1.Id));
        deleteResult.IsSuccess.Should().BeTrue();

        var list = (await host.Send(new GetUserAddressesQuery())).Value!;
        list.Should().BeEmpty();
    }

    [Fact]
    public async Task AddressOperations_ForDifferentUser_ReturnsNotFound()
    {
        await using var host = new AuthTestHost();
        await host.InitializeAsync();
        var user1Id = await host.AddUser("user1@example.com", phone: "0911111111");
        var user2Id = await host.AddUser("user2@example.com", phone: "0922222222");

        host.Authenticate(user1Id);
        var addr1 = (await host.Send(new CreateUserAddressCommand(BuildRequest("Địa chỉ của User 1")))).Value!;

        // Now switch to user2
        host.Authenticate(user2Id);

        var updateReq = new UpdateAddressRequest("Hack", "0900000000", 1, "HN", 1, "CG", "W", "P", "D", false);
        var updateResult = await host.Send(new UpdateUserAddressCommand(addr1.Id, updateReq));
        updateResult.IsFailure.Should().BeTrue();
        updateResult.Error.Should().Be(UserAddressErrors.NotFound);

        var setDefaultResult = await host.Send(new SetDefaultAddressCommand(addr1.Id));
        setDefaultResult.IsFailure.Should().BeTrue();
        setDefaultResult.Error.Should().Be(UserAddressErrors.NotFound);

        var deleteResult = await host.Send(new DeleteUserAddressCommand(addr1.Id));
        deleteResult.IsFailure.Should().BeTrue();
        deleteResult.Error.Should().Be(UserAddressErrors.NotFound);
    }
}
