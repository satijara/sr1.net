namespace InsuranceClaims.App.Data.Entities;

public sealed class InsurancePolicyEntity
{
    public int Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public decimal CoverageAmount { get; set; }

    public int PolicyHolderId { get; set; }
    public PolicyHolderEntity PolicyHolder { get; set; } = null!;

    public ICollection<InsuranceClaimEntity> Claims { get; set; }
        = new List<InsuranceClaimEntity>();
}