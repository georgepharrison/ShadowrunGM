using ShadowrunGM.Domain.Common;
using System.Text.Json.Serialization;

namespace ShadowrunGM.Domain.Character.Events;

/// <summary>
/// Event raised when a character is created.
/// </summary>
public sealed record CharacterCreated(CharacterId CharacterId, string Name) : DomainEvent;