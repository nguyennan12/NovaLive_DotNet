namespace NovaLive.Domain.Common;

public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    AlreadyExists = 3,
    Invalid = 4,
    Unauthorized = 5,
    Forbidden = 6,
    Unexpected = 7,
    TooManyRequests = 8,
    Locked = 9
}

public record Error(string Code, string? Message = null)
{
    public int? RetryAfterSeconds { get; init; }
    public ErrorType Type { get; init; } = ErrorType.Unexpected;

    public Error(ErrorType type, string code, string? message = null)
        : this(code, message)
    {
        Type = type;
    }

    public string Description => Message ?? string.Empty;

    public static readonly Error None = new(ErrorType.None, string.Empty);
    public static readonly Error NullValue = new(ErrorType.Unexpected, "Error.NullValue", "The specified result value is null.");

    public static Error Failure(string code, string? message = null) => new(ErrorType.Unexpected, code, message);
    public static Error Validation(string code, string? message = null) => new(ErrorType.Validation, code, message);
    public static Error NotFound(string code, string? message = null) => new(ErrorType.NotFound, code, message);
    public static Error AlreadyExists(string code, string? message = null) => new(ErrorType.AlreadyExists, code, message);
    public static Error Conflict(string code, string? message = null) => new(ErrorType.AlreadyExists, code, message);
    public static Error Unauthorized(string code, string? message = null) => new(ErrorType.Unauthorized, code, message);
    public static Error Forbidden(string code, string? message = null) => new(ErrorType.Forbidden, code, message);

    public static implicit operator Result(Error error) => Result.Failure(error);
}
