namespace NovaLive.Contracts.Common;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int Size,
    long Total);
