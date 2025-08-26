using Pgvector;

namespace ShadowrunGM.API.Infrastructure.Entities.Import;

public sealed class RuleContent
{
    public long Id { get; set; }

    // Ownership
    public long SourcebookId { get; set; }
    public Sourcebook Sourcebook { get; set; } = null!;

    // Positioning within the book
    public int SequenceNumber { get; set; }   // Monotonic, for stable ordering
    public int? PageNumber { get; set; }
    public long? ParentContentId { get; set; }
    public RuleContent? Parent { get; set; }

    // Labels
    public string? Heading { get; set; }
    public string? HeadingTitle { get; set; }   // NEW: leaf title of the heading/breadcrumb
    public int HeadingLevel { get; set; } = 0;  // NEW: 0 = no heading, 1..6 = #..######
    public string? Section { get; set; }        // top-level section, optional
    public string? ContentType { get; set; }    // "text" | "table" | "spell" | etc.

    // Text
    public string Content { get; set; } = null!;

    // DB-computed, don't let code set it
    public string SourceHash { get; private set; } = null!;

    // Embedding (pgvector)
    public Vector? Embedding { get; set; }
    public string? EmbeddingModel { get; set; } = "nomic-embed-text";
    public int? EmbeddingVersion { get; set; } = 1;

    // Audit
    public DateTimeOffset CreatedAt { get; set; }
}
