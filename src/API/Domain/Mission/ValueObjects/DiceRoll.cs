using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Mission;

/// <summary>
/// Represents a recorded dice roll in a game session.
/// </summary>
public sealed class DiceRoll : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the DiceRoll class.
    /// </summary>
    /// <param name="pool">The dice pool that was rolled.</param>
    /// <param name="outcome">The outcome of the roll.</param>
    /// <param name="rolledAt">When the roll occurred.</param>
    public DiceRoll(DicePool pool, DiceOutcome outcome, DateTime rolledAt)
    {
        Pool = pool;
        Outcome = outcome;
        RolledAt = rolledAt;
    }

    /// <summary>
    /// Gets the dice pool that was rolled.
    /// </summary>
    public DicePool Pool { get; }

    /// <summary>
    /// Gets the outcome of the roll.
    /// </summary>
    public DiceOutcome Outcome { get; }

    /// <summary>
    /// Gets when the roll occurred.
    /// </summary>
    public DateTime RolledAt { get; }

    /// <summary>
    /// Gets the atomic values that define this value object.
    /// </summary>
    /// <returns>The collection of atomic values.</returns>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Pool;
        yield return Outcome;
        yield return RolledAt;
    }
}