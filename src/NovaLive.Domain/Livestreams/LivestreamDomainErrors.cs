using NovaLive.Domain.Common;

namespace NovaLive.Domain.Livestreams;

public static class LivestreamDomainErrors
{
    public static readonly Error InvalidState =
        Error.Conflict("Livestream.InvalidState", "Không thể bắt đầu phiên live đã kết thúc hoặc bị hủy.");

    public static readonly Error NotLive =
        Error.Conflict("Livestream.NotLive", "Phiên livestream hiện không ở trạng thái phát trực tiếp.");

    public static readonly Error AlreadyEnded =
        Error.Conflict("Livestream.AlreadyEnded", "Phiên livestream đã kết thúc.");
}
