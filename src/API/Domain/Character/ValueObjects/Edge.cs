using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Character.ValueObjects;

/// <summary>
/// Represents a character's Edge resource with current and maximum values.
/// </summary>
public sealed class Edge : ValueObject
{
    private Edge(int current, int max)
    {
        if (max < 1)
            throw new ArgumentException("Maximum Edge must be at least 1.", nameof(max));
        
        if (current < 0)
            throw new ArgumentException("Current Edge cannot be negative.", nameof(current));
        
        if (current > max)
            throw new ArgumentException("Current Edge cannot exceed maximum Edge.", nameof(current));

        Current = current;
        Max = max;
    }

    /// <summary>
    /// Gets the current Edge value.
    /// </summary>
    public int Current { get; }

    /// <summary>
    /// Gets the maximum Edge value.
    /// </summary>
    public int Max { get; }

    /// <summary>
    /// Gets a value indicating whether the character has Edge available to spend.
    /// </summary>
    public bool HasEdge => Current > 0;

    /// <summary>
    /// Gets a value indicating whether Edge is at maximum.
    /// </summary>
    public bool IsAtMax => Current == Max;

    /// <summary>
    /// Creates a new Edge value object.
    /// </summary>
    /// <param name="startingEdge">The starting Edge value (both current and max).</param>
    /// <returns>A new Edge instance.</returns>
    public static Edge Create(int startingEdge)
    {
        return new Edge(startingEdge, startingEdge);
    }

    /// <summary>
    /// Creates a new Edge value object with specified current and max values.
    /// </summary>
    /// <param name="current">The current Edge value.</param>
    /// <param name="max">The maximum Edge value.</param>
    /// <returns>A new Edge instance.</returns>
    public static Edge CreateWithValues(int current, int max)
    {
        return new Edge(current, max);
    }

    /// <summary>
    /// Spends the specified amount of Edge.
    /// </summary>
    /// <param name="amount">The amount of Edge to spend.</param>
    /// <returns>A Result containing the new Edge state or an error.</returns>
    public Result<Edge> Spend(int amount)
    {
        if (amount <= 0)
            return Result<Edge>.Failure("Spend amount must be positive.");

        if (amount > Current)
            return Result<Edge>.Failure($"Cannot spend {amount} Edge. Only {Current} available.");

        return Result<Edge>.Success(new Edge(Current - amount, Max));
    }

    /// <summary>
    /// Regains the specified amount of Edge.
    /// </summary>
    /// <param name="amount">The amount of Edge to regain.</param>
    /// <returns>A Result containing the new Edge state or an error.</returns>
    public Result<Edge> Regain(int amount)
    {
        if (amount <= 0)
            return Result<Edge>.Failure("Regain amount must be positive.");

        int newCurrent = Math.Min(Current + amount, Max);
        return Result<Edge>.Success(new Edge(newCurrent, Max));
    }

    /// <summary>
    /// Refreshes Edge to maximum.
    /// </summary>
    /// <returns>A new Edge instance at maximum value.</returns>
    public Edge Refresh()
    {
        return new Edge(Max, Max);
    }

    /// <summary>
    /// Burns Edge permanently, reducing both current and maximum.
    /// </summary>
    /// <param name="amount">The amount of Edge to burn.</param>
    /// <returns>A Result containing the new Edge state or an error.</returns>
    public Result<Edge> Burn(int amount = 1)
    {
        if (amount <= 0)
            return Result<Edge>.Failure("Burn amount must be positive.");

        if (amount > Max)
            return Result<Edge>.Failure($"Cannot burn {amount} Edge. Maximum is only {Max}.");

        int newMax = Max - amount;
        if (newMax < 1)
            return Result<Edge>.Failure("Cannot burn Edge below 1.");

        int newCurrent = Math.Min(Current, newMax);
        return Result<Edge>.Success(new Edge(newCurrent, newMax));
    }

    /// <summary>
    /// Gets the atomic values that define this value object.
    /// </summary>
    /// <returns>The collection of atomic values.</returns>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Current;
        yield return Max;
    }

    /// <summary>
    /// Returns the string representation of the Edge.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString() => $"{Current}/{Max}";
}