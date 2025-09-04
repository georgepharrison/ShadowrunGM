using FlowRight.Core.Results;
using ShadowrunGM.Domain.Character;
using ShadowrunGM.Domain.Mission;

namespace ShadowrunGM.Domain.Tests.TestHelpers;

/// <summary>
/// Builder pattern for creating GameSession instances in tests.
/// Provides default values and fluent API for customization.
/// </summary>
public sealed class GameSessionBuilder
{
    private CharacterId _characterId = CharacterId.New();

    /// <summary>
    /// Sets the character ID for the session.
    /// </summary>
    /// <param name="characterId">The character ID to use.</param>
    /// <returns>This builder instance.</returns>
    public GameSessionBuilder WithCharacterId(CharacterId characterId)
    {
        _characterId = characterId;
        return this;
    }

    /// <summary>
    /// Builds a GameSession with the configured values.
    /// </summary>
    /// <returns>A new GameSession instance.</returns>
    public GameSession Build()
    {
        Result<GameSession> result = GameSession.Start(_characterId);
        if (!result.IsSuccess)
            throw new InvalidOperationException($"Failed to create GameSession: {result.Error}");

        if (!result.TryGetValue(out GameSession? session) || session == null)
            throw new InvalidOperationException("GameSession creation returned null");

        return session;
    }

    /// <summary>
    /// Creates a GameSession builder with default values.
    /// </summary>
    /// <returns>A new GameSessionBuilder instance.</returns>
    public static GameSessionBuilder Default() => new();
}

/// <summary>
/// Builder pattern for creating ChatMessage instances in tests.
/// </summary>
public sealed class ChatMessageBuilder
{
    private string _sender = "Test Player";
    private string _content = "Test message content";
    private MessageType _type = MessageType.Player;

    /// <summary>
    /// Sets the sender for the message.
    /// </summary>
    /// <param name="sender">The message sender.</param>
    /// <returns>This builder instance.</returns>
    public ChatMessageBuilder WithSender(string sender)
    {
        _sender = sender;
        return this;
    }

    /// <summary>
    /// Sets the content for the message.
    /// </summary>
    /// <param name="content">The message content.</param>
    /// <returns>This builder instance.</returns>
    public ChatMessageBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    /// <summary>
    /// Sets the message type.
    /// </summary>
    /// <param name="type">The message type.</param>
    /// <returns>This builder instance.</returns>
    public ChatMessageBuilder WithType(MessageType type)
    {
        _type = type;
        return this;
    }

    /// <summary>
    /// Builds a ChatMessage with the configured values.
    /// </summary>
    /// <returns>A new ChatMessage instance.</returns>
    public ChatMessage Build()
    {
        Result<ChatMessage> result = ChatMessage.Create(_sender, _content, _type);
        if (!result.IsSuccess)
            throw new InvalidOperationException($"Failed to create ChatMessage: {result.Error}");

        if (!result.TryGetValue(out ChatMessage? message) || message == null)
            throw new InvalidOperationException("ChatMessage creation returned null");

        return message;
    }
}

/// <summary>
/// Builder pattern for creating DicePool instances in tests.
/// </summary>
public sealed class DicePoolBuilder
{
    private int _attribute = 3;
    private int _skill = 3;
    private int _modifiers = 0;
    private int _edgeBonus = 0;
    private int _limit = 0;

    /// <summary>
    /// Sets the attribute dice in the pool.
    /// </summary>
    /// <param name="attribute">The attribute dice.</param>
    /// <returns>This builder instance.</returns>
    public DicePoolBuilder WithAttribute(int attribute)
    {
        _attribute = attribute;
        return this;
    }

    /// <summary>
    /// Sets the skill dice in the pool.
    /// </summary>
    /// <param name="skill">The skill dice.</param>
    /// <returns>This builder instance.</returns>
    public DicePoolBuilder WithSkill(int skill)
    {
        _skill = skill;
        return this;
    }

    /// <summary>
    /// Sets the modifier dice (can be negative).
    /// </summary>
    /// <param name="modifiers">The modifier dice.</param>
    /// <returns>This builder instance.</returns>
    public DicePoolBuilder WithModifiers(int modifiers)
    {
        _modifiers = modifiers;
        return this;
    }

