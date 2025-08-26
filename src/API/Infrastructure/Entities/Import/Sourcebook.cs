namespace ShadowrunGM.API.Infrastructure.Entities.Import;

public sealed class Sourcebook
{
    public long Id { get; set; }

    // Business identity (unique)
    public string Code { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Edition { get; set; } = "6e";

    // File metadata
    public string FileName { get; set; } = null!;
    public string FileHash { get; set; } = null!;

    // Book info
    public int? Year { get; set; }

    // Audit
    public DateTime CreatedUtc { get; set; }
    public DateTime UpdatedUtc { get; set; }
    public DateTimeOffset ImportedAt { get; internal set; }

    // Navigation
    public ICollection<RuleContent> Contents { get; set; } = [];
}
