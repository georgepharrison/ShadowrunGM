namespace ShadowrunGM.API.Importing.Contracts;

public sealed record ImportWorkItem(
    Guid JobId,
    string SourceFilename,
    string BlobUri,
    string ContentHash,
    string? SubmittedByUserId,
    string? TenantId,
    string TraceId,
    string Code,
    string Title,
    string Edition,
    int? Year);