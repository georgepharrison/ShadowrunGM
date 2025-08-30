using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShadowrunGM.Domain.Character;
using ShadowrunGM.Domain.Character.ValueObjects;

namespace ShadowrunGM.API.Infrastructure.Configurations.Character;

/// <summary>
/// Entity Framework configuration for Character aggregate.
/// </summary>
public sealed class CharacterConfiguration : IEntityTypeConfiguration<ShadowrunGM.Domain.Character.Character>
{
    /// <summary>
    /// Configures the Character entity mapping.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<ShadowrunGM.Domain.Character.Character> builder)
    {
        // Table configuration
        builder.ToTable("characters");

        // Primary key - CharacterId value object
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                guid => CharacterId.From(guid))
            .IsRequired();

        // Basic properties
        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        // AttributeSet value object - stored as separate columns
        builder.OwnsOne(x => x.Attributes, attrs =>
        {
            attrs.Property(a => a.Body).HasColumnName("Body").IsRequired();
            attrs.Property(a => a.Agility).HasColumnName("Agility").IsRequired();
            attrs.Property(a => a.Reaction).HasColumnName("Reaction").IsRequired();
            attrs.Property(a => a.Strength).HasColumnName("Strength").IsRequired();
            attrs.Property(a => a.Willpower).HasColumnName("Willpower").IsRequired();
            attrs.Property(a => a.Logic).HasColumnName("Logic").IsRequired();
            attrs.Property(a => a.Intuition).HasColumnName("Intuition").IsRequired();
            attrs.Property(a => a.Charisma).HasColumnName("Charisma").IsRequired();
        });

        // Edge value object - stored as separate columns
        builder.OwnsOne(x => x.Edge, edge =>
        {
            edge.Property(e => e.Current).HasColumnName("CurrentEdge").IsRequired();
            edge.Property(e => e.Max).HasColumnName("MaxEdge").IsRequired();
        });

        // ConditionMonitor value object - stored as separate columns  
        builder.OwnsOne(x => x.Health, health =>
        {
            health.Property(h => h.PhysicalDamage).HasColumnName("PhysicalDamage").IsRequired();
            health.Property(h => h.StunDamage).HasColumnName("StunDamage").IsRequired();
            health.Property(h => h.PhysicalBoxes).HasColumnName("PhysicalBoxes").IsRequired();
            health.Property(h => h.StunBoxes).HasColumnName("StunBoxes").IsRequired();
        });

        // Skills collection - ignored for now to simplify initial implementation
        builder.Ignore(x => x.Skills);

        // Timestamps
        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        builder.Property(x => x.ModifiedAt)
            .HasDefaultValueSql("timezone('utc', now())")
            .IsRequired();

        // Indexes
        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.ModifiedAt);

        // Ignore domain events (not persisted)
        builder.Ignore(x => x.DomainEvents);
    }
}