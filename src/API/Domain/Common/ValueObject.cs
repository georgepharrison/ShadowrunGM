namespace ShadowrunGM.Domain.Common;

/// <summary>
/// Base class for all domain value objects.
/// </summary>
public abstract class ValueObject
{
    /// <summary>
    /// Gets the atomic values that define this value object.
    /// </summary>
    /// <returns>The collection of atomic values.</returns>
    protected abstract IEnumerable<object?> GetAtomicValues();

    /// <summary>
    /// Determines whether this value object is equal to another value object.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the value objects are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
            return false;

        ValueObject other = (ValueObject)obj;

        using IEnumerator<object?> thisValues = GetAtomicValues().GetEnumerator();
        using IEnumerator<object?> otherValues = other.GetAtomicValues().GetEnumerator();

        while (thisValues.MoveNext() && otherValues.MoveNext())
        {
            if (thisValues.Current is null ^ otherValues.Current is null)
                return false;

            if (thisValues.Current?.Equals(otherValues.Current) == false)
                return false;
        }

        return !thisValues.MoveNext() && !otherValues.MoveNext();
    }

    /// <summary>
    /// Returns the hash code for this value object.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Where(x => x != null)
            .Aggregate(17, (current, value) => current * 31 + value!.GetHashCode());
    }

    /// <summary>
    /// Equality operator for value objects.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for value objects.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}