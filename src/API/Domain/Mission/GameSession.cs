using ShadowrunGM.ApiSdk.Common.Results;
using ShadowrunGM.Domain.Character;
using ShadowrunGM.Domain.Common;
using ShadowrunGM.Domain.Mission.Events;

namespace ShadowrunGM.Domain.Mission;

/// <summary>
/// Aggregate root representing a game session.
/// </summary>
public sealed class GameSession : AggregateRoot
{
    private readonly List<ChatMessage> _messages = [];
    private readonly List<DiceRoll> _rolls = [];

    private GameSession()
    {
    }

    /// <summary>
    /// Gets the unique identifier for this session.
    /// </summary>
    public SessionId Id { get; private init; }

    /// <summary>
    /// Gets the character identifier for this session.
    /// </summary>
    public CharacterId CharacterId { get; private init; }

    /// <summary>
    /// Gets the current state of the session.
    /// </summary>
    public SessionState State { get; private set; }

    /// <summary>
    /// Gets the date and time when the session started.
    /// </summary>
    public DateTime StartedAt { get; private init; }

    /// <summary>
    /// Gets the date and time when the session was completed.
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Gets the chat messages in this session.
    /// </summary>
    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

    /// <summary>
    /// Gets the dice rolls in this session.
    /// </summary>
    public IReadOnlyList<DiceRoll> Rolls => _rolls.AsReadOnly();

    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    /// <returns>The entity identifier.</returns>
    public override object GetId() => Id;

    /// <summary>
    /// Starts a new game session.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <returns>A Result containing the new session or an error.</returns>
    public static Result<GameSession> Start(CharacterId characterId)
    {
        GameSession session = new()
        {
            Id = SessionId.New(),
            CharacterId = characterId,
            State = SessionState.Active,
            StartedAt = DateTime.UtcNow
        };

        session.RaiseDomainEvent(new SessionStarted(session.Id, characterId));
        return Result.Success(session);
    }

    /// <summary>
    /// Adds a chat message to the session.
    /// </summary>
    /// <param name="message">The chat message to add.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public Result AddMessage(ChatMessage message)
    {
        if (State != SessionState.Active)
            return Result.Failure("Cannot add messages to an inactive session.");

        if (message == null)
            return Result.Failure("Message cannot be null.");

        _messages.Add(message);
        return Result.Success();
    }

    /// <summary>
    /// Resolves a dice roll in the session.
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <param name="diceService">The dice service to use for rolling.</param>
    /// <returns>A Result containing the dice outcome or an error.</returns>
    public Result<DiceOutcome> ResolveDiceRoll(DicePool pool, IDiceService diceService)
    {
        if (State != SessionState.Active)
            return Result.Failure<DiceOutcome>("Cannot roll dice in an inactive session.");

        if (pool == null)
            return Result.Failure<DiceOutcome>("Dice pool cannot be null.");

        if (diceService == null)
            return Result.Failure<DiceOutcome>("Dice service cannot be null.");

        DiceOutcome outcome = diceService.Roll(pool);
        _rolls.Add(new DiceRoll(pool, outcome, DateTime.UtcNow));

        RaiseDomainEvent(new DiceRolled(Id, pool, outcome));
        return Result.Success(outcome);
    }

    /// <summary>
    /// Pauses the session.
    /// </summary>
    /// <returns>A Result indicating success or failure.</returns>
    public Result Pause()
    {
        if (State != SessionState.Active)
            return Result.Failure("Can only pause an active session.");

        State = SessionState.Paused;
        RaiseDomainEvent(new SessionPaused(Id));
        return Result.Success();
    }

    /// <summary>
    /// Resumes a paused session.
    /// </summary>
    /// <returns>A Result indicating success or failure.</returns>
    public Result Resume()
    {
        if (State != SessionState.Paused)
            return Result.Failure("Can only resume a paused session.");

        State = SessionState.Active;
        RaiseDomainEvent(new SessionResumed(Id));
        return Result.Success();
    }

    /// <summary>
    /// Completes the session.
    /// </summary>
    /// <returns>A Result indicating success or failure.</returns>
    public Result Complete()
    {
        if (State == SessionState.Completed)
            return Result.Failure("Session is already completed.");

        State = SessionState.Completed;
        CompletedAt = DateTime.UtcNow;
        RaiseDomainEvent(new SessionCompleted(Id));
        return Result.Success();
    }
}

/// <summary>
/// Represents the state of a game session.
/// </summary>
public enum SessionState
{
    /// <summary>
    /// The session is active and in progress.
    /// </summary>
    Active,

    /// <summary>
    /// The session is paused.
    /// </summary>
    Paused,

    /// <summary>
    /// The session has been completed.
    /// </summary>
    Completed
}