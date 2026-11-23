using InsuranceClaims.App.DTOs;
using System.IO;

namespace InsuranceClaims.App.Services;

public sealed class InsuranceDataValidator
{
    private static readonly HashSet<string> AllowedStatuses = new(
        new[] { "Approved", "Pending", "Rejected" },
        StringComparer.OrdinalIgnoreCase);

    public void Validate(PolicyHolderDto holder)
    {
        ArgumentNullException.ThrowIfNull(holder);

        if (string.IsNullOrWhiteSpace(holder.PolicyHolder))
            throw new InvalidDataException("Не вказано власника страхового поліса.");

        if (holder.Policies is null)
            throw new InvalidDataException("Список полісів не може бути null.");

        foreach (InsurancePolicyDto policy in holder.Policies)
        {
            if (policy is null)
                throw new InvalidDataException("Поліс не може бути null.");

            if (string.IsNullOrWhiteSpace(policy.PolicyNumber))
                throw new InvalidDataException("Не вказано номер поліса.");

            if (policy.CoverageAmount <= 0)
                throw new InvalidDataException(
                    $"Сума покриття поліса {policy.PolicyNumber} має бути більшою за нуль.");

            if (policy.Claims is null)
                throw new InvalidDataException(
                    $"Список страхових випадків поліса {policy.PolicyNumber} не може бути null.");

            foreach (InsuranceClaimDto claim in policy.Claims)
            {
                if (claim is null)
                    throw new InvalidDataException("Страховий випадок не може бути null.");

                if (claim.IncidentDate == default)
                    throw new InvalidDataException("Не вказано коректну дату страхового випадку.");

                if (claim.PayoutAmount < 0)
                    throw new InvalidDataException("Сума виплати не може бути від'ємною.");

                if (string.IsNullOrWhiteSpace(claim.Status) || !AllowedStatuses.Contains(claim.Status))
                    throw new InvalidDataException(
                        "Статус має бути Approved, Pending або Rejected.");
            }
        }
    }
}