    /// <summary>
    /// Sets the Edge bonus dice.
    /// </summary>
    /// <param name="edgeBonus">The Edge bonus dice.</param>
    /// <returns>This builder instance.</returns>
    public DicePoolBuilder WithEdgeBonus(int edgeBonus)
    {
        _edgeBonus = edgeBonus;
        return this;
    }

    /// <summary>
    /// Sets the dice limit.
    /// </summary>
    /// <param name="limit">The dice limit.</param>
    /// <returns>This builder instance.</returns>
    public DicePoolBuilder WithLimit(int limit)
    {
        _limit = limit;
        return this;
    }

    /// <summary>
    /// Sets the total dice to a specific value by adjusting attribute/skill.
    /// </summary>
    /// <param name="totalDice">The desired total dice count.</param>
    /// <returns>This builder instance.</returns>
    public DicePoolBuilder WithTotalDice(int totalDice)
    {
        // Split evenly between attribute and skill
        _attribute = totalDice / 2;
        _skill = totalDice - _attribute;
        _modifiers = 0;
        _edgeBonus = 0;
        return this;
    }

    /// <summary>
    /// Builds a DicePool with the configured values.
    /// </summary>
    /// <returns>A new DicePool instance.</returns>
    public DicePool Build()
    {
        Result<DicePool> result = DicePool.Create(_attribute, _skill, _modifiers, _edgeBonus, _limit);
        if (!result.IsSuccess)
            throw new InvalidOperationException($"Failed to create DicePool: {result.Error}");

        if (!result.TryGetValue(out DicePool? pool) || pool == null)
            throw new InvalidOperationException("DicePool creation returned null");

        return pool;
    }
}

/// <summary>
/// Mock implementation of IDiceService for testing.
/// Provides predictable dice roll outcomes.
/// </summary>
public sealed class MockDiceService : IDiceService
{
    private readonly Queue<DiceOutcome> _predefinedOutcomes = new();

    /// <summary>
    /// Adds a predefined outcome to the queue.
    /// </summary>
    /// <param name="outcome">The outcome to return on next roll.</param>
    public void EnqueueOutcome(DiceOutcome outcome)
    {
        _predefinedOutcomes.Enqueue(outcome);
    }

    /// <summary>
    /// Adds a predefined outcome to the queue based on simple values.
    /// </summary>
    /// <param name="hits">Number of hits to return.</param>
    /// <param name="ones">Number of ones to return.</param>
    /// <param name="isGlitch">Whether the roll is a glitch.</param>
    /// <param name="isCriticalGlitch">Whether the roll is a critical glitch.</param>
    public void EnqueueOutcome(int hits, int ones = 0, bool isGlitch = false, bool isCriticalGlitch = false)
    {
        DiceOutcome outcome = DiceOutcome.Create([1, 2, 3, 4, 5, 6], hits, ones, isGlitch, isCriticalGlitch);
        _predefinedOutcomes.Enqueue(outcome);
    }

    /// <summary>
    /// Rolls a dice pool and returns the next queued outcome.
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <returns>The next predefined outcome, or a default outcome if queue is empty.</returns>
    public DiceOutcome Roll(DicePool pool)
    {
        if (_predefinedOutcomes.Count > 0)
        {
            return _predefinedOutcomes.Dequeue();
        }

        // Default outcome: half the dice succeed, no glitches
        int hits = pool.TotalDice / 2;
        return new DiceOutcome([1, 2, 3, 4, 5, 6], hits, 0, false, false);
    }

    /// <summary>
    /// Rolls with exploding sixes and returns the next queued outcome.
    /// </summary>
    /// <param name="pool">The dice pool to roll.</param>
    /// <returns>The next predefined outcome.</returns>
    public DiceOutcome RollWithExplodingSixes(DicePool pool)
    {
        return Roll(pool); // For testing, same behavior as regular roll
    }

    /// <summary>
    /// Rerolls failures and returns the next queued outcome.
    /// </summary>
    /// <param name="originalOutcome">The original outcome.</param>
    /// <returns>The next predefined outcome.</returns>
    public DiceOutcome RerollFailures(DiceOutcome originalOutcome)
    {
        return _predefinedOutcomes.Count > 0 ? _predefinedOutcomes.Dequeue() : originalOutcome;
    }
}