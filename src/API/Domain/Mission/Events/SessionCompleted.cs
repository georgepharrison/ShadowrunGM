using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Mission.Events;

/// <summary>
/// Domain event raised when a game session is completed.
/// </summary>
public sealed record SessionCompleted : DomainEvent
{
    /// <summary>
    /// Initializes a new instance of the SessionCompleted event.
    /// </summary>
    /// <param name="sessionId">The ID of the completed session.</param>
    public SessionCompleted(SessionId sessionId)
    {
        SessionId = sessionId;
    }

    /// <summary>
    /// Gets the ID of the completed session.
    /// </summary>
    public SessionId SessionId { get; }
}