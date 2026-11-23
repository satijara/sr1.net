namespace InsuranceClaims.App.Data.Entities;

public sealed class PolicyHolderEntity
{
    public int Id { get; set; }
    public string PolicyHolderName { get; set; } = string.Empty;

    public ICollection<InsurancePolicyEntity> Policies { get; set; }
        = new List<InsurancePolicyEntity>();
}