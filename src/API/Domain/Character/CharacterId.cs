namespace ShadowrunGM.Domain.Character;

/// <summary>
/// Strongly-typed identifier for a Character.
/// </summary>
public readonly record struct CharacterId
{
    private readonly Guid _value;

    private CharacterId(Guid value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the underlying value of the identifier.
    /// </summary>
    public Guid Value => _value;

    /// <summary>
    /// Creates a new unique CharacterId.
    /// </summary>
    /// <returns>A new CharacterId.</returns>
    public static CharacterId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a CharacterId from an existing Guid value.
    /// </summary>
    /// <param name="value">The Guid value.</param>
    /// <returns>A CharacterId.</returns>
    public static CharacterId From(Guid value) => new(value);

    /// <summary>
    /// Implicitly converts a CharacterId to a Guid.
    /// </summary>
    /// <param name="id">The CharacterId to convert.</param>
    public static implicit operator Guid(CharacterId id) => id._value;

    /// <summary>
    /// Returns the string representation of the CharacterId.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString() => _value.ToString();
}