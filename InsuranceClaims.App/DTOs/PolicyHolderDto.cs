using System.Text.Json.Serialization;

namespace InsuranceClaims.App.DTOs;

public sealed class PolicyHolderDto
{
    [JsonPropertyName("policyHolder")]
    public string PolicyHolder { get; set; } = string.Empty;

    [JsonPropertyName("policies")]
    public List<InsurancePolicyDto> Policies { get; set; } = new();
}

public sealed class InsurancePolicyDto
{
    [JsonPropertyName("policyNumber")]
    public string PolicyNumber { get; set; } = string.Empty;

    [JsonPropertyName("coverageAmount")]
    public decimal CoverageAmount { get; set; }

    [JsonPropertyName("claims")]
    public List<InsuranceClaimDto> Claims { get; set; } = new();
}

public sealed class InsuranceClaimDto
{
    [JsonPropertyName("incidentDate")]
    public DateTime IncidentDate { get; set; }

    [JsonPropertyName("payoutAmount")]
    public decimal PayoutAmount { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}