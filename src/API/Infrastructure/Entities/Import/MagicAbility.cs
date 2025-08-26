using Pgvector;

namespace ShadowrunGM.API.Infrastructure.Entities.Catalog;

/// <summary>
/// Unified table for magical abilities (spells, adept powers, rituals, metamagics).
/// Uses JSONB for extensibility on subtype-specific stats.
/// </summary>
public sealed class MagicAbility
{
    // Identity
    public long Id { get; set; }

    // Core fields
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string AbilityType { get; set; } = null!;     // "Spell" | "AdeptPower" | "Ritual" | "Metamagic"

    // Classification
    public string? Category { get; set; }                // "Combat", "Detection", ...
    public string? Range { get; set; }                   // "LOS", "Touch", ...
    public string? Duration { get; set; }                // "S", "I", "P"
    public string? Type { get; set; }                    // "M" | "P" (mana/physical)

    // Stats
    public int? DrainValue { get; set; }                 // spells
    public decimal? PowerPointCost { get; set; }         // adept powers
    public string ExtraJson { get; set; } = "{}";        // JSONB extensibility

    // Source reference
    public long SourcebookId { get; set; }
    public int? Page { get; set; }
    public bool IsVerified { get; set; }

    // Embeddings
    public Vector? Embedding { get; set; }
    public string? EmbeddingModel { get; set; }
    public int? EmbeddingVersion { get; set; }

    // Audit
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
