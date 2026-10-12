namespace NovaLive.Contracts.Common;

public record PagedRequest(
    int Page = 1,
    int Size = 20)
{
    public int NormalizedPage => Page <= 0 ? 1 : Page;

    public int NormalizedSize => Size <= 0 ? 20 : (Size > 100 ? 100 : Size);

    public int Skip => (NormalizedPage - 1) * NormalizedSize;

    public int Take => NormalizedSize;
}
