namespace ShadowrunGM.Domain.Mission;

/// <summary>
/// Strongly-typed identifier for a GameSession.
/// </summary>
public readonly record struct SessionId
{
    private readonly Guid _value;

    private SessionId(Guid value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the underlying value of the identifier.
    /// </summary>
    public Guid Value => _value;

    /// <summary>
    /// Creates a new unique SessionId.
    /// </summary>
    /// <returns>A new SessionId.</returns>
    public static SessionId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a SessionId from an existing Guid value.
    /// </summary>
    /// <param name="value">The Guid value.</param>
    /// <returns>A SessionId.</returns>
    public static SessionId From(Guid value) => new(value);

    /// <summary>
    /// Implicitly converts a SessionId to a Guid.
    /// </summary>
    /// <param name="id">The SessionId to convert.</param>
    public static implicit operator Guid(SessionId id) => id._value;

    /// <summary>
    /// Returns the string representation of the SessionId.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString() => _value.ToString();
}