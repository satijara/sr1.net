namespace InsuranceClaims.App.Analytics;

public sealed record CoverageBreachRow(
    string PolicyHolder,
    string PolicyNumber,
    decimal CoverageAmount,
    decimal ApprovedPayouts,
    decimal ExcessAmount);

public sealed record InconsistentClaimRow(
    string PolicyHolder,
    string PolicyNumber,
    DateTime IncidentDate,
    decimal PayoutAmount,
    string Status,
    string Explanation);

public sealed record StatusStatisticsRow(
    string Status,
    int ClaimsCount,
    decimal TotalPayout,
    decimal AveragePayout);

public sealed record MonthlyStatisticsRow(
    int Year,
    int Month,
    int ClaimsCount,
    decimal ApprovedPayouts,
    decimal AveragePayout);
