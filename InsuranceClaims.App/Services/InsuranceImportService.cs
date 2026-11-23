using InsuranceClaims.App.Data;
using InsuranceClaims.App.Data.Entities;
using InsuranceClaims.App.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.App.Services;

public sealed class InsuranceImportService
{
    private readonly InsuranceDbContext _db;
    private readonly InsuranceJsonReader _reader;
    private readonly InsuranceDataValidator _validator;

    public InsuranceImportService(
        InsuranceDbContext db,
        InsuranceJsonReader reader,
        InsuranceDataValidator validator)
    {
        _db = db;
        _reader = reader;
        _validator = validator;
    }

    public async Task<int> ImportAsync(
        string jsonPath,
        CancellationToken cancellationToken = default)
    {
        await _db.Database.EnsureCreatedAsync(cancellationToken);
        await using var transaction =
            await _db.Database.BeginTransactionAsync(cancellationToken);

        await _db.InsuranceClaims.ExecuteDeleteAsync(cancellationToken);
        await _db.InsurancePolicies.ExecuteDeleteAsync(cancellationToken);
        await _db.PolicyHolders.ExecuteDeleteAsync(cancellationToken);

        int importedHolders = 0;

        await foreach (PolicyHolderDto dto in
            _reader.ReadPolicyHoldersAsync(jsonPath, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            _validator.Validate(dto);

            var holder = new PolicyHolderEntity
            {
                PolicyHolderName = dto.PolicyHolder.Trim(),
                Policies = dto.Policies.Select(policy => new InsurancePolicyEntity
                {
                    PolicyNumber = policy.PolicyNumber.Trim(),
                    CoverageAmount = policy.CoverageAmount,
                    Claims = policy.Claims.Select(claim => new InsuranceClaimEntity
                    {
                        IncidentDate = claim.IncidentDate,
                        PayoutAmount = claim.PayoutAmount,
                        Status = claim.Status.Trim()
                    }).ToList()
                }).ToList()
            };

            _db.PolicyHolders.Add(holder);
            await _db.SaveChangesAsync(cancellationToken);
            _db.ChangeTracker.Clear();
            importedHolders++;
        }

        await transaction.CommitAsync(cancellationToken);
        return importedHolders;
    }
}