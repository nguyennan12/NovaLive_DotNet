namespace NovaLive.Domain.Common;

public interface IVersioned
{
    long Version { get; set; }
}

public interface IConcurrencyCheck
{
    byte[]? RowVersion { get; set; }
}
