namespace InsuranceClaims.App.Data.Entities;

public sealed class InsuranceClaimEntity
{
    public int Id { get; set; }
    public DateTime IncidentDate { get; set; }
    public decimal PayoutAmount { get; set; }
    public string Status { get; set; } = string.Empty;

    public int InsurancePolicyId { get; set; }
    public InsurancePolicyEntity InsurancePolicy { get; set; } = null!;
}