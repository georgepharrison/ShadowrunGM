using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Character.ValueObjects;

/// <summary>
/// Represents the complete set of attributes for a Shadowrun character.
/// </summary>
public sealed class AttributeSet : ValueObject
{
    private AttributeSet(
        int body,
        int agility,
        int reaction,
        int strength,
        int willpower,
        int logic,
        int intuition,
        int charisma)
    {
        Body = body;
        Agility = agility;
        Reaction = reaction;
        Strength = strength;
        Willpower = willpower;
        Logic = logic;
        Intuition = intuition;
        Charisma = charisma;
    }

    /// <summary>
    /// Gets the Body attribute value.
    /// </summary>
    public int Body { get; }

    /// <summary>
    /// Gets the Agility attribute value.
    /// </summary>
    public int Agility { get; }

    /// <summary>
    /// Gets the Reaction attribute value.
    /// </summary>
    public int Reaction { get; }

    /// <summary>
    /// Gets the Strength attribute value.
    /// </summary>
    public int Strength { get; }

    /// <summary>
    /// Gets the Willpower attribute value.
    /// </summary>
    public int Willpower { get; }

    /// <summary>
    /// Gets the Logic attribute value.
    /// </summary>
    public int Logic { get; }

    /// <summary>
    /// Gets the Intuition attribute value.
    /// </summary>
    public int Intuition { get; }

    /// <summary>
    /// Gets the Charisma attribute value.
    /// </summary>
    public int Charisma { get; }

    /// <summary>
    /// Gets the calculated Initiative value (Reaction + Intuition).
    /// </summary>
    public int Initiative => Reaction + Intuition;

    /// <summary>
    /// Gets the calculated Physical Limit.
    /// </summary>
    public int PhysicalLimit => (int)Math.Ceiling(((Strength * 2) + Body + Reaction) / 3.0);

    /// <summary>
    /// Gets the calculated Mental Limit.
    /// </summary>
    public int MentalLimit => (int)Math.Ceiling(((Logic * 2) + Intuition + Willpower) / 3.0);

    /// <summary>
    /// Gets the calculated Social Limit.
    /// </summary>
    public int SocialLimit => (int)Math.Ceiling(((Charisma * 2) + Willpower + (Body + Willpower + 6) / 12.0) / 3.0);

    /// <summary>
    /// Creates a new AttributeSet with the specified values.
    /// </summary>
    /// <param name="body">The Body attribute value.</param>
    /// <param name="agility">The Agility attribute value.</param>
    /// <param name="reaction">The Reaction attribute value.</param>
    /// <param name="strength">The Strength attribute value.</param>
    /// <param name="willpower">The Willpower attribute value.</param>
    /// <param name="logic">The Logic attribute value.</param>
    /// <param name="intuition">The Intuition attribute value.</param>
    /// <param name="charisma">The Charisma attribute value.</param>
    /// <returns>A Result containing the new AttributeSet or an error.</returns>
    public static Result<AttributeSet> Create(
        int body,
        int agility,
        int reaction,
        int strength,
        int willpower,
        int logic,
        int intuition,
        int charisma)
    {
        if (!IsValidAttributeValue(body))
            return Result<AttributeSet>.Failure($"Body attribute must be between 1 and 10. Value: {body}");
        
        if (!IsValidAttributeValue(agility))
            return Result<AttributeSet>.Failure($"Agility attribute must be between 1 and 10. Value: {agility}");
        
        if (!IsValidAttributeValue(reaction))
            return Result<AttributeSet>.Failure($"Reaction attribute must be between 1 and 10. Value: {reaction}");
        
        if (!IsValidAttributeValue(strength))
            return Result<AttributeSet>.Failure($"Strength attribute must be between 1 and 10. Value: {strength}");
        
        if (!IsValidAttributeValue(willpower))
            return Result<AttributeSet>.Failure($"Willpower attribute must be between 1 and 10. Value: {willpower}");
        
        if (!IsValidAttributeValue(logic))
            return Result<AttributeSet>.Failure($"Logic attribute must be between 1 and 10. Value: {logic}");
        
        if (!IsValidAttributeValue(intuition))
            return Result<AttributeSet>.Failure($"Intuition attribute must be between 1 and 10. Value: {intuition}");
        
        if (!IsValidAttributeValue(charisma))
            return Result<AttributeSet>.Failure($"Charisma attribute must be between 1 and 10. Value: {charisma}");

        return Result<AttributeSet>.Success(new AttributeSet(
            body, agility, reaction, strength, willpower, logic, intuition, charisma));
    }

    /// <summary>
    /// Creates a new AttributeSet from a dictionary of attribute values.
    /// </summary>
    /// <param name="attributes">Dictionary mapping attribute names to values.</param>
    /// <returns>A Result containing the new AttributeSet or an error.</returns>
    public static Result<AttributeSet> Create(Dictionary<string, int> attributes)
    {
        if (attributes == null)
            return Result<AttributeSet>.Failure("Attributes dictionary cannot be null.");

        if (!TryGetAttribute(attributes, "Body", out int body) ||
            !TryGetAttribute(attributes, "Agility", out int agility) ||
            !TryGetAttribute(attributes, "Reaction", out int reaction) ||
            !TryGetAttribute(attributes, "Strength", out int strength) ||
            !TryGetAttribute(attributes, "Willpower", out int willpower) ||
            !TryGetAttribute(attributes, "Logic", out int logic) ||
            !TryGetAttribute(attributes, "Intuition", out int intuition) ||
            !TryGetAttribute(attributes, "Charisma", out int charisma))
        {
            return Result<AttributeSet>.Failure("Missing required attributes. All attributes (Body, Agility, Reaction, Strength, Willpower, Logic, Intuition, Charisma) must be provided.");
        }

        return Create(body, agility, reaction, strength, willpower, logic, intuition, charisma);
    }

    private static bool TryGetAttribute(Dictionary<string, int> attributes, string name, out int value)
    {
        value = 0;
        foreach (KeyValuePair<string, int> kvp in attributes)
        {
            if (string.Equals(kvp.Key, name, StringComparison.OrdinalIgnoreCase))
            {
                value = kvp.Value;
                return true;
            }
        }
        return false;
    }

    private static bool IsValidAttributeValue(int value)
    {
        return value >= 1 && value <= 10;
    }

    /// <summary>
    /// Gets the atomic values that define this value object.
    /// </summary>
    /// <returns>The collection of atomic values.</returns>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Body;
        yield return Agility;
        yield return Reaction;
        yield return Strength;
        yield return Willpower;
        yield return Logic;
        yield return Intuition;
        yield return Charisma;
    }
}