using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Character.Events;

/// <summary>
/// Event raised when edge is spent.
/// </summary>
public sealed record EdgeSpent(CharacterId CharacterId, int Amount, string Purpose) : DomainEvent;