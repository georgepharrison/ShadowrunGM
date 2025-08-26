using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShadowrunGM.API.Infrastructure.Entities.Catalog;

namespace ShadowrunGM.API.Infrastructure.Configurations.Catalog;

public sealed class GameItemConfiguration : IEntityTypeConfiguration<GameItem>
{
    #region Public Methods

    public void Configure(EntityTypeBuilder<GameItem> b)
    {
        // Table + key
        b.ToTable("game_items");
        b.HasKey(x => x.Id);

        // Core
        b.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Slug)
            .HasMaxLength(220)
            .IsRequired();

        // Classification
        b.Property(x => x.ItemType)
            .HasMaxLength(50)
            .IsRequired(); // "Weapon" | "Armor" | "Gear" | ...
        b.Property(x => x.Category)
            .HasMaxLength(100);

        // Stats
        b.Property(x => x.Cost)
            .HasColumnType("numeric(12,2)");
        b.Property(x => x.Availability)
            .HasMaxLength(10);

        b.Property(x => x.StatsJson)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}")
            .IsRequired();

        // Source reference
        b.Property(x => x.SourcebookId).IsRequired();
        b.Property(x => x.Page);

        // Embeddings
        b.Property(x => x.Embedding)
            .HasColumnName("embedding")
            .HasColumnType("vector(768)");
        b.Property(x => x.EmbeddingModel)
            .HasMaxLength(100);
        b.Property(x => x.EmbeddingVersion);

        // Audit
        b.Property(x => x.CreatedAt)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();
        b.Property(x => x.UpdatedAt)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        // Relations
        b.HasOne<Entities.Import.Sourcebook>()           // no nav prop on entity; FK-only
            .WithMany()
            .HasForeignKey(x => x.SourcebookId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes (btree)
        b.HasIndex(x => x.Slug).IsUnique();
        b.HasIndex(x => new { x.ItemType, x.Category });
        b.HasIndex(x => x.Cost);
        b.HasIndex(x => x.SourcebookId);
        b.HasIndex(x => new { x.SourcebookId, x.Page });

        // Optional fuzzy search on name (enable pg_trgm extension first)
        // b.HasIndex(x => x.Name).HasMethod("gin").HasOperators("gin_trgm_ops");

        // Optional JSONB GIN index for StatsJson (add via raw SQL in migration)
        // migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_game_items_statsjson ON game_items USING gin (\"StatsJson\" jsonb_path_ops);");

        // Optional IVFFlat/HNSW index on embedding (raw SQL in migration; requires pgvector)
        // migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_game_items_embedding_ivf ON game_items USING ivfflat (embedding vector_l2_ops) WITH (lists = 100);");
    }

    #endregion Public Methods
}