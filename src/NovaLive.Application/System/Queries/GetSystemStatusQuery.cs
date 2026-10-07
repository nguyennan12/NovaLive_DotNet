using MediatR;

namespace NovaLive.Application.System.Queries;

public sealed record GetSystemStatusQuery : IRequest<SystemStatusDto>;

public sealed record SystemStatusDto(string Service, string Status, DateTimeOffset UtcNow);
