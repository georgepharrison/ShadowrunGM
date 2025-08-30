namespace ShadowrunGM.Domain.Common;

/// <summary>
/// Base class for all domain entities.
/// </summary>
public abstract class Entity
{
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    public abstract object GetId();

    /// <summary>
    /// Determines whether this entity is equal to another entity.
    /// </summary>
    /// <param name="obj">The object to compare with.</param>
    /// <returns>True if the entities are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return GetId().Equals(other.GetId());
    }

    /// <summary>
    /// Returns the hash code for this entity.
    /// </summary>
    /// <returns>The hash code.</returns>
    public override int GetHashCode()
    {
        return GetId().GetHashCode();
    }

    /// <summary>
    /// Equality operator for entities.
    /// </summary>
    public static bool operator ==(Entity? left, Entity? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator for entities.
    /// </summary>
    public static bool operator !=(Entity? left, Entity? right)
    {
        return !(left == right);
    }
}