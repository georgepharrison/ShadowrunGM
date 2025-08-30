using ShadowrunGM.Domain.Character;
using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Mission.Events;

/// <summary>
/// Event raised when a game session is started.
/// </summary>
public sealed record SessionStarted(SessionId SessionId, CharacterId CharacterId) : DomainEvent;