using InsuranceClaims.App.Data.Entities;

namespace InsuranceClaims.App.Analytics;

public static class InsuranceAnalytics
{
    private static bool IsApproved(string status) =>
        string.Equals(status, "Approved", StringComparison.OrdinalIgnoreCase);

    public static List<CoverageBreachRow> FindCoverageBreaches(
        IEnumerable<PolicyHolderEntity> holders)
    {
        ArgumentNullException.ThrowIfNull(holders);

        return holders
            .SelectMany(holder => holder.Policies.Select(policy => new
            {
                holder.PolicyHolderName,
                policy.PolicyNumber,
                policy.CoverageAmount,
                ApprovedPayouts = policy.Claims
                    .Where(claim => IsApproved(claim.Status))
                    .Sum(claim => claim.PayoutAmount)
            }))
            .Where(x => x.ApprovedPayouts > x.CoverageAmount)
            .Select(x => new CoverageBreachRow(
                x.PolicyHolderName,
                x.PolicyNumber,
                x.CoverageAmount,
                x.ApprovedPayouts,
                x.ApprovedPayouts - x.CoverageAmount))
            .OrderByDescending(x => x.ExcessAmount)
            .ToList();
    }

    public static List<InconsistentClaimRow> FindInconsistentClaims(
        IEnumerable<PolicyHolderEntity> holders)
    {
        ArgumentNullException.ThrowIfNull(holders);

        return holders
            .SelectMany(holder => holder.Policies.SelectMany(policy => policy.Claims
                .Where(claim =>
                    (!IsApproved(claim.Status) && claim.PayoutAmount > 0) ||
                    (IsApproved(claim.Status) && claim.PayoutAmount == 0))
                .Select(claim => new InconsistentClaimRow(
                    holder.PolicyHolderName,
                    policy.PolicyNumber,
                    claim.IncidentDate,
                    claim.PayoutAmount,
                    claim.Status,
                    !IsApproved(claim.Status)
                        ? "Є виплата, хоча статус випадку не Approved."
                        : "Випадок має статус Approved, але сума виплати дорівнює нулю."))))
            .OrderByDescending(x => x.PayoutAmount)
            .ToList();
    }

    public static List<StatusStatisticsRow> GetStatusStatistics(
        IEnumerable<PolicyHolderEntity> holders)
    {
        ArgumentNullException.ThrowIfNull(holders);

        return holders
            .SelectMany(holder => holder.Policies)
            .SelectMany(policy => policy.Claims)
            .GroupBy(claim => claim.Status)
            .Select(group => new StatusStatisticsRow(
                group.Key,
                group.Count(),
                group.Sum(claim => claim.PayoutAmount),
                group.Average(claim => claim.PayoutAmount)))
            .OrderBy(x => x.Status)
            .ToList();
    }

    public static List<MonthlyStatisticsRow> GetMonthlyPayoutStatistics(
        IEnumerable<PolicyHolderEntity> holders)
    {
        ArgumentNullException.ThrowIfNull(holders);

        return holders
            .SelectMany(holder => holder.Policies)
            .SelectMany(policy => policy.Claims)
            .Where(claim => IsApproved(claim.Status))
            .GroupBy(claim => new { claim.IncidentDate.Year, claim.IncidentDate.Month })
            .Select(group => new MonthlyStatisticsRow(
                group.Key.Year,
                group.Key.Month,
                group.Count(),
                group.Sum(claim => claim.PayoutAmount),
                group.Average(claim => claim.PayoutAmount)))
            .OrderBy(x => x.Year)
            .ThenBy(x => x.Month)
            .ToList();
    }
}