namespace NovaLive.Domain.Pricing;

internal static class ProrationEngine
{
    public sealed record AllocationCandidate(
        Guid Id,
        decimal Weight,
        decimal Cap);

    public static Dictionary<Guid, decimal> Allocate(
        decimal totalBudget,
        IReadOnlyList<AllocationCandidate> candidates)
    {
        var result = candidates.ToDictionary(c => c.Id, _ => 0m);

        if (totalBudget <= 0 || candidates.Count == 0)
            return result;

        decimal totalCap = candidates.Sum(c => c.Cap);
        decimal effectiveBudget = Math.Min(totalBudget, totalCap);
        decimal totalWeight = candidates.Sum(c => c.Weight);

        if (totalWeight <= 0 || effectiveBudget <= 0)
            return result;

        // Phase 1: Base allocation (Floor of exact share, capped)
        var remainders = new List<(Guid Id, decimal Remainder, decimal Cap)>();
        decimal totalAllocated = 0m;

        foreach (var c in candidates)
        {
            if (c.Weight <= 0 || c.Cap <= 0)
            {
                remainders.Add((c.Id, 0m, c.Cap));
                continue;
            }

            decimal exactShare = effectiveBudget * (c.Weight / totalWeight);
            decimal baseAllocated = Math.Min(Math.Floor(exactShare), c.Cap);
            decimal remainder = exactShare - Math.Floor(exactShare);

            result[c.Id] = baseAllocated;
            totalAllocated += baseAllocated;
            remainders.Add((c.Id, remainder, c.Cap));
        }

        // Phase 2: Remainder distribution (+1 VND) with Cap check
        decimal delta = effectiveBudget - totalAllocated;

        if (delta > 0)
        {
            var sortedCandidates = remainders
                .OrderByDescending(r => r.Remainder)
                .ThenBy(r => r.Id)
                .ToList();

            bool allocatedAny;
            do
            {
                allocatedAny = false;
                foreach (var candidate in sortedCandidates)
                {
                    if (delta <= 0)
                        break;

                    if (result[candidate.Id] < candidate.Cap)
                    {
                        result[candidate.Id] += 1m;
                        delta -= 1m;
                        allocatedAny = true;
                    }
                }
            } while (delta > 0 && allocatedAny);
        }

        return result;
    }
}
