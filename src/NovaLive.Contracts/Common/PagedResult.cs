namespace NovaLive.Contracts.Common;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int Size,
    long Total)
{
    public int TotalPages => Size > 0 ? (int)Math.Ceiling(Total / (double)Size) : 0;

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int size, long total) =>
        new(items, page, size, total);
}
