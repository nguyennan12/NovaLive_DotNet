using NovaLive.Domain.System;

namespace NovaLive.Application.Abstractions.Persistence.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct);
}
