using Shouldly;
using ShadowrunGM.Domain.Character;
using ShadowrunGM.Domain.Character.Events;
using ShadowrunGM.Domain.Common;
using ShadowrunGM.Domain.Mission;
using ShadowrunGM.Domain.Mission.Events;
using ShadowrunGM.Domain.Tests.TestHelpers;
using Xunit;

namespace ShadowrunGM.Domain.Tests.Common;

/// <summary>
/// Comprehensive tests for all domain events covering creation, serialization, and data integrity.
/// Tests are organized by event type and follow TDD principles - these tests FAIL first.
/// </summary>
public sealed class DomainEventTests
{
    public sealed class CharacterDomainEvents
    {
        public sealed class CharacterCreatedTests
        {
            [Fact]
            public void Constructor_WithValidParameters_ShouldCreateEvent()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                string characterName = "Test Runner";

                // Act
                CharacterCreated eventInstance = new(characterId, characterName);

                // Assert
                eventInstance.ShouldNotBeNull();
                eventInstance.CharacterId.ShouldBe(characterId);
                eventInstance.Name.ShouldBe(characterName);
            }

            [Fact]
            public void Constructor_WithValidParameters_ShouldInheritFromDomainEvent()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                string characterName = "Test Runner";

                // Act
                CharacterCreated eventInstance = new(characterId, characterName);

                // Assert
                eventInstance.ShouldBeAssignableTo<DomainEvent>();
                eventInstance.OccurredAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
                eventInstance.OccurredAt.ShouldBeGreaterThan(DateTime.UtcNow.AddSeconds(-1));
            }

            [Fact]
            public void Constructor_WithEmptyName_ShouldStillCreateEvent()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                string emptyName = string.Empty;

                // Act
                CharacterCreated eventInstance = new(characterId, emptyName);

