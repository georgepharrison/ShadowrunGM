using ShadowrunGM.Domain.Mission;
using System.Reflection;

namespace ShadowrunGM.Domain.Tests.Mission.ValueObjects;

/// <summary>
/// Comprehensive test suite for DiceRoll value object behavior.
/// Tests roll recording, immutability, and value object semantics.
/// </summary>
public sealed class DiceRollTests
{
    public sealed class Constructor
    {
        [Fact]
        public void Constructor_WithValidData_ShouldSetAllProperties()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(5, 3, -1, 2, 6);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 5, 3, 1, 4, 6, 2, 5, 3];
            DiceOutcome outcome = new(rolls, 5, 1, false, false);
            DateTime rolledAt = new(2023, 6, 15, 14, 30, 45, DateTimeKind.Utc);

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Pool.ShouldBe(pool);
            diceRoll.Outcome.ShouldBe(outcome);
            diceRoll.RolledAt.ShouldBe(rolledAt);
        }

        [Fact]
        public void Constructor_WithMinimalPool_ShouldWork()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(1); // Single die
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [4];
            DiceOutcome outcome = new(rolls, 0, 0, false, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Pool.ShouldBe(pool);
            diceRoll.Pool.TotalDice.ShouldBe(1);
            diceRoll.Outcome.ShouldBe(outcome);
            diceRoll.RolledAt.ShouldBe(rolledAt);
        }

        [Fact]
        public void Constructor_WithComplexPool_ShouldWork()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(8, 6, -3, 4, 7);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 6, 5, 5, 4, 3, 2, 1, 6, 5, 4, 3, 2, 1, 6]; // 15 dice total
            DiceOutcome outcome = new(rolls, 7, 2, false, false);
            DateTime rolledAt = new(2023, 12, 1, 9, 15, 30, DateTimeKind.Utc);

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Pool.ShouldBe(pool);
            diceRoll.Pool.TotalDice.ShouldBe(15); // 8 + 6 - 3 + 4 = 15
            diceRoll.Pool.HasLimit.ShouldBeTrue();
            diceRoll.Pool.Limit.ShouldBe(7);
            diceRoll.Outcome.ShouldBe(outcome);
            diceRoll.Outcome.Hits.ShouldBe(7);
            diceRoll.RolledAt.ShouldBe(rolledAt);
        }

        [Fact]
        public void Constructor_WithSuccessfulOutcome_ShouldWork()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(6, 4);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 5, 6, 3, 2, 5, 4, 1, 6, 3]; // 4 hits (6,5,6,5,6)
            DiceOutcome outcome = new(rolls, 4, 1, false, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Outcome.IsSuccess.ShouldBeTrue();
            diceRoll.Outcome.IsFailure.ShouldBeFalse();
            diceRoll.Outcome.Hits.ShouldBe(4);
            diceRoll.Outcome.Ones.ShouldBe(1);
        }

        [Fact]
        public void Constructor_WithFailedOutcome_ShouldWork()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(4, 2);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [2, 3, 4, 1, 3, 2]; // No hits
            DiceOutcome outcome = new(rolls, 0, 1, false, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Outcome.IsSuccess.ShouldBeFalse();
            diceRoll.Outcome.IsFailure.ShouldBeTrue();
            diceRoll.Outcome.Hits.ShouldBe(0);
            diceRoll.Outcome.IsGlitch.ShouldBeFalse();
            diceRoll.Outcome.IsCriticalGlitch.ShouldBeFalse();
        }

        [Fact]
        public void Constructor_WithGlitchOutcome_ShouldWork()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(4);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 1, 1, 1]; // 1 hit, 3 ones = glitch but not critical
            DiceOutcome outcome = new(rolls, 1, 3, true, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Outcome.IsSuccess.ShouldBeTrue(); // Has hits, so success despite glitch
            diceRoll.Outcome.IsGlitch.ShouldBeTrue();
            diceRoll.Outcome.IsCriticalGlitch.ShouldBeFalse();
        }

        [Fact]
        public void Constructor_WithCriticalGlitchOutcome_ShouldWork()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(4);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [1, 1, 1, 2]; // No hits, 3 ones = critical glitch
            DiceOutcome outcome = new(rolls, 0, 3, true, true);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Outcome.IsSuccess.ShouldBeFalse();
            diceRoll.Outcome.IsFailure.ShouldBeTrue();
            diceRoll.Outcome.IsGlitch.ShouldBeTrue();
            diceRoll.Outcome.IsCriticalGlitch.ShouldBeTrue();
        }

        [Fact]
        public void Constructor_WithEdgePool_ShouldWork()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(4, 3, 0, 5, 0); // Edge ignoring limits
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 6, 5, 4, 3, 2, 1, 5, 6, 4, 3, 2]; // 12 dice, 5 hits
            DiceOutcome outcome = new(rolls, 5, 1, false, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Pool.TotalDice.ShouldBe(12); // 4 + 3 + 5 = 12
            diceRoll.Pool.EdgeBonus.ShouldBe(5);
            diceRoll.Pool.IgnoresLimit.ShouldBeTrue();
            diceRoll.Outcome.Hits.ShouldBe(5);
        }

        [Fact]
        public void Constructor_WithNullPool_ShouldAcceptNull()
        {
            // This tests current behavior - in production we might want to guard against this
            
            // Arrange
            DicePool nullPool = null!;
            IReadOnlyList<int> rolls = [6, 5];
            DiceOutcome outcome = new(rolls, 2, 0, false, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act & Assert - Constructor currently accepts null
            Should.NotThrow(() => new DiceRoll(nullPool, outcome, rolledAt));
        }

        [Fact]
        public void Constructor_WithNullOutcome_ShouldAcceptNull()
        {
            // This tests current behavior - in production we might want to guard against this
            
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(3);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            DiceOutcome nullOutcome = null!;
            DateTime rolledAt = DateTime.UtcNow;

            // Act & Assert - Constructor currently accepts null
            Should.NotThrow(() => new DiceRoll(pool!, nullOutcome, rolledAt));
        }
    }

    public sealed class TimestampHandling
    {
        [Fact]
        public void Constructor_WithUtcTimestamp_ShouldPreserveKind()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(3);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 3, 4];
            DiceOutcome outcome = new(rolls, 1, 0, false, false);
            DateTime utcTime = new(2023, 8, 15, 10, 30, 45, DateTimeKind.Utc);

            // Act
            DiceRoll diceRoll = new(pool!, outcome, utcTime);

            // Assert
            diceRoll.RolledAt.ShouldBe(utcTime);
            diceRoll.RolledAt.Kind.ShouldBe(DateTimeKind.Utc);
        }

        [Fact]
        public void Constructor_WithLocalTimestamp_ShouldPreserveKind()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(3);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [5, 2, 1];
            DiceOutcome outcome = new(rolls, 1, 1, false, false);
            DateTime localTime = new(2023, 8, 15, 10, 30, 45, DateTimeKind.Local);

            // Act
            DiceRoll diceRoll = new(pool!, outcome, localTime);

            // Assert
            diceRoll.RolledAt.ShouldBe(localTime);
            diceRoll.RolledAt.Kind.ShouldBe(DateTimeKind.Local);
        }

        [Fact]
        public void Constructor_WithUnspecifiedTimestamp_ShouldPreserveKind()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(2);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [3, 4];
            DiceOutcome outcome = new(rolls, 0, 0, false, false);
            DateTime unspecifiedTime = new(2023, 8, 15, 10, 30, 45, DateTimeKind.Unspecified);

            // Act
            DiceRoll diceRoll = new(pool!, outcome, unspecifiedTime);

            // Assert
            diceRoll.RolledAt.ShouldBe(unspecifiedTime);
            diceRoll.RolledAt.Kind.ShouldBe(DateTimeKind.Unspecified);
        }

        [Fact]
        public void Constructor_WithPreciseTimestamp_ShouldPreservePrecision()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(1);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6];
            DiceOutcome outcome = new(rolls, 1, 0, false, false);
            DateTime preciseTime = new(2023, 8, 15, 10, 30, 45, 123, DateTimeKind.Utc);

            // Act
            DiceRoll diceRoll = new(pool!, outcome, preciseTime);

            // Assert
            diceRoll.RolledAt.ShouldBe(preciseTime);
            diceRoll.RolledAt.Millisecond.ShouldBe(123);
        }
    }

    public sealed class ValueObjectBehavior
    {
        [Fact]
        public void TwoDiceRolls_WithSameValues_ShouldBeEqual()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(4, 3, -1);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 5, 3, 2, 4, 1];
            DiceOutcome outcome = new(rolls, 2, 1, false, false);
            DateTime timestamp = new(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);

            DiceRoll first = new(pool!, outcome, timestamp);
            DiceRoll second = new(pool!, outcome, timestamp);

            // Act & Assert
            first.ShouldBe(second);
            first.GetHashCode().ShouldBe(second.GetHashCode());
        }

        [Fact]
        public void TwoDiceRolls_WithDifferentPools_ShouldNotBeEqual()
        {
            // Arrange
            Result<DicePool> firstPoolResult = DicePool.Create(4, 3);
            Result<DicePool> secondPoolResult = DicePool.Create(5, 3); // Different attribute
            firstPoolResult.TryGetValue(out DicePool? firstPool).ShouldBeTrue();
            secondPoolResult.TryGetValue(out DicePool? secondPool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 5, 3, 2, 4, 1];
            DiceOutcome outcome = new(rolls, 2, 1, false, false);
            DateTime timestamp = new(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);

            DiceRoll first = new(firstPool!, outcome, timestamp);
            DiceRoll second = new(secondPool!, outcome, timestamp);

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void TwoDiceRolls_WithDifferentOutcomes_ShouldNotBeEqual()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(4, 3);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> firstRolls = [6, 5, 3, 2, 4, 1];
            IReadOnlyList<int> secondRolls = [6, 5, 3, 2, 4, 2]; // Different last roll
            DiceOutcome firstOutcome = new(firstRolls, 2, 1, false, false);
            DiceOutcome secondOutcome = new(secondRolls, 2, 0, false, false); // Different ones count
            DateTime timestamp = new(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);

            DiceRoll first = new(pool!, firstOutcome, timestamp);
            DiceRoll second = new(pool!, secondOutcome, timestamp);

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void TwoDiceRolls_WithDifferentTimestamps_ShouldNotBeEqual()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(4, 3);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 5, 3, 2, 4, 1];
            DiceOutcome outcome = new(rolls, 2, 1, false, false);
            DateTime firstTimestamp = new(2023, 6, 15, 14, 30, 0, DateTimeKind.Utc);
            DateTime secondTimestamp = new(2023, 6, 15, 14, 30, 1, DateTimeKind.Utc); // 1 second later

            DiceRoll first = new(pool!, outcome, firstTimestamp);
            DiceRoll second = new(pool!, outcome, secondTimestamp);

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void DiceRoll_ShouldBeImmutable()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(3, 2);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 4, 3, 2, 1];
            DiceOutcome outcome = new(rolls, 1, 1, false, false);
            DateTime timestamp = DateTime.UtcNow;
            
            DiceRoll diceRoll = new(pool!, outcome, timestamp);

            // Act & Assert - Value object should have no public setters
            typeof(DiceRoll).GetProperty("Pool")?.SetMethod.ShouldBeNull();
            typeof(DiceRoll).GetProperty("Outcome")?.SetMethod.ShouldBeNull();
            typeof(DiceRoll).GetProperty("RolledAt")?.SetMethod.ShouldBeNull();
        }

        [Fact]
        public void DiceRoll_WithNullComparison_ShouldNotBeEqual()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(3);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [5];
            DiceOutcome outcome = new(rolls, 1, 0, false, false);
            DiceRoll diceRoll = new(pool!, outcome, DateTime.UtcNow);

            // Act & Assert
            diceRoll.ShouldNotBe(null);
            diceRoll.Equals(null).ShouldBeFalse();
        }

        [Fact]
        public void DiceRoll_WithDifferentType_ShouldNotBeEqual()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(3);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [4];
            DiceOutcome outcome = new(rolls, 0, 0, false, false);
            DiceRoll diceRoll = new(pool!, outcome, DateTime.UtcNow);
            object differentType = "not a dice roll";

            // Act & Assert
            diceRoll.Equals(differentType).ShouldBeFalse();
        }
    }

    public sealed class PropertyAccess
    {
        [Fact]
        public void Properties_ShouldReturnExpectedReferences()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(5, 3, -1, 2);
            poolResult.TryGetValue(out DicePool? originalPool).ShouldBeTrue();
            
            IReadOnlyList<int> originalRolls = [6, 5, 4, 3, 2, 1, 6, 5, 4];
            DiceOutcome originalOutcome = new(originalRolls, 4, 1, false, false);
            DateTime originalTimestamp = new(2023, 9, 10, 16, 45, 30, DateTimeKind.Utc);

            DiceRoll diceRoll = new(originalPool!, originalOutcome, originalTimestamp);

            // Act & Assert - Properties should return the exact same references
            diceRoll.Pool.ShouldBeSameAs(originalPool);
            diceRoll.Outcome.ShouldBeSameAs(originalOutcome);
            diceRoll.RolledAt.ShouldBe(originalTimestamp);
        }

        [Fact]
        public void Properties_ShouldBeReadOnly()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(3);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [2];
            DiceOutcome outcome = new(rolls, 0, 0, false, false);
            DiceRoll diceRoll = new(pool!, outcome, DateTime.UtcNow);

            // Act & Assert - All properties should be read-only
            PropertyInfo poolProperty = typeof(DiceRoll).GetProperty("Pool")!;
            PropertyInfo outcomeProperty = typeof(DiceRoll).GetProperty("Outcome")!;
            PropertyInfo timestampProperty = typeof(DiceRoll).GetProperty("RolledAt")!;

            poolProperty.CanRead.ShouldBeTrue();
            poolProperty.CanWrite.ShouldBeFalse();
            
            outcomeProperty.CanRead.ShouldBeTrue();
            outcomeProperty.CanWrite.ShouldBeFalse();
            
            timestampProperty.CanRead.ShouldBeTrue();
            timestampProperty.CanWrite.ShouldBeFalse();
        }
    }

    public sealed class RealWorldScenarios
    {
        [Fact]
        public void DiceRoll_ForSimpleSkillTest_ShouldWork()
        {
            // Arrange - Simple skill test: Attribute 4 + Skill 3 = 7 dice
            Result<DicePool> poolResult = DicePool.Create(attribute: 4, skill: 3);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 5, 4, 3, 2, 1, 6]; // 3 hits (6, 5, 6)
            DiceOutcome outcome = new(rolls, 3, 1, false, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Pool.TotalDice.ShouldBe(7);
            diceRoll.Pool.Attribute.ShouldBe(4);
            diceRoll.Pool.Skill.ShouldBe(3);
            diceRoll.Outcome.IsSuccess.ShouldBeTrue();
            diceRoll.Outcome.Hits.ShouldBe(3);
        }

        [Fact]
        public void DiceRoll_ForOpposedTest_ShouldWork()
        {
            // Arrange - Opposed test with modifiers: Attribute 5 + Skill 4 - 2 (modifiers) = 7 dice
            Result<DicePool> poolResult = DicePool.Create(attribute: 5, skill: 4, modifiers: -2);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 6, 5, 3, 2, 4, 1]; // 3 hits
            DiceOutcome outcome = new(rolls, 3, 1, false, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Pool.TotalDice.ShouldBe(7);
            diceRoll.Pool.Modifiers.ShouldBe(-2);
            diceRoll.Outcome.Hits.ShouldBe(3);
        }

        [Fact]
        public void DiceRoll_ForEdgeEnhancedTest_ShouldWork()
        {
            // Arrange - Edge-enhanced test: Attribute 4 + Skill 3 + 3 Edge = 10 dice, no limit
            Result<DicePool> poolResult = DicePool.Create(attribute: 4, skill: 3, edgeBonus: 3, limit: 0);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 6, 6, 5, 5, 4, 3, 2, 1, 6]; // 6 hits
            DiceOutcome outcome = new(rolls, 6, 1, false, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Pool.TotalDice.ShouldBe(10);
            diceRoll.Pool.EdgeBonus.ShouldBe(3);
            diceRoll.Pool.IgnoresLimit.ShouldBeTrue();
            diceRoll.Outcome.Hits.ShouldBe(6);
        }

        [Fact]
        public void DiceRoll_ForLimitedTest_ShouldWork()
        {
            // Arrange - Test with limit: 8 dice, limit 5
            Result<DicePool> poolResult = DicePool.Create(attribute: 5, skill: 3, limit: 5);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 6, 6, 6, 6, 6, 5, 4]; // 7 potential hits but limited to 5
            // In actual gameplay, the outcome would respect the limit, but for this test we record all hits
            DiceOutcome outcome = new(rolls, 7, 0, false, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Pool.TotalDice.ShouldBe(8);
            diceRoll.Pool.HasLimit.ShouldBeTrue();
            diceRoll.Pool.Limit.ShouldBe(5);
            diceRoll.Outcome.Hits.ShouldBe(7); // Raw hits before limit application
        }

        [Fact]
        public void DiceRoll_ForGlitchScenario_ShouldWork()
        {
            // Arrange - Glitch scenario: more than half dice are 1s
            Result<DicePool> poolResult = DicePool.Create(attribute: 3, skill: 3);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 1, 1, 1, 1]; // 1 hit, 4 ones out of 5 dice = glitch
            DiceOutcome outcome = new(rolls, 1, 4, true, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Pool.TotalDice.ShouldBe(6);
            diceRoll.Outcome.IsGlitch.ShouldBeTrue();
            diceRoll.Outcome.IsCriticalGlitch.ShouldBeFalse(); // Has hits
            diceRoll.Outcome.IsSuccess.ShouldBeTrue(); // Still success due to hits
        }

        [Fact]
        public void DiceRoll_ForCriticalGlitchScenario_ShouldWork()
        {
            // Arrange - Critical glitch: more than half dice are 1s AND no hits
            Result<DicePool> poolResult = DicePool.Create(attribute: 2, skill: 2);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [1, 1, 1, 2]; // 0 hits, 3 ones out of 4 dice = critical glitch
            DiceOutcome outcome = new(rolls, 0, 3, true, true);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Pool.TotalDice.ShouldBe(4);
            diceRoll.Outcome.IsGlitch.ShouldBeTrue();
            diceRoll.Outcome.IsCriticalGlitch.ShouldBeTrue();
            diceRoll.Outcome.IsSuccess.ShouldBeFalse();
            diceRoll.Outcome.IsFailure.ShouldBeTrue();
        }
    }

    public sealed class EdgeCases
    {
        [Fact]
        public void DiceRoll_WithZeroHits_ShouldWork()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(3);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [2, 3, 4]; // No hits, no ones
            DiceOutcome outcome = new(rolls, 0, 0, false, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Outcome.Hits.ShouldBe(0);
            diceRoll.Outcome.Ones.ShouldBe(0);
            diceRoll.Outcome.IsSuccess.ShouldBeFalse();
            diceRoll.Outcome.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public void DiceRoll_WithAllSixes_ShouldWork()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(5);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [6, 6, 6, 6, 6]; // All hits
            DiceOutcome outcome = new(rolls, 5, 0, false, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Outcome.Hits.ShouldBe(5);
            diceRoll.Outcome.Ones.ShouldBe(0);
            diceRoll.Outcome.IsSuccess.ShouldBeTrue();
            diceRoll.Outcome.IsGlitch.ShouldBeFalse();
        }

        [Fact]
        public void DiceRoll_WithAllOnes_ShouldWork()
        {
            // Arrange
            Result<DicePool> poolResult = DicePool.Create(4);
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            IReadOnlyList<int> rolls = [1, 1, 1, 1]; // All ones = critical glitch
            DiceOutcome outcome = new(rolls, 0, 4, true, true);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Outcome.Hits.ShouldBe(0);
            diceRoll.Outcome.Ones.ShouldBe(4);
            diceRoll.Outcome.IsCriticalGlitch.ShouldBeTrue();
            diceRoll.Outcome.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public void DiceRoll_WithMaximumDicePool_ShouldWork()
        {
            // Arrange - Test with very large dice pool
            Result<DicePool> poolResult = DicePool.Create(attribute: 50, skill: 50); // 100 dice
            poolResult.TryGetValue(out DicePool? pool).ShouldBeTrue();
            
            // Create 100 dice rolls - mix of results
            List<int> largeDiceRolls = [];
            for (int i = 0; i < 100; i++)
            {
                largeDiceRolls.Add((i % 6) + 1); // Cycle through 1-6
            }
            
            IReadOnlyList<int> rolls = largeDiceRolls;
            int expectedHits = rolls.Count(r => r >= 5);
            int expectedOnes = rolls.Count(r => r == 1);
            DiceOutcome outcome = new(rolls, expectedHits, expectedOnes, false, false);
            DateTime rolledAt = DateTime.UtcNow;

            // Act
            DiceRoll diceRoll = new(pool!, outcome, rolledAt);

            // Assert
            diceRoll.Pool.TotalDice.ShouldBe(100);
            diceRoll.Outcome.Rolls.Count.ShouldBe(100);
            diceRoll.Outcome.Hits.ShouldBe(expectedHits);
            diceRoll.Outcome.Ones.ShouldBe(expectedOnes);
        }
    }
}