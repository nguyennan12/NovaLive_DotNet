using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Domain.Users;

namespace NovaLive.Infrastructure.Persistence.Repositories;

public sealed class UserAddressRepository(AppDbContext dbContext) : IUserAddressRepository
{
    public Task<UserAddress?> GetByIdForUserAsync(Guid id, Guid userId, CancellationToken ct = default) =>
        dbContext.UserAddresses.AsNoTracking().SingleOrDefaultAsync(a => a.Id == id && a.UserId == userId, ct);
}
