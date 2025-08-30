using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Mission.Events;

/// <summary>
/// Domain event raised when a game session is resumed.
/// </summary>
public sealed record SessionResumed : DomainEvent
{
    /// <summary>
    /// Initializes a new instance of the SessionResumed event.
    /// </summary>
    /// <param name="sessionId">The ID of the resumed session.</param>
    public SessionResumed(SessionId sessionId)
    {
        SessionId = sessionId;
    }

    /// <summary>
    /// Gets the ID of the resumed session.
    /// </summary>
    public SessionId SessionId { get; }
}