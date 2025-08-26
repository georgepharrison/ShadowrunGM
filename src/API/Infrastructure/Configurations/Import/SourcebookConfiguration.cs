using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShadowrunGM.API.Infrastructure.Entities.Import;

namespace ShadowrunGM.API.Infrastructure.Configurations.Import;

public sealed class SourcebookConfiguration : IEntityTypeConfiguration<Sourcebook>
{
    #region Public Methods

    public void Configure(EntityTypeBuilder<Sourcebook> b)
    {
        b.ToTable("sourcebooks");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).HasMaxLength(50).IsRequired();
        b.Property(x => x.Title).HasMaxLength(400).IsRequired();
        b.Property(x => x.Edition).HasMaxLength(50).IsRequired().HasDefaultValue("6e");

        b.Property(x => x.FileName).HasMaxLength(400).IsRequired();
        b.Property(x => x.FileHash).HasMaxLength(128).IsRequired();

        b.Property(x => x.Year);

        // Timestamps (DB defaults)
        b.Property(x => x.ImportedAt).HasDefaultValueSql("timezone('utc', now())");
        b.Property(x => x.CreatedUtc).HasDefaultValueSql("timezone('utc', now())");
        b.Property(x => x.UpdatedUtc).HasDefaultValueSql("timezone('utc', now())");

        // Indexes
        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => x.Title);
        b.HasIndex(x => x.FileHash);

        // Relationship
        b.HasMany(x => x.Contents)
         .WithOne(x => x.Sourcebook)
         .HasForeignKey(x => x.SourcebookId)
         .OnDelete(DeleteBehavior.Cascade);
    }

    #endregion Public Methods
}