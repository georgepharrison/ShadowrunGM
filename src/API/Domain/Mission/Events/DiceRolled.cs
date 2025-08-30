using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Mission.Events;

/// <summary>
/// Domain event raised when dice are rolled in a session.
/// </summary>
public sealed record DiceRolled : DomainEvent
{
    /// <summary>
    /// Initializes a new instance of the DiceRolled event.
    /// </summary>
    /// <param name="sessionId">The ID of the session.</param>
    /// <param name="pool">The dice pool that was rolled.</param>
    /// <param name="outcome">The outcome of the roll.</param>
    public DiceRolled(SessionId sessionId, DicePool pool, DiceOutcome outcome)
    {
        SessionId = sessionId;
        Pool = pool;
        Outcome = outcome;
    }

    /// <summary>
    /// Gets the ID of the session.
    /// </summary>
    public SessionId SessionId { get; }

    /// <summary>
    /// Gets the dice pool that was rolled.
    /// </summary>
    public DicePool Pool { get; }

    /// <summary>
    /// Gets the outcome of the roll.
    /// </summary>
    public DiceOutcome Outcome { get; }
}