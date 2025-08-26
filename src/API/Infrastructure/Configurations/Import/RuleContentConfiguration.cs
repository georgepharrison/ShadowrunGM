using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShadowrunGM.API.Infrastructure.Entities.Import;

namespace ShadowrunGM.API.Infrastructure.Configurations.Import;

public sealed class RuleContentConfiguration : IEntityTypeConfiguration<RuleContent>
{
    #region Public Methods

    public void Configure(EntityTypeBuilder<RuleContent> b)
    {
        b.ToTable("rule_contents");
        b.HasKey(x => x.Id);

        // Required data
        b.Property(x => x.SourcebookId).IsRequired();
        b.Property(x => x.SequenceNumber).IsRequired();
        b.Property(x => x.Content).IsRequired();

        // Optional labels
        b.Property(x => x.Heading);
        b.Property(x => x.HeadingTitle).HasColumnName("heading_title");   // NEW
        b.Property(x => x.HeadingLevel).HasColumnName("heading_level").HasDefaultValue(0).IsRequired(); // NEW
        b.Property(x => x.Section);
        b.Property(x => x.ContentType).HasMaxLength(50);
        b.Property(x => x.PageNumber);

        // Audit
        b.Property(x => x.CreatedAt)
         .HasDefaultValueSql("timezone('utc', now())");

        // Self-reference (SET NULL on delete so children don't become orphans)
        b.HasOne(x => x.Parent)
         .WithMany()
         .HasForeignKey(x => x.ParentContentId)
         .OnDelete(DeleteBehavior.SetNull); // NEW (was Restrict)

        // Vector column (match your model’s dim)
        b.Property(x => x.Embedding)
         .HasColumnName("embedding")
         .HasColumnType("vector(768)");

        b.Property(x => x.EmbeddingModel).HasColumnName("embedding_model").HasMaxLength(100);
        b.Property(x => x.EmbeddingVersion).HasColumnName("embedding_version");

        // Compute in DB using a stored generated column.
        // Requires Postgres 12+ (ok) and md5(text) is immutable (ok).
        b.Property(x => x.SourceHash)
         .HasColumnName("source_hash")
         .HasMaxLength(32)
         .HasComputedColumnSql("md5(content)", stored: true);

        // ---------- Indexes ----------
        // Book-scoped ordering
        b.HasIndex(x => new { x.SourcebookId, x.SequenceNumber });

        // Useful filters
        b.HasIndex(x => new { x.SourcebookId, x.ContentType });
        b.HasIndex(x => new { x.SourcebookId, x.PageNumber });

        // Idempotency/dup-detection (book + hash)
        b.HasIndex(x => new { x.SourcebookId, x.SourceHash }).IsUnique();

        // Fuzzy search on content (requires pg_trgm)
        b.HasIndex(x => x.Content)
         .HasMethod("gin")
         .HasOperators("gin_trgm_ops");

        // Fuzzy search on headings (requires pg_trgm)
        b.HasIndex(x => x.Heading).HasMethod("gin").HasOperators("gin_trgm_ops");
        b.HasIndex(x => x.HeadingTitle).HasMethod("gin").HasOperators("gin_trgm_ops"); // NEW

        // (Optional) quick filters by level within a book
        b.HasIndex(x => new { x.SourcebookId, x.HeadingLevel }); // NEW
    }

    #endregion Public Methods
}