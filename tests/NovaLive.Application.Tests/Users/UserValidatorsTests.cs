using FluentAssertions;
using NovaLive.Application.UseCases.Users.Addresses.Commands.CreateUserAddress;
using NovaLive.Application.UseCases.Users.Addresses.Commands.UpdateUserAddress;
using NovaLive.Application.UseCases.Users.Profile.Commands.UpdateUserProfile;
using NovaLive.Contracts.V1.Users;

namespace NovaLive.Application.Tests.Users;

public sealed class UserValidatorsTests
{
    [Fact]
    public void UpdateUserProfileCommandValidator_ValidData_Passes()
    {
        var validator = new UpdateUserProfileCommandValidator();
        var command = new UpdateUserProfileCommand(new UpdateProfileRequest(
            FullName: "Nguyễn Văn A",
            Phone: "0901234567",
            Birthday: new DateOnly(1995, 1, 1),
            Gender: "Male",
            AvatarUrl: "https://example.com/avatar.jpg"));

        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateUserProfileCommandValidator_EmptyFullName_Fails()
    {
        var validator = new UpdateUserProfileCommandValidator();
        var command = new UpdateUserProfileCommand(new UpdateProfileRequest(
            FullName: "",
            Phone: "0901234567",
            Birthday: null,
            Gender: null,
            AvatarUrl: null));

        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.EndsWith("FullName"));
    }

    [Fact]
    public void UpdateUserProfileCommandValidator_FutureBirthday_Fails()
    {
        var validator = new UpdateUserProfileCommandValidator();
        var command = new UpdateUserProfileCommand(new UpdateProfileRequest(
            FullName: "Valid Name",
            Phone: null,
            Birthday: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            Gender: null,
            AvatarUrl: null));

        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.EndsWith("Birthday"));
    }

    [Fact]
    public void CreateUserAddressCommandValidator_ValidData_Passes()
    {
        var validator = new CreateUserAddressCommandValidator();
        var command = new CreateUserAddressCommand(new CreateAddressRequest(
            RecipientName: "Trần B",
            Phone: "0987654321",
            ProvinceId: 1,
            ProvinceName: "Hà Nội",
            DistrictId: 2,
            DistrictName: "Cầu Giấy",
            WardCode: "W01",
            WardName: "Dịch Vọng",
            DetailAddress: "Số 10 Phạm Hùng",
            IsDefault: true));

        var result = validator.Validate(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateUserAddressCommandValidator_MissingRequiredFields_Fails()
    {
        var validator = new CreateUserAddressCommandValidator();
        var command = new CreateUserAddressCommand(new CreateAddressRequest(
            RecipientName: "",
            Phone: "",
            ProvinceId: null,
            ProvinceName: "",
            DistrictId: null,
            DistrictName: "",
            WardCode: null,
            WardName: "",
            DetailAddress: "",
            IsDefault: false));

        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCountGreaterOrEqualTo(5);
    }

    [Fact]
    public void UpdateUserAddressCommandValidator_EmptyAddressId_Fails()
    {
        var validator = new UpdateUserAddressCommandValidator();
        var command = new UpdateUserAddressCommand(Guid.Empty, new UpdateAddressRequest(
            RecipientName: "Trần B",
            Phone: "0987654321",
            ProvinceId: 1,
            ProvinceName: "Hà Nội",
            DistrictId: 2,
            DistrictName: "Cầu Giấy",
            WardCode: "W01",
            WardName: "Dịch Vọng",
            DetailAddress: "Số 10 Phạm Hùng",
            IsDefault: false));

        var result = validator.Validate(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserAddressCommand.AddressId));
    }

    [Fact]
    public void SetDefaultAddressCommandValidator_ValidAndEmpty_Tested()
    {
        var validator = new NovaLive.Application.UseCases.Users.Addresses.Commands.SetDefaultAddress.SetDefaultAddressCommandValidator();
        validator.Validate(new NovaLive.Application.UseCases.Users.Addresses.Commands.SetDefaultAddress.SetDefaultAddressCommand(Guid.NewGuid())).IsValid.Should().BeTrue();
        validator.Validate(new NovaLive.Application.UseCases.Users.Addresses.Commands.SetDefaultAddress.SetDefaultAddressCommand(Guid.Empty)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void DeleteUserAddressCommandValidator_ValidAndEmpty_Tested()
    {
        var validator = new NovaLive.Application.UseCases.Users.Addresses.Commands.DeleteUserAddress.DeleteUserAddressCommandValidator();
        validator.Validate(new NovaLive.Application.UseCases.Users.Addresses.Commands.DeleteUserAddress.DeleteUserAddressCommand(Guid.NewGuid())).IsValid.Should().BeTrue();
        validator.Validate(new NovaLive.Application.UseCases.Users.Addresses.Commands.DeleteUserAddress.DeleteUserAddressCommand(Guid.Empty)).IsValid.Should().BeFalse();
    }
}
