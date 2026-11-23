using InsuranceClaims.App.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceClaims.App.Data;

public sealed class InsuranceDbContext : DbContext
{
    public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options)
        : base(options)
    {
    }

    public DbSet<PolicyHolderEntity> PolicyHolders => Set<PolicyHolderEntity>();
    public DbSet<InsurancePolicyEntity> InsurancePolicies => Set<InsurancePolicyEntity>();
    public DbSet<InsuranceClaimEntity> InsuranceClaims => Set<InsuranceClaimEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PolicyHolderEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PolicyHolderName)
                .HasMaxLength(200)
                .IsRequired();
            entity.HasIndex(x => x.PolicyHolderName);

            entity.HasMany(x => x.Policies)
                .WithOne(x => x.PolicyHolder)
                .HasForeignKey(x => x.PolicyHolderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InsurancePolicyEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PolicyNumber)
                .HasMaxLength(60)
                .IsRequired();
            entity.Property(x => x.CoverageAmount).HasPrecision(18, 2);
            entity.HasIndex(x => x.PolicyNumber).IsUnique();
            entity.HasIndex(x => x.CoverageAmount);
            entity.HasIndex(x => x.PolicyHolderId);

            entity.HasMany(x => x.Claims)
                .WithOne(x => x.InsurancePolicy)
                .HasForeignKey(x => x.InsurancePolicyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InsuranceClaimEntity>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.PayoutAmount).HasPrecision(18, 2);
            entity.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();
            entity.HasIndex(x => x.IncidentDate);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.InsurancePolicyId);
        });
    }
}