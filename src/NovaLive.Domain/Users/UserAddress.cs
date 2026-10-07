using NovaLive.Domain.Common;

namespace NovaLive.Domain.Users;

public sealed class UserAddress : AuditableEntity
{
    public Guid UserId { get; set; }

    public string RecipientName { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public int? ProvinceId { get; set; }

    public string ProvinceName { get; set; } = string.Empty;

    public int? DistrictId { get; set; }

    public string DistrictName { get; set; } = string.Empty;

    public string? WardCode { get; set; }

    public string WardName { get; set; } = string.Empty;

    public string DetailAddress { get; set; } = string.Empty;

    public bool IsDefault { get; set; }
}
