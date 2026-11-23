using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using InsuranceClaims.App.DTOs;

namespace InsuranceClaims.App.Services;

public sealed class InsuranceJsonReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async IAsyncEnumerable<PolicyHolderDto> ReadPolicyHoldersAsync(
        string jsonPath,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(jsonPath))
            throw new ArgumentException("Не вказано шлях до JSON-файлу.", nameof(jsonPath));

        if (!File.Exists(jsonPath))
            throw new FileNotFoundException("JSON-файл не знайдено.", jsonPath);

        await using var stream = new FileStream(
            jsonPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 65536,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        await foreach (PolicyHolderDto? holder in
            JsonSerializer.DeserializeAsyncEnumerable<PolicyHolderDto>(
                stream, JsonOptions, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (holder is null)
                throw new JsonException("Елемент власника поліса не може бути null.");

            yield return holder;
        }
    }
}