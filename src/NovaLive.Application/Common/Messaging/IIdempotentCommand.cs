namespace NovaLive.Application.Common.Messaging;

public interface IIdempotentCommand
{
    string IdempotencyKey { get; }
}
