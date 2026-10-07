namespace NovaLive.Contracts.Common;

public sealed record ApiResponse<T>(T Data, string? Message = null)
{
    public static ApiResponse<T> Ok(T data, string? message = null) => new(data, message);
}
