using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.System;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class AuditLogRepository(AppDbContext db) : IAuditLogRepository
{
    public async Task AddAsync(AuditLog log, CancellationToken ct) => await db.AuditLogs.AddAsync(log, ct);
}
