using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Mission.Events;

/// <summary>
/// Domain event raised when a game session is paused.
/// </summary>
public sealed record SessionPaused : DomainEvent
{
    /// <summary>
    /// Initializes a new instance of the SessionPaused event.
    /// </summary>
    /// <param name="sessionId">The ID of the paused session.</param>
    public SessionPaused(SessionId sessionId)
    {
        SessionId = sessionId;
    }

    /// <summary>
    /// Gets the ID of the paused session.
    /// </summary>
    public SessionId SessionId { get; }
}