using InsuranceClaims.App.Analytics;
using InsuranceClaims.App.Data.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace InsuranceClaims.Tests;

[TestClass]
public class InsuranceAnalyticsTests
{
    private static List<PolicyHolderEntity> CreateSampleData()
    {
        var holder = new PolicyHolderEntity { Id = 1, PolicyHolderName = "ТОВ Тест" };

        var policy1 = new InsurancePolicyEntity
        {
            Id = 1,
            PolicyNumber = "POL-1",
            CoverageAmount = 25000m,
            PolicyHolderId = holder.Id,
            Claims = new List<InsuranceClaimEntity>
            {
                new() { IncidentDate = new DateTime(2026, 8, 15), PayoutAmount = 12000m, Status = "Approved" },
                new() { IncidentDate = new DateTime(2026, 8, 20), PayoutAmount = 2500m, Status = "Pending" },
                new() { IncidentDate = new DateTime(2026, 9, 1), PayoutAmount = 10000m, Status = "Approved" }
            }
        };

        var policy2 = new InsurancePolicyEntity
        {
            Id = 2,
            PolicyNumber = "POL-2",
            CoverageAmount = 10000m,
            PolicyHolderId = holder.Id,
            Claims = new List<InsuranceClaimEntity>
            {
                new() { IncidentDate = new DateTime(2026, 9, 5), PayoutAmount = 15000m, Status = "Approved" },
                new() { IncidentDate = new DateTime(2026, 9, 8), PayoutAmount = 0m, Status = "Approved" }
            }
        };

        holder.Policies = new List<InsurancePolicyEntity> { policy1, policy2 };
        return new List<PolicyHolderEntity> { holder };
    }

    [TestMethod]
    public void FindCoverageBreaches_ReturnsPoliciesWhereApprovedPayoutsExceedCoverage()
    {
        var result = InsuranceAnalytics.FindCoverageBreaches(CreateSampleData());

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("POL-2", result[0].PolicyNumber);
        Assert.AreEqual(15000m, result[0].ApprovedPayouts);
        Assert.AreEqual(5000m, result[0].ExcessAmount);
    }

    [TestMethod]
    public void FindInconsistentClaims_FindsNonApprovedPayoutAndZeroApprovedPayout()
    {
        var result = InsuranceAnalytics.FindInconsistentClaims(CreateSampleData());

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.Any(x => x.Status == "Pending" && x.PayoutAmount == 2500m));
        Assert.IsTrue(result.Any(x => x.Status == "Approved" && x.PayoutAmount == 0m));
    }

    [TestMethod]
    public void GetStatusStatistics_GroupsClaimsAndCalculatesAggregates()
    {
        var result = InsuranceAnalytics.GetStatusStatistics(CreateSampleData());

        var approved = result.Single(x => x.Status == "Approved");
        var pending = result.Single(x => x.Status == "Pending");

        Assert.AreEqual(4, approved.ClaimsCount);
        Assert.AreEqual(37000m, approved.TotalPayout);
        Assert.AreEqual(2500m, pending.TotalPayout);
    }

    [TestMethod]
    public void GetMonthlyPayoutStatistics_GroupsApprovedClaimsByMonth()
    {
        var result = InsuranceAnalytics.GetMonthlyPayoutStatistics(CreateSampleData());

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(2026, result[0].Year);
        Assert.AreEqual(8, result[0].Month);
        Assert.AreEqual(12000m, result[0].ApprovedPayouts);
        Assert.AreEqual(25000m, result[1].ApprovedPayouts);
    }

    [TestMethod]
    public void Analytics_EmptyCollection_ReturnsEmptyResults()
    {
        var empty = new List<PolicyHolderEntity>();

        Assert.AreEqual(0, InsuranceAnalytics.FindCoverageBreaches(empty).Count);
        Assert.AreEqual(0, InsuranceAnalytics.FindInconsistentClaims(empty).Count);
        Assert.AreEqual(0, InsuranceAnalytics.GetStatusStatistics(empty).Count);
        Assert.AreEqual(0, InsuranceAnalytics.GetMonthlyPayoutStatistics(empty).Count);
    }
}
