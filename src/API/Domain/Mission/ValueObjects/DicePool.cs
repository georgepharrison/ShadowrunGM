using ShadowrunGM.ApiSdk.Common.Results;
using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Mission;

/// <summary>
/// Represents a dice pool for Shadowrun dice mechanics.
/// </summary>
public sealed class DicePool : ValueObject
{
    private DicePool(int attribute, int skill, int modifiers, int edgeBonus, int limit)
    {
        Attribute = attribute;
        Skill = skill;
        Modifiers = modifiers;
        EdgeBonus = edgeBonus;
        Limit = limit;
    }

    /// <summary>
    /// Gets the attribute dice contribution.
    /// </summary>
    public int Attribute { get; }

    /// <summary>
    /// Gets the skill dice contribution.
    /// </summary>
    public int Skill { get; }

    /// <summary>
    /// Gets the modifier dice (positive or negative).
    /// </summary>
    public int Modifiers { get; }

    /// <summary>
    /// Gets the Edge bonus dice.
    /// </summary>
    public int EdgeBonus { get; }

    /// <summary>
    /// Gets the limit for this test (0 means no limit).
    /// </summary>
    public int Limit { get; }

    /// <summary>
    /// Gets the total number of dice in the pool.
    /// </summary>
    public int TotalDice => Math.Max(0, Attribute + Skill + Modifiers + EdgeBonus);

    /// <summary>
    /// Gets whether this pool has a limit applied.
    /// </summary>
    public bool HasLimit => Limit > 0;

    /// <summary>
    /// Gets whether Edge was used to ignore limits.
    /// </summary>
    public bool IgnoresLimit => EdgeBonus > 0 && Limit == 0;

    /// <summary>
    /// Creates a new dice pool.
    /// </summary>
    /// <param name="attribute">The attribute dice.</param>
    /// <param name="skill">The skill dice.</param>
    /// <param name="modifiers">The modifier dice.</param>
    /// <param name="edgeBonus">The Edge bonus dice.</param>
    /// <param name="limit">The limit for the test.</param>
    /// <returns>A Result containing the new dice pool or an error.</returns>
    public static Result<DicePool> Create(
        int attribute,
        int skill = 0,
        int modifiers = 0,
        int edgeBonus = 0,
        int limit = 0)
    {
        if (attribute < 0)
            return Result.Failure<DicePool>("Attribute dice cannot be negative.");

        if (skill < 0)
            return Result.Failure<DicePool>("Skill dice cannot be negative.");

        if (edgeBonus < 0)
            return Result.Failure<DicePool>("Edge bonus cannot be negative.");

        if (limit < 0)
            return Result.Failure<DicePool>("Limit cannot be negative.");

        int totalDice = Math.Max(0, attribute + skill + modifiers + edgeBonus);
        if (totalDice == 0)
            return Result.Failure<DicePool>("Dice pool must have at least 1 die.");

        if (totalDice > 100)
            return Result.Failure<DicePool>("Dice pool cannot exceed 100 dice.");

        return Result.Success(new DicePool(attribute, skill, modifiers, edgeBonus, limit));
    }

    /// <summary>
    /// Gets the atomic values that define this value object.
    /// </summary>
    /// <returns>The collection of atomic values.</returns>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Attribute;
        yield return Skill;
        yield return Modifiers;
        yield return EdgeBonus;
        yield return Limit;
    }

    /// <summary>
    /// Returns the string representation of the dice pool.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString()
    {
        string poolStr = $"{TotalDice}d6";
        if (HasLimit)
            poolStr += $" [Limit {Limit}]";
        if (IgnoresLimit)
            poolStr += " [No Limit - Edge]";
        return poolStr;
    }
}