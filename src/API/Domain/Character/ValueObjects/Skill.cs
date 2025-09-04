using ShadowrunGM.Domain.Common;
using FlowRight.Core.Results;
using FlowRight.Validation.Builders;

namespace ShadowrunGM.Domain.Character.ValueObjects;

/// <summary>
/// Represents a character skill with rating and optional specialization.
/// </summary>
public sealed class Skill : ValueObject
{
    private Skill(string name, int rating, string? specialization)
    {
        Name = name;
        Rating = rating;
        Specialization = specialization;
    }

    /// <summary>
    /// Gets the skill name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the skill rating.
    /// </summary>
    public int Rating { get; }

    /// <summary>
    /// Gets the skill specialization, if any.
    /// </summary>
    public string? Specialization { get; }

    /// <summary>
    /// Gets a value indicating whether this skill has a specialization.
    /// </summary>
    public bool HasSpecialization => !string.IsNullOrWhiteSpace(Specialization);

    /// <summary>
    /// Gets the effective dice pool for this skill (including specialization bonus).
    /// </summary>
    /// <param name="isUsingSpecialization">Whether the specialization applies to the current test.</param>
    /// <returns>The total dice pool.</returns>
    public int GetDicePool(bool isUsingSpecialization = false)
    {
        return Rating + (HasSpecialization && isUsingSpecialization ? 2 : 0);
    }

    /// <summary>
    /// Creates a new Skill value object.
    /// </summary>
    /// <param name="name">The skill name.</param>
    /// <param name="rating">The skill rating.</param>
    /// <param name="specialization">Optional skill specialization.</param>
    /// <returns>A Result containing the new Skill or an error.</returns>
    public static Result<Skill> Create(string name, int rating, string? specialization = null) =>
        new ValidationBuilder<Skill>()
            .RuleFor(x => x.Name, name)
                .NotEmpty()
                .WithMessage("Skill name is required")
                .MaximumLength(50)
                .WithMessage("Skill name cannot exceed 50 characters")
            .RuleFor(x => x.Rating, rating)
                .InclusiveBetween(0, 12)
                .WithMessage("Skill rating must be between 0 and 12")
            .RuleFor(x => x.Specialization, specialization ?? string.Empty)
                .MaximumLength(50)
                .WithMessage("Specialization cannot exceed 50 characters")
                .When(spec => !string.IsNullOrWhiteSpace(specialization))
            .Build(() => new Skill(
                name.Trim(),
                rating,
                string.IsNullOrWhiteSpace(specialization) ? null : specialization.Trim()));

    /// <summary>
    /// Creates a new Skill with an updated rating.
    /// </summary>
    /// <param name="newRating">The new skill rating.</param>
    /// <returns>A Result containing the new Skill or an error.</returns>
    public Result<Skill> WithRating(int newRating)
    {
        return Create(Name, newRating, Specialization);
    }

    /// <summary>
    /// Creates a new Skill with an updated specialization.
    /// </summary>
    /// <param name="newSpecialization">The new specialization.</param>
    /// <returns>A Result containing the new Skill or an error.</returns>
    public Result<Skill> WithSpecialization(string? newSpecialization)
    {
        return Create(Name, Rating, newSpecialization);
    }

    /// <summary>
    /// Gets the atomic values that define this value object.
    /// </summary>
    /// <returns>The collection of atomic values.</returns>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Name;
        yield return Rating;
        yield return Specialization;
    }

    /// <summary>
    /// Returns the string representation of the Skill.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString()
    {
        return HasSpecialization 
            ? $"{Name} {Rating} ({Specialization})" 
            : $"{Name} {Rating}";
    }
}