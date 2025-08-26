using Pgvector;

namespace ShadowrunGM.API.Infrastructure.Entities.Catalog;

/// <summary>
/// Unified table for all equipment, gear, augmentations, and weapons.
/// Uses JSONB for extensibility on subtype-specific stats.
/// </summary>
public sealed class GameItem
{
    // Identity
    public long Id { get; set; }

    // Core fields
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;           // unique normalized key

    // Classification
    public string ItemType { get; set; } = null!;       // "Weapon" | "Armor" | "Gear" | "Augmentation" | "Vehicle" | "Drone" | "Focus" | "Ammo" | ...
    public string? Category { get; set; }               // e.g. "Light Pistol", "Cyberarm", etc.

    // Stats
    public decimal? Cost { get; set; }
    public string? Availability { get; set; }           // e.g. "8R", "12F"
    public string StatsJson { get; set; } = "{}";       // JSONB extensibility

    // Source reference
    public long SourcebookId { get; set; }
    public int? Page { get; set; }

    // Embeddings
    public Vector? Embedding { get; set; }
    public string? EmbeddingModel { get; set; }
    public int? EmbeddingVersion { get; set; }

    // Audit
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
