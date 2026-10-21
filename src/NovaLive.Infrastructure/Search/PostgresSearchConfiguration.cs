namespace NovaLive.Infrastructure.Search;

public static class PostgresSearchConfiguration
{
    // Created as COPY = simple by AddProductSearchVectorAndTrigramIndex; no unaccent.
    public const string TextSearchConfig = "vietnamese";
}
