using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShadowrunGM.API.Infrastructure.Entities.Catalog;

namespace ShadowrunGM.API.Infrastructure.Configurations.Catalog;

public sealed class MagicAbilityConfiguration : IEntityTypeConfiguration<MagicAbility>
{
    #region Public Methods

    public void Configure(EntityTypeBuilder<MagicAbility> b)
    {
        // Table + key
        b.ToTable("magic_abilities");
        b.HasKey(x => x.Id);

        // Core
        b.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        b.Property(x => x.Slug)
            .HasMaxLength(220)
            .IsRequired();

        b.Property(x => x.AbilityType)
            .HasMaxLength(50)
            .IsRequired(); // "Spell" | "AdeptPower" | "Ritual" | "Metamagic"

        // Classification
        b.Property(x => x.Category).HasMaxLength(100);
        b.Property(x => x.Range).HasMaxLength(80);
        b.Property(x => x.Duration).HasMaxLength(80);
        b.Property(x => x.Type).HasMaxLength(40); // "M" | "P"

        // Stats
        b.Property(x => x.DrainValue);
        b.Property(x => x.PowerPointCost).HasColumnType("numeric(6,2)");

        b.Property(x => x.ExtraJson)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}")
            .IsRequired();

        // Source reference
        b.Property(x => x.SourcebookId).IsRequired();
        b.Property(x => x.Page);
        b.Property(x => x.IsVerified)
            .HasDefaultValue(false)
            .IsRequired();

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
        b.HasOne<Entities.Import.Sourcebook>()           // FK-only
            .WithMany()
            .HasForeignKey(x => x.SourcebookId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes (btree)
        b.HasIndex(x => x.Slug).IsUnique();
        b.HasIndex(x => new { x.AbilityType, x.Category });
        b.HasIndex(x => x.SourcebookId);
        b.HasIndex(x => new { x.SourcebookId, x.Page });
        b.HasIndex(x => x.IsVerified);

        // Optional fuzzy search on name (pg_trgm)
        // b.HasIndex(x => x.Name).HasMethod("gin").HasOperators("gin_trgm_ops");

        // Optional JSONB GIN index for ExtraJson (raw SQL in migration)
        // migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_magic_abilities_extrajson ON magic_abilities USING gin (\"ExtraJson\" jsonb_path_ops);");

        // Optional IVFFlat/HNSW index on embedding (raw SQL in migration)
        // migrationBuilder.Sql("CREATE INDEX IF NOT EXISTS ix_magic_abilities_embedding_ivf ON magic_abilities USING ivfflat (embedding vector_l2_ops) WITH (lists = 100);");
    }

    #endregion Public Methods
}