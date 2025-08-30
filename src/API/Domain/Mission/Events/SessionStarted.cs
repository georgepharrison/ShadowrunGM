using ShadowrunGM.Domain.Character;
using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Mission.Events;

/// <summary>
/// Domain event raised when a game session is started.
/// </summary>
public sealed record SessionStarted : DomainEvent
{
    /// <summary>
    /// Initializes a new instance of the SessionStarted event.
    /// </summary>
    /// <param name="sessionId">The ID of the started session.</param>
    /// <param name="characterId">The ID of the character in the session.</param>
    public SessionStarted(SessionId sessionId, CharacterId characterId)
    {
        SessionId = sessionId;
        CharacterId = characterId;
    }

    /// <summary>
    /// Gets the ID of the started session.
    /// </summary>
    public SessionId SessionId { get; }

    /// <summary>
    /// Gets the ID of the character in the session.
    /// </summary>
    public CharacterId CharacterId { get; }
}