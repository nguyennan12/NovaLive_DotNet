using NovaLive.Domain.Common;

namespace NovaLive.Domain.Users;

public sealed class UserAddress : AuditableEntity
{
    public Guid UserId { get; private set; }

    public string RecipientName { get; private set; } = string.Empty;

    public string Phone { get; private set; } = string.Empty;

    public int? ProvinceId { get; private set; }

    public string ProvinceName { get; private set; } = string.Empty;

    public int? DistrictId { get; private set; }

    public string DistrictName { get; private set; } = string.Empty;

    public string? WardCode { get; private set; }

    public string WardName { get; private set; } = string.Empty;

    public string DetailAddress { get; private set; } = string.Empty;

    public bool IsDefault { get; private set; }
}
