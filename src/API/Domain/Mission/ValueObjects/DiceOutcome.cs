using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Mission;

/// <summary>
/// Represents the outcome of a dice roll.
/// </summary>
public sealed class DiceOutcome : ValueObject
{
    /// <summary>
    /// Initializes a new instance of the DiceOutcome class.
    /// </summary>
    /// <param name="rolls">The individual die results.</param>
    /// <param name="hits">The number of hits (5s and 6s).</param>
    /// <param name="ones">The number of 1s rolled.</param>
    /// <param name="isGlitch">Whether this is a glitch.</param>
    /// <param name="isCriticalGlitch">Whether this is a critical glitch.</param>
    public DiceOutcome(
        IReadOnlyList<int> rolls,
        int hits,
        int ones,
        bool isGlitch,
        bool isCriticalGlitch)
    {
        Rolls = rolls?.ToArray() ?? Array.Empty<int>(); // Defensive copy to prevent mutation
        Hits = hits;
        Ones = ones;
        IsGlitch = isGlitch;
        IsCriticalGlitch = isCriticalGlitch;
    }

    /// <summary>
    /// Gets the individual die results.
    /// </summary>
    public IReadOnlyList<int> Rolls { get; }

    /// <summary>
    /// Gets the number of hits (5s and 6s).
    /// </summary>
    public int Hits { get; }

    /// <summary>
    /// Gets the number of 1s rolled.
    /// </summary>
    public int Ones { get; }

    /// <summary>
    /// Gets whether this is a glitch (more than half the dice show 1).
    /// </summary>
    public bool IsGlitch { get; }

    /// <summary>
    /// Gets whether this is a critical glitch (glitch with no hits).
    /// </summary>
    public bool IsCriticalGlitch { get; }

    /// <summary>
    /// Gets whether the roll was successful (has hits and no critical glitch).
    /// </summary>
    public bool IsSuccess => Hits > 0 && !IsCriticalGlitch;

    /// <summary>
    /// Gets whether the roll was a failure (no hits or critical glitch).
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Creates a new DiceOutcome.
    /// </summary>
    /// <param name="rolls">The individual die results.</param>
    /// <param name="hits">The number of hits.</param>
    /// <param name="ones">The number of ones.</param>
    /// <param name="isGlitch">Whether this is a glitch.</param>
    /// <param name="isCriticalGlitch">Whether this is a critical glitch.</param>
    /// <returns>A new DiceOutcome.</returns>
    public static DiceOutcome Create(
        IReadOnlyList<int> rolls,
        int hits,
        int ones,
        bool isGlitch,
        bool isCriticalGlitch) =>
        new(rolls, hits, ones, isGlitch, isCriticalGlitch);

    /// <summary>
    /// Gets the atomic values that define this value object.
    /// </summary>
    /// <returns>The collection of atomic values.</returns>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        foreach (int roll in Rolls)
            yield return roll;
        yield return Hits;
        yield return Ones;
        yield return IsGlitch;
        yield return IsCriticalGlitch;
    }

    /// <summary>
    /// Returns the string representation of the dice outcome.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString()
    {
        string result = $"{Hits} hits";
        if (IsCriticalGlitch)
            result += " [CRITICAL GLITCH!]";
        else if (IsGlitch)
            result += " [Glitch]";
        return result;
    }
}