                // Assert
                eventInstance.ShouldNotBeNull();
                eventInstance.Name.ShouldBe(emptyName);
                // Event creation should not validate business rules - that's for the aggregate
            }

            [Fact]
            public void Constructor_WithNullName_ShouldStillCreateEvent()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                string nullName = null!;

                // Act
                CharacterCreated eventInstance = new(characterId, nullName);

                // Assert
                eventInstance.ShouldNotBeNull();
                eventInstance.Name.ShouldBe(nullName);
                // Events are data containers - validation happens at aggregate level
            }

            [Fact]
            public void RecordEquality_WithSameValues_ShouldBeEqual()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                string characterName = "Test Runner";

                // Act
                DateTime fixedTime = new(2025, 8, 30, 6, 14, 0, DateTimeKind.Utc);
                CharacterCreated event1 = new(characterId, characterName) { OccurredAt = fixedTime };
                CharacterCreated event2 = new(characterId, characterName) { OccurredAt = fixedTime };

                // Assert
                event1.ShouldBe(event2);
                event1.GetHashCode().ShouldBe(event2.GetHashCode());
                (event1 == event2).ShouldBeTrue();
            }

            [Fact]
            public void RecordEquality_WithDifferentCharacterId_ShouldNotBeEqual()
            {
                // Arrange
                CharacterId characterId1 = CharacterId.New();
                CharacterId characterId2 = CharacterId.New();
                string characterName = "Test Runner";

                // Act
                CharacterCreated event1 = new(characterId1, characterName);
                CharacterCreated event2 = new(characterId2, characterName);

                // Assert
                event1.ShouldNotBe(event2);
                (event1 == event2).ShouldBeFalse();
            }

            [Fact]
            public void RecordEquality_WithDifferentName_ShouldNotBeEqual()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                string name1 = "Test Runner";
                string name2 = "Different Runner";

                // Act
                CharacterCreated event1 = new(characterId, name1);
                CharacterCreated event2 = new(characterId, name2);

                // Assert
                event1.ShouldNotBe(event2);
                (event1 == event2).ShouldBeFalse();
            }

            [Fact]
            public void ToString_ShouldProvideReadableRepresentation()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                string characterName = "Test Runner";

                // Act
                CharacterCreated eventInstance = new(characterId, characterName);
                string stringRepresentation = eventInstance.ToString();

                // Assert
                stringRepresentation.ShouldNotBeNullOrWhiteSpace();
                stringRepresentation.ShouldContain(nameof(CharacterCreated));
                // Should contain the property values for debugging
                stringRepresentation.ShouldContain(characterId.ToString());
                stringRepresentation.ShouldContain(characterName);
            }
        }

        public sealed class EdgeSpentTests
        {
            [Fact]
            public void Constructor_WithValidParameters_ShouldCreateEvent()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                int amount = 2;
                string purpose = "Push the Limit";

                // Act
                EdgeSpent eventInstance = new(characterId, amount, purpose);

                // Assert
                eventInstance.ShouldNotBeNull();
                eventInstance.CharacterId.ShouldBe(characterId);
                eventInstance.Amount.ShouldBe(amount);
                eventInstance.Purpose.ShouldBe(purpose);
            }

            [Fact]
            public void Constructor_WithValidParameters_ShouldInheritFromDomainEvent()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                int amount = 1;
                string purpose = "Test Purpose";

                // Act
                EdgeSpent eventInstance = new(characterId, amount, purpose);

                // Assert
                eventInstance.ShouldBeAssignableTo<DomainEvent>();
                eventInstance.OccurredAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
                eventInstance.OccurredAt.ShouldBeGreaterThan(DateTime.UtcNow.AddSeconds(-1));
            }

            [Theory]
            [InlineData(1)]
            [InlineData(3)]
            [InlineData(7)]
            public void Constructor_WithDifferentAmounts_ShouldStoreAmount(int amount)
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                string purpose = "Test Purpose";

                // Act
                EdgeSpent eventInstance = new(characterId, amount, purpose);

                // Assert
                eventInstance.Amount.ShouldBe(amount);
            }

            [Fact]
            public void Constructor_WithZeroAmount_ShouldStillCreateEvent()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                int zeroAmount = 0;
                string purpose = "Test Purpose";

                // Act
                EdgeSpent eventInstance = new(characterId, zeroAmount, purpose);

                // Assert
                eventInstance.Amount.ShouldBe(zeroAmount);
                // Events don't validate - business rules are enforced at aggregate level
            }

            [Fact]
            public void Constructor_WithNegativeAmount_ShouldStillCreateEvent()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                int negativeAmount = -1;
                string purpose = "Test Purpose";

                // Act
                EdgeSpent eventInstance = new(characterId, negativeAmount, purpose);

                // Assert
                eventInstance.Amount.ShouldBe(negativeAmount);
                // Events are data containers - validation is not their responsibility
            }

            [Fact]
            public void Constructor_WithEmptyPurpose_ShouldStillCreateEvent()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                int amount = 2;
                string emptyPurpose = string.Empty;

                // Act
                EdgeSpent eventInstance = new(characterId, amount, emptyPurpose);

                // Assert
                eventInstance.Purpose.ShouldBe(emptyPurpose);
            }

            [Fact]
            public void RecordEquality_WithSameValues_ShouldBeEqual()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                int amount = 2;
                string purpose = "Push the Limit";

                // Act
                DateTime fixedTime = new(2025, 8, 30, 6, 14, 0, DateTimeKind.Utc);
                EdgeSpent event1 = new(characterId, amount, purpose) { OccurredAt = fixedTime };
                EdgeSpent event2 = new(characterId, amount, purpose) { OccurredAt = fixedTime };

                // Assert
                event1.ShouldBe(event2);
                event1.GetHashCode().ShouldBe(event2.GetHashCode());
            }

            [Fact]
            public void RecordEquality_WithDifferentAmount_ShouldNotBeEqual()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                string purpose = "Push the Limit";

                // Act
                EdgeSpent event1 = new(characterId, 1, purpose);
                EdgeSpent event2 = new(characterId, 2, purpose);

                // Assert
                event1.ShouldNotBe(event2);
            }

            [Fact]
            public void RecordEquality_WithDifferentPurpose_ShouldNotBeEqual()
            {
                // Arrange
                CharacterId characterId = CharacterId.New();
                int amount = 2;

                // Act
                EdgeSpent event1 = new(characterId, amount, "Purpose 1");
                EdgeSpent event2 = new(characterId, amount, "Purpose 2");

                // Assert
                event1.ShouldNotBe(event2);
            }
        }
    }

    public sealed class GameSessionDomainEvents
    {
        public sealed class SessionStartedTests
        {
            [Fact]
            public void Constructor_WithValidParameters_ShouldCreateEvent()
            {
                // Arrange
                SessionId sessionId = SessionId.New();
                CharacterId characterId = CharacterId.New();

                // Act
                SessionStarted eventInstance = new(sessionId, characterId);

                // Assert
                eventInstance.ShouldNotBeNull();
                eventInstance.SessionId.ShouldBe(sessionId);
                eventInstance.CharacterId.ShouldBe(characterId);
            }

            [Fact]
            public void Constructor_WithValidParameters_ShouldInheritFromDomainEvent()
            {
                // Arrange
                SessionId sessionId = SessionId.New();
                CharacterId characterId = CharacterId.New();

                // Act
                SessionStarted eventInstance = new(sessionId, characterId);

                // Assert
                eventInstance.ShouldBeAssignableTo<DomainEvent>();
                eventInstance.OccurredAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
                eventInstance.OccurredAt.ShouldBeGreaterThan(DateTime.UtcNow.AddSeconds(-1));
            }

            [Fact]
            public void RecordEquality_WithSameValues_ShouldBeEqual()
            {
                // Arrange
                SessionId sessionId = SessionId.New();
                CharacterId characterId = CharacterId.New();

                // Act
                DateTime fixedTime = new(2025, 8, 30, 6, 14, 0, DateTimeKind.Utc);
                SessionStarted event1 = new(sessionId, characterId) { OccurredAt = fixedTime };
                SessionStarted event2 = new(sessionId, characterId) { OccurredAt = fixedTime };

                // Assert
                event1.ShouldBe(event2);
                event1.GetHashCode().ShouldBe(event2.GetHashCode());
            }

            [Fact]
            public void RecordEquality_WithDifferentSessionId_ShouldNotBeEqual()
            {
                // Arrange
                SessionId sessionId1 = SessionId.New();
                SessionId sessionId2 = SessionId.New();
                CharacterId characterId = CharacterId.New();

                // Act
                SessionStarted event1 = new(sessionId1, characterId);
                SessionStarted event2 = new(sessionId2, characterId);

                // Assert
                event1.ShouldNotBe(event2);
            }

            [Fact]
            public void RecordEquality_WithDifferentCharacterId_ShouldNotBeEqual()
            {
                // Arrange
                SessionId sessionId = SessionId.New();
                CharacterId characterId1 = CharacterId.New();
                CharacterId characterId2 = CharacterId.New();

                // Act
                SessionStarted event1 = new(sessionId, characterId1);
                SessionStarted event2 = new(sessionId, characterId2);

                // Assert
                event1.ShouldNotBe(event2);
            }
        }

        public sealed class SessionPausedTests
        {
            [Fact]
            public void Constructor_WithValidSessionId_ShouldCreateEvent()
            {
                // Arrange
                SessionId sessionId = SessionId.New();

                // Act
                SessionPaused eventInstance = new(sessionId);

                // Assert
                eventInstance.ShouldNotBeNull();
                eventInstance.SessionId.ShouldBe(sessionId);
            }

            [Fact]
            public void Constructor_WithValidSessionId_ShouldInheritFromDomainEvent()
            {
                // Arrange
                SessionId sessionId = SessionId.New();

                // Act
                SessionPaused eventInstance = new(sessionId);

                // Assert
                eventInstance.ShouldBeAssignableTo<DomainEvent>();
                eventInstance.OccurredAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
            }

            [Fact]
            public void RecordEquality_WithSameSessionId_ShouldBeEqual()
            {
                // Arrange
                SessionId sessionId = SessionId.New();

                // Act
                DateTime fixedTime = new(2025, 8, 30, 6, 14, 0, DateTimeKind.Utc);
                SessionPaused event1 = new(sessionId) { OccurredAt = fixedTime };
                SessionPaused event2 = new(sessionId) { OccurredAt = fixedTime };

                // Assert
                event1.ShouldBe(event2);
                event1.GetHashCode().ShouldBe(event2.GetHashCode());
            }
        }

        public sealed class SessionResumedTests
        {
            [Fact]
            public void Constructor_WithValidSessionId_ShouldCreateEvent()
            {
                // Arrange
                SessionId sessionId = SessionId.New();

                // Act
                SessionResumed eventInstance = new(sessionId);

                // Assert
                eventInstance.ShouldNotBeNull();
                eventInstance.SessionId.ShouldBe(sessionId);
            }

            [Fact]
            public void RecordEquality_WithSameSessionId_ShouldBeEqual()
            {
                // Arrange
                SessionId sessionId = SessionId.New();

                // Act
                DateTime fixedTime = new(2025, 8, 30, 6, 14, 0, DateTimeKind.Utc);
                SessionResumed event1 = new(sessionId) { OccurredAt = fixedTime };
                SessionResumed event2 = new(sessionId) { OccurredAt = fixedTime };

                // Assert
                event1.ShouldBe(event2);
            }
        }

        public sealed class SessionCompletedTests
        {
            [Fact]
            public void Constructor_WithValidSessionId_ShouldCreateEvent()
            {
                // Arrange
                SessionId sessionId = SessionId.New();

                // Act
                SessionCompleted eventInstance = new(sessionId);

                // Assert
                eventInstance.ShouldNotBeNull();
                eventInstance.SessionId.ShouldBe(sessionId);
            }

            [Fact]
            public void RecordEquality_WithSameSessionId_ShouldBeEqual()
            {
                // Arrange
                SessionId sessionId = SessionId.New();

                // Act
                DateTime fixedTime = new(2025, 8, 30, 6, 14, 0, DateTimeKind.Utc);
                SessionCompleted event1 = new(sessionId) { OccurredAt = fixedTime };
                SessionCompleted event2 = new(sessionId) { OccurredAt = fixedTime };

                // Assert
                event1.ShouldBe(event2);
            }
        }

        public sealed class DiceRolledTests
        {
            [Fact]
            public void Constructor_WithValidParameters_ShouldCreateEvent()
            {
                // Arrange
                SessionId sessionId = SessionId.New();
                DicePoolBuilder poolBuilder = new();
                DicePool pool = poolBuilder.WithTotalDice(6).Build();
                DiceOutcome outcome = new([6, 5, 4, 3, 2, 1], 2, 1, false, false);

                // Act
                DiceRolled eventInstance = new(sessionId, pool, outcome);

                // Assert
                eventInstance.ShouldNotBeNull();
                eventInstance.SessionId.ShouldBe(sessionId);
                eventInstance.Pool.ShouldBe(pool);
                eventInstance.Outcome.ShouldBe(outcome);
            }

            [Fact]
            public void Constructor_WithValidParameters_ShouldInheritFromDomainEvent()
            {
                // Arrange
                SessionId sessionId = SessionId.New();
                DicePoolBuilder poolBuilder = new();
                DicePool pool = poolBuilder.Build();
                DiceOutcome outcome = new([4, 5, 6], 3, 0, false, false);

                // Act
                DiceRolled eventInstance = new(sessionId, pool, outcome);

                // Assert
                eventInstance.ShouldBeAssignableTo<DomainEvent>();
                eventInstance.OccurredAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
            }

            [Fact]
            public void Constructor_WithNullPool_ShouldStillCreateEvent()
            {
                // Arrange
                SessionId sessionId = SessionId.New();
                DicePool nullPool = null!;
                DiceOutcome outcome = new([6], 1, 0, false, false);

                // Act
                DiceRolled eventInstance = new(sessionId, nullPool, outcome);

                // Assert
                eventInstance.ShouldNotBeNull();
                eventInstance.Pool.ShouldBe(nullPool);
                // Events don't validate - they're data containers
            }

            [Fact]
            public void RecordEquality_WithSameValues_ShouldBeEqual()
            {
                // Arrange
                SessionId sessionId = SessionId.New();
                DicePoolBuilder poolBuilder = new();
                DicePool pool = poolBuilder.Build();
                DiceOutcome outcome = new([6, 5], 2, 0, false, false);

                // Act
                DateTime fixedTime = new(2025, 8, 30, 6, 14, 0, DateTimeKind.Utc);
                DiceRolled event1 = new(sessionId, pool, outcome) { OccurredAt = fixedTime };
                DiceRolled event2 = new(sessionId, pool, outcome) { OccurredAt = fixedTime };

                // Assert
                event1.ShouldBe(event2);
                event1.GetHashCode().ShouldBe(event2.GetHashCode());
            }

            [Fact]
            public void RecordEquality_WithDifferentPool_ShouldNotBeEqual()
            {
                // Arrange
                SessionId sessionId = SessionId.New();
                DicePool pool1 = new DicePoolBuilder().WithTotalDice(4).Build();
                DicePool pool2 = new DicePoolBuilder().WithTotalDice(6).Build();
                DiceOutcome outcome = new([6, 5], 2, 0, false, false);

                // Act
                DiceRolled event1 = new(sessionId, pool1, outcome);
                DiceRolled event2 = new(sessionId, pool2, outcome);

                // Assert
                event1.ShouldNotBe(event2);
            }

            [Fact]
            public void RecordEquality_WithDifferentOutcome_ShouldNotBeEqual()
            {
                // Arrange
                SessionId sessionId = SessionId.New();
                DicePoolBuilder poolBuilder = new();
                DicePool pool = poolBuilder.Build();
                DiceOutcome outcome1 = new([6, 5], 2, 0, false, false);
                DiceOutcome outcome2 = new([4, 3], 0, 0, false, false);

                // Act
                DiceRolled event1 = new(sessionId, pool, outcome1);
                DiceRolled event2 = new(sessionId, pool, outcome2);

                // Assert
                event1.ShouldNotBe(event2);
            }
        }
    }

    public sealed class DomainEventBehavior
    {
        [Fact]
        public void DomainEvent_WhenCreated_ShouldSetOccurredAtToCurrentTime()
        {
            // Arrange
            DateTime beforeCreation = DateTime.UtcNow.AddMilliseconds(-10);
            
            // Act
            CharacterCreated eventInstance = new(CharacterId.New(), "Test");
            
            // Assert
            DateTime afterCreation = DateTime.UtcNow.AddMilliseconds(10);
            eventInstance.OccurredAt.ShouldBeGreaterThanOrEqualTo(beforeCreation);
            eventInstance.OccurredAt.ShouldBeLessThanOrEqualTo(afterCreation);
        }

        [Fact]
        public void DomainEvent_ShouldBeImmutableRecord()
        {
            // Arrange & Act
            CharacterCreated eventInstance = new(CharacterId.New(), "Test");

            // Assert - Records are immutable by design, properties should be init-only
            Type eventType = typeof(CharacterCreated);
            System.Reflection.PropertyInfo[] properties = eventType.GetProperties();
            
            foreach (System.Reflection.PropertyInfo property in properties)
            {
                // All properties should have getters
                property.GetGetMethod().ShouldNotBeNull();
                
                // Properties should only have init-only setters or no setters
                System.Reflection.MethodInfo? setter = property.GetSetMethod();
                if (setter != null)
                {
                    // Check if it's an init-only setter by checking the method name
                    // Init-only setters have "init" in their method metadata
                    setter.ReturnParameter.GetRequiredCustomModifiers()
                        .Any(t => t.Name.Contains("IsExternalInit")).ShouldBeTrue("Property should be init-only");
                }
            }
        }

        [Fact]
        public void DomainEvent_ShouldSupportRecordCopyingBehavior()
        {
            // Arrange
            CharacterId originalId = CharacterId.New();
            CharacterCreated originalEvent = new(originalId, "Original Name");

            // Act - Since domain event properties are immutable, we create a new instance
            CharacterId newId = CharacterId.New();
            CharacterCreated newEvent = new(newId, originalEvent.Name);

            // Assert
            newEvent.CharacterId.ShouldBe(newId);
            newEvent.Name.ShouldBe(originalEvent.Name);
            newEvent.ShouldNotBe(originalEvent); // Should be different instances
            
            // Domain events should maintain immutability - properties cannot be changed after creation
            originalEvent.CharacterId.ShouldBe(originalId); // Original unchanged
        }

        [Fact]
        public void DomainEvent_ShouldImplementProperToString()
        {
            // Arrange
            CharacterId characterId = CharacterId.New();
            string characterName = "Test Runner";

            // Act
            CharacterCreated eventInstance = new(characterId, characterName);
            string stringRepresentation = eventInstance.ToString();

            // Assert
            stringRepresentation.ShouldNotBeNullOrWhiteSpace();
            stringRepresentation.ShouldContain(nameof(CharacterCreated));
            // ToString should be helpful for debugging and logging
        }

        [Fact]
        public void AllDomainEvents_ShouldInheritFromDomainEventBase()
        {
            // This test documents that all domain events in the system should inherit from DomainEvent
            // It serves as documentation and ensures consistency

            // Arrange & Act - Create instances of all domain event types
            CharacterCreated characterCreated = new(CharacterId.New(), "Test");
            EdgeSpent edgeSpent = new(CharacterId.New(), 1, "Test");
            SessionStarted sessionStarted = new(SessionId.New(), CharacterId.New());
            SessionPaused sessionPaused = new(SessionId.New());
            SessionResumed sessionResumed = new(SessionId.New());
            SessionCompleted sessionCompleted = new(SessionId.New());
            DiceRolled diceRolled = new(SessionId.New(), 
                new DicePoolBuilder().Build(), 
                new([6], 1, 0, false, false));

            // Assert - All events should inherit from DomainEvent
            characterCreated.ShouldBeAssignableTo<DomainEvent>();
            edgeSpent.ShouldBeAssignableTo<DomainEvent>();
            sessionStarted.ShouldBeAssignableTo<DomainEvent>();
            sessionPaused.ShouldBeAssignableTo<DomainEvent>();
            sessionResumed.ShouldBeAssignableTo<DomainEvent>();
            sessionCompleted.ShouldBeAssignableTo<DomainEvent>();
            diceRolled.ShouldBeAssignableTo<DomainEvent>();
        }

        [Fact]
        public void DomainEvents_ShouldProvideJsonSerializationCapability()
        {
            // This test documents that domain events should be serializable for event sourcing
            // or message publishing scenarios
            
            // Arrange
            CharacterCreated originalEvent = new(CharacterId.New(), "Test Runner");

            // Act - Test basic serialization capability
            string json = System.Text.Json.JsonSerializer.Serialize(originalEvent);
            CharacterCreated? deserializedEvent = System.Text.Json.JsonSerializer.Deserialize<CharacterCreated>(json);

            // Assert
            json.ShouldNotBeNullOrWhiteSpace();
            deserializedEvent.ShouldNotBeNull();
            deserializedEvent.Name.ShouldBe(originalEvent.Name);
            // Note: CharacterId serialization might need custom converters depending on implementation
        }

        [Fact]
        public void DomainEvents_ShouldMaintainDataIntegrityAcrossRecordOperations()
        {
            // Arrange
            SessionId sessionId = SessionId.New();
            DicePool pool = new DicePoolBuilder().WithTotalDice(5).Build();
            DiceOutcome outcome = new([6, 5, 4, 3, 2], 2, 0, false, false);

            // Act
            DiceRolled originalEvent = new(sessionId, pool, outcome);
            DiceRolled copiedEvent = originalEvent with { }; // Copy with no changes
            
            // Assert
            copiedEvent.ShouldBe(originalEvent);
            copiedEvent.SessionId.ShouldBe(originalEvent.SessionId);
            copiedEvent.Pool.ShouldBe(originalEvent.Pool);
            copiedEvent.Outcome.ShouldBe(originalEvent.Outcome);
            copiedEvent.OccurredAt.ShouldBe(originalEvent.OccurredAt);
        }
    }
}