namespace ShadowrunGM.Domain.Mission;

/// <summary>
/// Domain service for dice rolling mechanics.
/// </summary>
public interface IDiceService
{
    /// <summary>
    /// Rolls a dice pool and calculates the outcome.
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <returns>The outcome of the dice roll.</returns>
    DiceOutcome Roll(DicePool pool);

    /// <summary>
    /// Rolls a dice pool with exploding 6s (Edge rule).
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <returns>The outcome of the dice roll with exploding 6s.</returns>
    DiceOutcome RollWithExplodingSixes(DicePool pool);

    /// <summary>
    /// Rerolls failed dice (Edge rule).
    /// </summary>
    /// <param name="originalOutcome">The original roll outcome.</param>
    /// <returns>The outcome after rerolling failures.</returns>
    DiceOutcome RerollFailures(DiceOutcome originalOutcome);
}