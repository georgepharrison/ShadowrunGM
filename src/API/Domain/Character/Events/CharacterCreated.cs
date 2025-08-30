using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Character.Events;

/// <summary>
/// Domain event raised when a new character is created.
/// </summary>
public sealed record CharacterCreated : DomainEvent
{
    /// <summary>
    /// Initializes a new instance of the CharacterCreated event.
    /// </summary>
    /// <param name="characterId">The ID of the created character.</param>
    /// <param name="name">The name of the character.</param>
    public CharacterCreated(CharacterId characterId, string name)
    {
        CharacterId = characterId;
        Name = name;
    }

    /// <summary>
    /// Gets the ID of the created character.
    /// </summary>
    public CharacterId CharacterId { get; }

    /// <summary>
    /// Gets the name of the character.
    /// </summary>
    public string Name { get; }
}