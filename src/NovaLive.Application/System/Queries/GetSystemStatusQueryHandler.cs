using MediatR;
using NovaLive.Application.Abstractions.Services;

namespace NovaLive.Application.System.Queries;

public sealed class GetSystemStatusQueryHandler(IDateTimeProvider dateTimeProvider)
    : IRequestHandler<GetSystemStatusQuery, SystemStatusDto>
{
    public Task<SystemStatusDto> Handle(GetSystemStatusQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new SystemStatusDto("NovaLive.CoreApi", "Healthy", dateTimeProvider.UtcNow));
    }
}
