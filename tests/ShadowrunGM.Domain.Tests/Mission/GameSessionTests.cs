using Shouldly;
using ShadowrunGM.ApiSdk.Common.Results;
using ShadowrunGM.Domain.Character;
using ShadowrunGM.Domain.Mission;
using ShadowrunGM.Domain.Mission.Events;
using ShadowrunGM.Domain.Tests.TestHelpers;
using Xunit;

namespace ShadowrunGM.Domain.Tests.Mission;

/// <summary>
/// Comprehensive tests for the GameSession aggregate covering all business rules and operations.
/// Tests are organized by functionality and follow TDD principles - these tests FAIL first.
/// </summary>
public sealed class GameSessionTests
{
    public sealed class SessionLifecycle
    {
        [Fact]
        public void Start_WithValidCharacterId_ShouldCreateActiveSession()
        {
            // Arrange
            CharacterId characterId = CharacterId.New();

            // Act
            Result<GameSession> result = GameSession.Start(characterId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out GameSession? session).ShouldBeTrue();
            session.ShouldNotBeNull();
            
            session.CharacterId.ShouldBe(characterId);
            session.State.ShouldBe(SessionState.Active);
            session.StartedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
            session.StartedAt.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-1));
            session.CompletedAt.ShouldBeNull();
        }

        [Fact]
        public void Start_WithValidCharacterId_ShouldRaiseSessionStartedEvent()
        {
            // Arrange
            CharacterId characterId = CharacterId.New();

            // Act
            Result<GameSession> result = GameSession.Start(characterId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out GameSession? session).ShouldBeTrue();
            session.ShouldNotBeNull();
            
            session.DomainEvents.Count.ShouldBe(1);
            session.DomainEvents.First().ShouldBeOfType<SessionStarted>();
            
            SessionStarted startedEvent = (SessionStarted)session.DomainEvents.First();
            startedEvent.SessionId.ShouldBe(session.Id);
            startedEvent.CharacterId.ShouldBe(characterId);
        }

        [Fact]
        public void Start_WithValidCharacterId_ShouldInitializeEmptyCollections()
        {
            // Arrange
            CharacterId characterId = CharacterId.New();

            // Act
            Result<GameSession> result = GameSession.Start(characterId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out GameSession? session).ShouldBeTrue();
            session.ShouldNotBeNull();
            
            session.Messages.Count.ShouldBe(0);
            session.Rolls.Count.ShouldBe(0);
        }

        [Fact]
        public void Pause_WhenActive_ShouldChangeStateToPaused()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();

            // Act
            Result result = session.Pause();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            session.State.ShouldBe(SessionState.Paused);
        }

        [Fact]
        public void Pause_WhenActive_ShouldRaiseSessionPausedEvent()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();
            session.ClearDomainEvents(); // Clear creation events

            // Act
            Result result = session.Pause();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            session.DomainEvents.Count.ShouldBe(1);
            session.DomainEvents.First().ShouldBeOfType<SessionPaused>();
            
            SessionPaused pausedEvent = (SessionPaused)session.DomainEvents.First();
            pausedEvent.SessionId.ShouldBe(session.Id);
        }

        [Fact]
        public void Pause_WhenAlreadyPaused_ShouldReturnFailure()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();
            session.Pause();

            // Act
            Result result = session.Pause();

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("active session");
        }

        [Fact]
        public void Pause_WhenCompleted_ShouldReturnFailure()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();
            session.Complete();

            // Act
            Result result = session.Pause();

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("active session");
        }

        [Fact]
        public void Resume_WhenPaused_ShouldChangeStateToActive()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();
            session.Pause();

            // Act
            Result result = session.Resume();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            session.State.ShouldBe(SessionState.Active);
        }

        [Fact]
        public void Resume_WhenPaused_ShouldRaiseSessionResumedEvent()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();
            session.Pause();
            session.ClearDomainEvents(); // Clear previous events

            // Act
            Result result = session.Resume();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            session.DomainEvents.Count.ShouldBe(1);
            session.DomainEvents.First().ShouldBeOfType<SessionResumed>();
            
            SessionResumed resumedEvent = (SessionResumed)session.DomainEvents.First();
            resumedEvent.SessionId.ShouldBe(session.Id);
        }

        [Fact]
        public void Resume_WhenActive_ShouldReturnFailure()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();

            // Act
            Result result = session.Resume();

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("paused session");
        }

        [Fact]
        public void Resume_WhenCompleted_ShouldReturnFailure()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();
            session.Complete();

            // Act
            Result result = session.Resume();

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("paused session");
        }

        [Fact]
        public void Complete_WhenActive_ShouldChangeStateToCompleted()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();

            // Act
            Result result = session.Complete();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            session.State.ShouldBe(SessionState.Completed);
            session.CompletedAt.ShouldNotBeNull();
            session.CompletedAt.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        }

        [Fact]
        public void Complete_WhenPaused_ShouldChangeStateToCompleted()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();
            session.Pause();

            // Act
            Result result = session.Complete();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            session.State.ShouldBe(SessionState.Completed);
            session.CompletedAt.ShouldNotBeNull();
        }

        [Fact]
        public void Complete_WhenActive_ShouldRaiseSessionCompletedEvent()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();
            session.ClearDomainEvents(); // Clear creation events

            // Act
            Result result = session.Complete();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            session.DomainEvents.Count.ShouldBe(1);
            session.DomainEvents.First().ShouldBeOfType<SessionCompleted>();
            
            SessionCompleted completedEvent = (SessionCompleted)session.DomainEvents.First();
            completedEvent.SessionId.ShouldBe(session.Id);
        }

        [Fact]
        public void Complete_WhenAlreadyCompleted_ShouldReturnFailure()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();
            session.Complete();

            // Act
            Result result = session.Complete();

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("already completed");
        }
    }

    public sealed class MessageManagement
    {
        [Fact]
        public void AddMessage_WhenActiveWithValidMessage_ShouldSucceed()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            
            ChatMessageBuilder messageBuilder = new();
            ChatMessage message = messageBuilder
                .WithSender("Player")
                .WithContent("I search the room for clues.")
                .WithType(MessageType.Player)
                .Build();

            // Act
            Result result = session.AddMessage(message);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            session.Messages.Count.ShouldBe(1);
            session.Messages[0].ShouldBe(message);
        }

        [Fact]
        public void AddMessage_WhenPaused_ShouldReturnFailure()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            session.Pause();
            
            ChatMessageBuilder messageBuilder = new();
            ChatMessage message = messageBuilder.Build();

            // Act
            Result result = session.AddMessage(message);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("inactive session");
            session.Messages.Count.ShouldBe(0);
        }

        [Fact]
        public void AddMessage_WhenCompleted_ShouldReturnFailure()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            session.Complete();
            
            ChatMessageBuilder messageBuilder = new();
            ChatMessage message = messageBuilder.Build();

            // Act
            Result result = session.AddMessage(message);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("inactive session");
            session.Messages.Count.ShouldBe(0);
        }

        [Fact]
        public void AddMessage_WithNullMessage_ShouldReturnFailure()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            ChatMessage nullMessage = null!;

            // Act
            Result result = session.AddMessage(nullMessage);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("cannot be null");
            session.Messages.Count.ShouldBe(0);
        }

        [Fact]
        public void AddMessage_MultipleMessages_ShouldMaintainOrder()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            
            ChatMessage message1 = new ChatMessageBuilder()
                .WithContent("First message")
                .Build();
            ChatMessage message2 = new ChatMessageBuilder()
                .WithContent("Second message")
                .Build();
            ChatMessage message3 = new ChatMessageBuilder()
                .WithContent("Third message")
                .Build();

            // Act
            session.AddMessage(message1);
            session.AddMessage(message2);
            session.AddMessage(message3);

            // Assert
            session.Messages.Count.ShouldBe(3);
            session.Messages[0].Content.ShouldBe("First message");
            session.Messages[1].Content.ShouldBe("Second message");
            session.Messages[2].Content.ShouldBe("Third message");
        }

        [Fact]
        public void AddMessage_DifferentMessageTypes_ShouldAllBeAccepted()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            
            ChatMessage playerMessage = new ChatMessageBuilder()
                .WithType(MessageType.Player)
                .WithContent("Player action")
                .Build();
            ChatMessage gmMessage = new ChatMessageBuilder()
                .WithType(MessageType.GameMaster)
                .WithContent("GM response")
                .Build();
            ChatMessage systemMessage = new ChatMessageBuilder()
                .WithType(MessageType.System)
                .WithContent("System notification")
                .Build();
            ChatMessage narrativeMessage = new ChatMessageBuilder()
                .WithType(MessageType.Narrative)
                .WithContent("Scene description")
                .Build();

            // Act
            session.AddMessage(playerMessage);
            session.AddMessage(gmMessage);
            session.AddMessage(systemMessage);
            session.AddMessage(narrativeMessage);

            // Assert
            session.Messages.Count.ShouldBe(4);
            session.Messages.Count(m => m.Type == MessageType.Player).ShouldBe(1);
            session.Messages.Count(m => m.Type == MessageType.GameMaster).ShouldBe(1);
            session.Messages.Count(m => m.Type == MessageType.System).ShouldBe(1);
            session.Messages.Count(m => m.Type == MessageType.Narrative).ShouldBe(1);
        }
    }

    public sealed class DiceRollManagement
    {
        [Fact]
        public void ResolveDiceRoll_WhenActiveWithValidInputs_ShouldSucceed()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            
            DicePoolBuilder poolBuilder = new();
            DicePool pool = poolBuilder.WithTotalDice(6).Build();
            
            MockDiceService diceService = new();
            DiceOutcome expectedOutcome = new([4, 5, 6, 2, 1, 3], 3, 1, false, false);
            diceService.EnqueueOutcome(expectedOutcome);

            // Act
            Result<DiceOutcome> result = session.ResolveDiceRoll(pool, diceService);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DiceOutcome? outcome).ShouldBeTrue();
            outcome.ShouldBe(expectedOutcome);
            
            session.Rolls.Count.ShouldBe(1);
            session.Rolls[0].Pool.ShouldBe(pool);
            session.Rolls[0].Outcome.ShouldBe(expectedOutcome);
        }

        [Fact]
        public void ResolveDiceRoll_WhenActiveWithValidInputs_ShouldRaiseDiceRolledEvent()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            session.ClearDomainEvents(); // Clear creation events
            
            DicePoolBuilder poolBuilder = new();
            DicePool pool = poolBuilder.Build();
            
            MockDiceService diceService = new();
            DiceOutcome expectedOutcome = new([5, 6, 4, 3, 2, 1], 2, 1, false, false);
            diceService.EnqueueOutcome(expectedOutcome);

            // Act
            Result<DiceOutcome> result = session.ResolveDiceRoll(pool, diceService);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            session.DomainEvents.Count.ShouldBe(1);
            session.DomainEvents.First().ShouldBeOfType<DiceRolled>();
            
            DiceRolled rolledEvent = (DiceRolled)session.DomainEvents.First();
            rolledEvent.SessionId.ShouldBe(session.Id);
            rolledEvent.Pool.ShouldBe(pool);
            rolledEvent.Outcome.ShouldBe(expectedOutcome);
        }

        [Fact]
        public void ResolveDiceRoll_WhenPaused_ShouldReturnFailure()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            session.Pause();
            
            DicePoolBuilder poolBuilder = new();
            DicePool pool = poolBuilder.Build();
            MockDiceService diceService = new();

            // Act
            Result<DiceOutcome> result = session.ResolveDiceRoll(pool, diceService);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("inactive session");
            session.Rolls.Count.ShouldBe(0);
        }

        [Fact]
        public void ResolveDiceRoll_WhenCompleted_ShouldReturnFailure()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            session.Complete();
            
            DicePoolBuilder poolBuilder = new();
            DicePool pool = poolBuilder.Build();
            MockDiceService diceService = new();

            // Act
            Result<DiceOutcome> result = session.ResolveDiceRoll(pool, diceService);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("inactive session");
            session.Rolls.Count.ShouldBe(0);
        }

        [Fact]
        public void ResolveDiceRoll_WithNullPool_ShouldReturnFailure()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            DicePool nullPool = null!;
            MockDiceService diceService = new();

            // Act
            Result<DiceOutcome> result = session.ResolveDiceRoll(nullPool, diceService);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("cannot be null");
            session.Rolls.Count.ShouldBe(0);
        }

        [Fact]
        public void ResolveDiceRoll_WithNullDiceService_ShouldReturnFailure()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            
            DicePoolBuilder poolBuilder = new();
            DicePool pool = poolBuilder.Build();
            IDiceService nullService = null!;

            // Act
            Result<DiceOutcome> result = session.ResolveDiceRoll(pool, nullService);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("cannot be null");
            session.Rolls.Count.ShouldBe(0);
        }

        [Fact]
        public void ResolveDiceRoll_MultipleRolls_ShouldMaintainChronologicalOrder()
        {
            // Arrange
            GameSessionBuilder sessionBuilder = new();
            GameSession session = sessionBuilder.Build();
            
            DicePoolBuilder poolBuilder = new();
            MockDiceService diceService = new();
            
            // Setup different outcomes for tracking
            DiceOutcome outcome1 = new([6, 5, 4, 3, 2, 1], 2, 1, false, false);
            DiceOutcome outcome2 = new([4, 4, 4, 6, 6, 6], 3, 0, false, false);
            DiceOutcome outcome3 = new([1, 1, 1, 2, 2, 2], 0, 3, true, false);
            
            diceService.EnqueueOutcome(outcome1);
            diceService.EnqueueOutcome(outcome2);
            diceService.EnqueueOutcome(outcome3);

            // Act
            DateTime beforeRolls = DateTime.UtcNow.AddSeconds(-1);
            session.ResolveDiceRoll(poolBuilder.Build(), diceService);
            session.ResolveDiceRoll(poolBuilder.Build(), diceService);
            session.ResolveDiceRoll(poolBuilder.Build(), diceService);
            DateTime afterRolls = DateTime.UtcNow.AddSeconds(1);

            // Assert
            session.Rolls.Count.ShouldBe(3);
            session.Rolls[0].Outcome.ShouldBe(outcome1);
            session.Rolls[1].Outcome.ShouldBe(outcome2);
            session.Rolls[2].Outcome.ShouldBe(outcome3);
            
            // Verify timestamps are in chronological order
            session.Rolls[0].RolledAt.ShouldBeGreaterThan(beforeRolls);
            session.Rolls[0].RolledAt.ShouldBeLessThanOrEqualTo(session.Rolls[1].RolledAt);
            session.Rolls[1].RolledAt.ShouldBeLessThanOrEqualTo(session.Rolls[2].RolledAt);
            session.Rolls[2].RolledAt.ShouldBeLessThan(afterRolls);
        }
    }

    public sealed class BusinessRuleValidation
    {
        [Fact]
        public void GameSession_ShouldMaintainAggregateInvariants_StateTransitions()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();

            // Assert - Initial state should be valid
            session.State.ShouldBe(SessionState.Active);
            session.CompletedAt.ShouldBeNull();

            // Act & Assert - Valid state transitions
            session.Pause().IsSuccess.ShouldBeTrue();
            session.State.ShouldBe(SessionState.Paused);

            session.Resume().IsSuccess.ShouldBeTrue();
            session.State.ShouldBe(SessionState.Active);

            session.Complete().IsSuccess.ShouldBeTrue();
            session.State.ShouldBe(SessionState.Completed);
            session.CompletedAt.ShouldNotBeNull();
        }

        [Fact]
        public void GameSession_ShouldMaintainAggregateInvariants_ImmutableCollections()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();

            // Assert - Collections should be read-only
            session.Messages.ShouldBeAssignableTo<IReadOnlyList<ChatMessage>>();
            session.Rolls.ShouldBeAssignableTo<IReadOnlyList<DiceRoll>>();

            // Messages should be read-only - this is enforced by the interface
            // IReadOnlyList<T> does not expose Add/Remove methods
            session.Messages.ShouldBeAssignableTo<IReadOnlyList<ChatMessage>>();
            session.Rolls.ShouldBeAssignableTo<IReadOnlyList<DiceRoll>>();
        }

        [Fact]
        public void GameSession_ShouldMaintainAggregateInvariants_ConsistentTimestamps()
        {
            // Arrange & Act
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();

            // Assert - StartedAt should be set and reasonable
            session.StartedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
            session.StartedAt.ShouldBeGreaterThan(DateTime.UtcNow.AddMinutes(-1));

            // When completed, CompletedAt should be after StartedAt
            session.Complete();
            session.CompletedAt.ShouldNotBeNull();
            session.CompletedAt.Value.ShouldBeGreaterThanOrEqualTo(session.StartedAt);
        }

        [Fact]
        public void GameSession_ShouldMaintainAggregateInvariants_CharacterIdConsistency()
        {
            // Arrange
            CharacterId characterId = CharacterId.New();
            
            // Act
            Result<GameSession> result = GameSession.Start(characterId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out GameSession? session).ShouldBeTrue();
            session.ShouldNotBeNull();
            
            session.CharacterId.ShouldBe(characterId);
            // CharacterId should remain immutable throughout session lifecycle
            
            session.Pause();
            session.CharacterId.ShouldBe(characterId);
            
            session.Resume();
            session.CharacterId.ShouldBe(characterId);
            
            session.Complete();
            session.CharacterId.ShouldBe(characterId);
        }

        [Fact]
        public void GameSession_ShouldMaintainAggregateInvariants_UniqueSessionId()
        {
            // Arrange
            CharacterId characterId = CharacterId.New();
            
            // Act - Create multiple sessions
            Result<GameSession> result1 = GameSession.Start(characterId);
            Result<GameSession> result2 = GameSession.Start(characterId);
            Result<GameSession> result3 = GameSession.Start(characterId);

            // Assert - All sessions should have unique IDs
            result1.IsSuccess.ShouldBeTrue();
            result2.IsSuccess.ShouldBeTrue();
            result3.IsSuccess.ShouldBeTrue();
            
            result1.TryGetValue(out GameSession? session1).ShouldBeTrue();
            result2.TryGetValue(out GameSession? session2).ShouldBeTrue();
            result3.TryGetValue(out GameSession? session3).ShouldBeTrue();
            
            session1.ShouldNotBeNull();
            session2.ShouldNotBeNull();
            session3.ShouldNotBeNull();
            
            session1.Id.ShouldNotBe(session2.Id);
            session1.Id.ShouldNotBe(session3.Id);
            session2.Id.ShouldNotBe(session3.Id);
        }
    }

    public sealed class DomainEventBehavior
    {
        [Fact]
        public void GameSession_WhenStarted_ShouldRaiseSessionStartedEvent()
        {
            // Arrange
            CharacterId characterId = CharacterId.New();

            // Act
            Result<GameSession> result = GameSession.Start(characterId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out GameSession? session).ShouldBeTrue();
            session.ShouldNotBeNull();
            
            session.DomainEvents.Count.ShouldBe(1);
            SessionStarted startedEvent = session.DomainEvents.OfType<SessionStarted>().Single();
            startedEvent.SessionId.ShouldBe(session.Id);
            startedEvent.CharacterId.ShouldBe(characterId);
        }

        [Fact]
        public void GameSession_WhenDiceRolled_ShouldRaiseDiceRolledEvent()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();
            session.ClearDomainEvents();
            
            DicePoolBuilder poolBuilder = new();
            DicePool pool = poolBuilder.Build();
            
            MockDiceService diceService = new();
            DiceOutcome outcome = new([6, 5, 4, 3, 2, 1], 2, 1, false, false);
            diceService.EnqueueOutcome(outcome);

            // Act
            Result<DiceOutcome> result = session.ResolveDiceRoll(pool, diceService);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            session.DomainEvents.Count.ShouldBe(1);
            DiceRolled rolledEvent = session.DomainEvents.OfType<DiceRolled>().Single();
            rolledEvent.SessionId.ShouldBe(session.Id);
            rolledEvent.Pool.ShouldBe(pool);
            rolledEvent.Outcome.ShouldBe(outcome);
        }

        [Fact]
        public void GameSession_StateTransitions_ShouldRaiseAppropriateEvents()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();
            session.ClearDomainEvents();

            // Act & Assert - Pause
            session.Pause();
            session.DomainEvents.Count.ShouldBe(1);
            session.DomainEvents.OfType<SessionPaused>().Single().SessionId.ShouldBe(session.Id);
            
            session.ClearDomainEvents();

            // Act & Assert - Resume
            session.Resume();
            session.DomainEvents.Count.ShouldBe(1);
            session.DomainEvents.OfType<SessionResumed>().Single().SessionId.ShouldBe(session.Id);
            
            session.ClearDomainEvents();

            // Act & Assert - Complete
            session.Complete();
            session.DomainEvents.Count.ShouldBe(1);
            session.DomainEvents.OfType<SessionCompleted>().Single().SessionId.ShouldBe(session.Id);
        }

        [Fact]
        public void GameSession_DomainEvents_ShouldAccumulateUntilCleared()
        {
            // Arrange
            GameSessionBuilder builder = new();
            GameSession session = builder.Build();
            
            DicePoolBuilder poolBuilder = new();
            MockDiceService diceService = new();
            diceService.EnqueueOutcome(3, 1, false, false); // 3 hits, 1 one
            diceService.EnqueueOutcome(1, 2, true, false);  // 1 hit, 2 ones, glitch

            // Act
            session.ResolveDiceRoll(poolBuilder.Build(), diceService);
            session.ResolveDiceRoll(poolBuilder.Build(), diceService);
            session.Pause();

            // Assert
            session.DomainEvents.Count.ShouldBe(4); // 1 creation + 2 dice rolls + 1 pause
            session.DomainEvents.OfType<SessionStarted>().Count().ShouldBe(1);
            session.DomainEvents.OfType<DiceRolled>().Count().ShouldBe(2);
            session.DomainEvents.OfType<SessionPaused>().Count().ShouldBe(1);
        }
    }
}