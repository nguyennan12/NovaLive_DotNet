using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Domain.Shops;
namespace NovaLive.Application.UseCases.Shops;

public sealed class ShopAddressDefaults(IAppDbContext db)
{
    public async Task SelectAsync(List<ShopAddress> addresses, ShopAddress selected,
        bool primary, bool returns, CancellationToken ct)
    {
        primary |= !addresses.Any(a => a.IsPrimary);
        // If no return warehouse was selected, use the primary warehouse as the fallback.
        var returnTarget = returns ? selected : addresses.FirstOrDefault(a => a.IsReturn)
            ?? (primary ? selected : addresses.First(a => a.IsPrimary));
        foreach (var address in addresses)
        {
            if (primary) address.SetPrimary(false);
            address.SetReturn(false);
        }
        // Flush removals before promotions; unique partial indexes are checked per SQL statement.
        // The owner lock and ITransactionalCommand protect both saves as one atomic operation.
        await db.SaveChangesAsync(ct);
        if (primary) selected.SetPrimary(true);
        returnTarget.SetReturn(true);
    }
}
