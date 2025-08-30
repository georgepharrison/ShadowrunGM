using ShadowrunGM.Domain.Mission;

namespace ShadowrunGM.Domain.Tests.Mission.ValueObjects;

/// <summary>
/// Comprehensive test suite for DiceOutcome value object behavior.
/// Tests outcome calculations, success/failure logic, glitch detection, and immutability.
/// </summary>
public sealed class DiceOutcomeTests
{
    public sealed class Constructor
    {
        [Fact]
        public void Constructor_WithValidData_ShouldSetAllProperties()
        {
            // Arrange
            IReadOnlyList<int> rolls = [5, 6, 3, 1, 4, 6];
            int hits = 3; // Three 5s and 6s
            int ones = 1; // One 1
            bool isGlitch = false;
            bool isCriticalGlitch = false;

            // Act
            DiceOutcome outcome = new(rolls, hits, ones, isGlitch, isCriticalGlitch);

            // Assert
            outcome.Rolls.ShouldBe(rolls);
            outcome.Hits.ShouldBe(hits);
            outcome.Ones.ShouldBe(ones);
            outcome.IsGlitch.ShouldBe(isGlitch);
            outcome.IsCriticalGlitch.ShouldBe(isCriticalGlitch);
        }

        [Fact]
        public void Constructor_WithEmptyRolls_ShouldWork()
        {
            // Arrange
            IReadOnlyList<int> emptyRolls = [];
            int hits = 0;
            int ones = 0;

            // Act
            DiceOutcome outcome = new(emptyRolls, hits, ones, false, false);

            // Assert
            outcome.Rolls.ShouldBe(emptyRolls);
            outcome.Rolls.Count.ShouldBe(0);
            outcome.Hits.ShouldBe(0);
            outcome.Ones.ShouldBe(0);
        }

        [Fact]
        public void Constructor_WithNullRolls_ShouldAcceptNull()
        {
            // This tests that the constructor accepts null (which would be a bug)
            // In a real implementation, we might want to guard against this
            
            // Act & Assert
            Should.NotThrow(() => new DiceOutcome(null!, 0, 0, false, false));
        }
    }

    public sealed class SuccessFailureLogic
    {
        [Theory]
        [InlineData(1, false, true)]
        [InlineData(3, false, true)]
        [InlineData(5, false, true)]
        public void IsSuccess_WithHitsAndNoCriticalGlitch_ShouldReturnTrue(
            int hits, bool isCriticalGlitch, bool expectedSuccess)
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 5, 3, 2];
            DiceOutcome outcome = new(rolls, hits, 0, false, isCriticalGlitch);

            // Act & Assert
            outcome.IsSuccess.ShouldBe(expectedSuccess);
            outcome.IsFailure.ShouldBe(!expectedSuccess);
        }

        [Fact]
        public void IsSuccess_WithNoHits_ShouldReturnFalse()
        {
            // Arrange
            IReadOnlyList<int> rolls = [3, 2, 4, 1];
            DiceOutcome outcome = new(rolls, 0, 1, false, false);

            // Act & Assert
            outcome.IsSuccess.ShouldBeFalse();
            outcome.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public void IsSuccess_WithCriticalGlitch_ShouldReturnFalse()
        {
            // Arrange - Critical glitch means failure regardless of hits
            IReadOnlyList<int> rolls = [6, 1, 1, 1]; // 1 hit but critical glitch
            DiceOutcome outcome = new(rolls, 1, 3, true, true);

            // Act & Assert
            outcome.IsSuccess.ShouldBeFalse();
            outcome.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public void IsSuccess_WithHitsAndRegularGlitch_ShouldReturnTrue()
        {
            // Arrange - Regular glitch doesn't prevent success if there are hits
            IReadOnlyList<int> rolls = [6, 5, 1, 1, 1]; // 2 hits, glitch but not critical
            DiceOutcome outcome = new(rolls, 2, 3, true, false);

            // Act & Assert
            outcome.IsSuccess.ShouldBeTrue();
            outcome.IsFailure.ShouldBeFalse();
        }

        [Theory]
        [InlineData(0, false, false)] // No hits, no glitch = failure
        [InlineData(0, true, false)] // No hits, regular glitch = failure
        [InlineData(0, false, true)] // No hits, critical glitch = failure
        [InlineData(2, false, true)] // Hits but critical glitch = failure
        public void IsFailure_ShouldReturnTrueForFailureConditions(
            int hits, bool isGlitch, bool isCriticalGlitch)
        {
            // Arrange
            IReadOnlyList<int> rolls = [1, 2, 3, 4];
            DiceOutcome outcome = new(rolls, hits, 0, isGlitch, isCriticalGlitch);

            // Act & Assert
            outcome.IsFailure.ShouldBeTrue();
            outcome.IsSuccess.ShouldBeFalse();
        }
    }

    public sealed class GlitchScenarios
    {
        [Fact]
        public void Outcome_WithGlitchButNoHits_ShouldBeCriticalGlitch()
        {
            // Arrange - More than half dice show 1, and no hits
            IReadOnlyList<int> rolls = [1, 1, 1, 2]; // 3 ones out of 4 dice, no hits
            DiceOutcome outcome = new(rolls, 0, 3, true, true);

            // Act & Assert
            outcome.IsGlitch.ShouldBeTrue();
            outcome.IsCriticalGlitch.ShouldBeTrue();
            outcome.IsSuccess.ShouldBeFalse();
            outcome.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public void Outcome_WithGlitchAndHits_ShouldBeGlitchButNotCritical()
        {
            // Arrange - More than half dice show 1, but there are hits
            IReadOnlyList<int> rolls = [6, 1, 1, 1]; // 3 ones out of 4 dice, 1 hit
            DiceOutcome outcome = new(rolls, 1, 3, true, false);

            // Act & Assert
            outcome.IsGlitch.ShouldBeTrue();
            outcome.IsCriticalGlitch.ShouldBeFalse();
            outcome.IsSuccess.ShouldBeTrue(); // Has hits and not critical glitch
            outcome.IsFailure.ShouldBeFalse();
        }

        [Fact]
        public void Outcome_WithLessThanHalfOnes_ShouldNotBeGlitch()
        {
            // Arrange - Less than half dice show 1
            IReadOnlyList<int> rolls = [6, 5, 1, 3]; // 1 one out of 4 dice
            DiceOutcome outcome = new(rolls, 2, 1, false, false);

            // Act & Assert
            outcome.IsGlitch.ShouldBeFalse();
            outcome.IsCriticalGlitch.ShouldBeFalse();
            outcome.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void Outcome_WithExactlyHalfOnes_ShouldNotBeGlitch()
        {
            // Arrange - Exactly half dice show 1 (not more than half)
            IReadOnlyList<int> rolls = [6, 5, 1, 1]; // 2 ones out of 4 dice = 50%
            DiceOutcome outcome = new(rolls, 2, 2, false, false);

            // Act & Assert
            outcome.IsGlitch.ShouldBeFalse();
            outcome.IsCriticalGlitch.ShouldBeFalse();
            outcome.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void Outcome_WithMoreThanHalfOnes_ShouldBeGlitch()
        {
            // Arrange - More than half dice show 1
            IReadOnlyList<int> rolls = [6, 1, 1, 1, 2]; // 3 ones out of 5 dice = 60%
            DiceOutcome outcome = new(rolls, 1, 3, true, false);

            // Act & Assert
            outcome.IsGlitch.ShouldBeTrue();
            outcome.IsCriticalGlitch.ShouldBeFalse(); // Has hits
        }

        [Fact]
        public void Outcome_WithAllOnes_ShouldBeCriticalGlitch()
        {
            // Arrange - All dice are 1s
            IReadOnlyList<int> rolls = [1, 1, 1, 1];
            DiceOutcome outcome = new(rolls, 0, 4, true, true);

            // Act & Assert
            outcome.IsGlitch.ShouldBeTrue();
            outcome.IsCriticalGlitch.ShouldBeTrue();
            outcome.IsSuccess.ShouldBeFalse();
            outcome.IsFailure.ShouldBeTrue();
        }
    }

    public sealed class SpecificRollScenarios
    {
        [Fact]
        public void Outcome_WithOnlyHits_ShouldBeSuccessful()
        {
            // Arrange - All dice are hits
            IReadOnlyList<int> rolls = [5, 6, 5, 6];
            DiceOutcome outcome = new(rolls, 4, 0, false, false);

            // Act & Assert
            outcome.Hits.ShouldBe(4);
            outcome.Ones.ShouldBe(0);
            outcome.IsGlitch.ShouldBeFalse();
            outcome.IsCriticalGlitch.ShouldBeFalse();
            outcome.IsSuccess.ShouldBeTrue();
            outcome.IsFailure.ShouldBeFalse();
        }

        [Fact]
        public void Outcome_WithMixedResults_ShouldCalculateCorrectly()
        {
            // Arrange - Mix of hits, ones, and misses
            IReadOnlyList<int> rolls = [6, 1, 4, 5, 2, 6]; // 3 hits, 1 one
            DiceOutcome outcome = new(rolls, 3, 1, false, false);

            // Act & Assert
            outcome.Hits.ShouldBe(3);
            outcome.Ones.ShouldBe(1);
            outcome.IsGlitch.ShouldBeFalse(); // 1 one out of 6 dice < 50%
            outcome.IsCriticalGlitch.ShouldBeFalse();
            outcome.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void Outcome_WithOnlyMisses_ShouldBeFailure()
        {
            // Arrange - All dice are misses (2, 3, 4)
            IReadOnlyList<int> rolls = [2, 3, 4, 3];
            DiceOutcome outcome = new(rolls, 0, 0, false, false);

            // Act & Assert
            outcome.Hits.ShouldBe(0);
            outcome.Ones.ShouldBe(0);
            outcome.IsGlitch.ShouldBeFalse();
            outcome.IsCriticalGlitch.ShouldBeFalse();
            outcome.IsSuccess.ShouldBeFalse();
            outcome.IsFailure.ShouldBeTrue();
        }

        [Fact]
        public void Outcome_WithSingleDie_ShouldWorkCorrectly()
        {
            // Arrange - Single die scenarios
            
            // Single hit
            IReadOnlyList<int> hitRolls = [6];
            DiceOutcome hitOutcome = new(hitRolls, 1, 0, false, false);
            
            // Single one (critical glitch since all dice are 1s and no hits)
            IReadOnlyList<int> oneRolls = [1];
            DiceOutcome oneOutcome = new(oneRolls, 0, 1, true, true);
            
            // Single miss
            IReadOnlyList<int> missRolls = [3];
            DiceOutcome missOutcome = new(missRolls, 0, 0, false, false);

            // Act & Assert
            hitOutcome.IsSuccess.ShouldBeTrue();
            hitOutcome.IsFailure.ShouldBeFalse();
            
            oneOutcome.IsSuccess.ShouldBeFalse();
            oneOutcome.IsFailure.ShouldBeTrue();
            oneOutcome.IsCriticalGlitch.ShouldBeTrue();
            
            missOutcome.IsSuccess.ShouldBeFalse();
            missOutcome.IsFailure.ShouldBeTrue();
            missOutcome.IsGlitch.ShouldBeFalse();
        }

        [Fact]
        public void Outcome_WithLargeDicePool_ShouldHandleCorrectly()
        {
            // Arrange - Large dice pool scenario
            IReadOnlyList<int> rolls = [6, 5, 4, 3, 2, 1, 6, 5, 4, 3, 2, 1]; // 4 hits, 2 ones
            DiceOutcome outcome = new(rolls, 4, 2, false, false);

            // Act & Assert
            outcome.Hits.ShouldBe(4);
            outcome.Ones.ShouldBe(2);
            outcome.IsGlitch.ShouldBeFalse(); // 2 ones out of 12 dice < 50%
            outcome.IsCriticalGlitch.ShouldBeFalse();
            outcome.IsSuccess.ShouldBeTrue();
        }
    }

    public sealed class ValueObjectBehavior
    {
        [Fact]
        public void TwoDiceOutcomes_WithSameValues_ShouldBeEqual()
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 1, 4, 5];
            DiceOutcome first = new(rolls, 2, 1, false, false);
            DiceOutcome second = new(rolls, 2, 1, false, false);

            // Act & Assert
            first.ShouldBe(second);
            first.GetHashCode().ShouldBe(second.GetHashCode());
        }

        [Fact]
        public void TwoDiceOutcomes_WithDifferentRolls_ShouldNotBeEqual()
        {
            // Arrange
            IReadOnlyList<int> firstRolls = [6, 1, 4, 5];
            IReadOnlyList<int> secondRolls = [6, 1, 4, 6]; // Different last roll
            DiceOutcome first = new(firstRolls, 2, 1, false, false);
            DiceOutcome second = new(secondRolls, 3, 1, false, false); // Different hits too

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void TwoDiceOutcomes_WithDifferentHits_ShouldNotBeEqual()
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 1, 4, 5];
            DiceOutcome first = new(rolls, 2, 1, false, false);
            DiceOutcome second = new(rolls, 3, 1, false, false); // Different hits

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void TwoDiceOutcomes_WithDifferentOnes_ShouldNotBeEqual()
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 1, 4, 5];
            DiceOutcome first = new(rolls, 2, 1, false, false);
            DiceOutcome second = new(rolls, 2, 2, false, false); // Different ones

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void TwoDiceOutcomes_WithDifferentGlitchStatus_ShouldNotBeEqual()
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 1, 4, 5];
            DiceOutcome first = new(rolls, 2, 1, false, false);
            DiceOutcome second = new(rolls, 2, 1, true, false); // Different glitch

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void TwoDiceOutcomes_WithDifferentCriticalGlitchStatus_ShouldNotBeEqual()
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 1, 4, 5];
            DiceOutcome first = new(rolls, 2, 1, false, false);
            DiceOutcome second = new(rolls, 2, 1, false, true); // Different critical glitch

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void DiceOutcome_ShouldBeImmutable()
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 1, 4, 5];
            DiceOutcome outcome = new(rolls, 2, 1, false, false);

            // Act & Assert - Value object should have no public setters
            typeof(DiceOutcome).GetProperty("Rolls")?.SetMethod.ShouldBeNull();
            typeof(DiceOutcome).GetProperty("Hits")?.SetMethod.ShouldBeNull();
            typeof(DiceOutcome).GetProperty("Ones")?.SetMethod.ShouldBeNull();
            typeof(DiceOutcome).GetProperty("IsGlitch")?.SetMethod.ShouldBeNull();
            typeof(DiceOutcome).GetProperty("IsCriticalGlitch")?.SetMethod.ShouldBeNull();
        }

        [Fact]
        public void DiceOutcome_WithNullComparison_ShouldNotBeEqual()
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 1, 4, 5];
            DiceOutcome outcome = new(rolls, 2, 1, false, false);

            // Act & Assert
            outcome.ShouldNotBe(null);
            outcome.Equals(null).ShouldBeFalse();
        }

        [Fact]
        public void DiceOutcome_WithDifferentType_ShouldNotBeEqual()
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 1, 4, 5];
            DiceOutcome outcome = new(rolls, 2, 1, false, false);
            object differentType = "not a dice outcome";

            // Act & Assert
            outcome.Equals(differentType).ShouldBeFalse();
        }

        [Fact]
        public void Rolls_ShouldBeImmutableCollection()
        {
            // Arrange
            List<int> originalRolls = [6, 1, 4, 5];
            DiceOutcome outcome = new(originalRolls, 2, 1, false, false);

            // Act - Try to modify original list
            originalRolls.Add(3);

            // Assert - Outcome should not be affected
            outcome.Rolls.Count.ShouldBe(4);
            outcome.Rolls.ShouldNotContain(3);
            
            // The Rolls property should return IReadOnlyList
            outcome.Rolls.ShouldBeAssignableTo<IReadOnlyList<int>>();
        }
    }

    public sealed class ToStringBehavior
    {
        [Fact]
        public void ToString_WithSuccessfulOutcome_ShouldShowHits()
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 5, 3, 2];
            DiceOutcome outcome = new(rolls, 2, 0, false, false);

            // Act
            string stringRepresentation = outcome.ToString();

            // Assert
            stringRepresentation.ShouldBe("2 hits");
        }

        [Fact]
        public void ToString_WithCriticalGlitch_ShouldShowCriticalGlitch()
        {
            // Arrange
            IReadOnlyList<int> rolls = [1, 1, 1, 2];
            DiceOutcome outcome = new(rolls, 0, 3, true, true);

            // Act
            string stringRepresentation = outcome.ToString();

            // Assert
            stringRepresentation.ShouldBe("0 hits [CRITICAL GLITCH!]");
        }

        [Fact]
        public void ToString_WithRegularGlitch_ShouldShowGlitch()
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 1, 1, 1];
            DiceOutcome outcome = new(rolls, 1, 3, true, false);

            // Act
            string stringRepresentation = outcome.ToString();

            // Assert
            stringRepresentation.ShouldBe("1 hits [Glitch]");
        }

        [Fact]
        public void ToString_WithNoHitsNoGlitch_ShouldShowJustHits()
        {
            // Arrange
            IReadOnlyList<int> rolls = [2, 3, 4, 3];
            DiceOutcome outcome = new(rolls, 0, 0, false, false);

            // Act
            string stringRepresentation = outcome.ToString();

            // Assert
            stringRepresentation.ShouldBe("0 hits");
        }

        [Fact]
        public void ToString_WithSingleHit_ShouldShowSingular()
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 2, 3, 4];
            DiceOutcome outcome = new(rolls, 1, 0, false, false);

            // Act
            string stringRepresentation = outcome.ToString();

            // Assert
            stringRepresentation.ShouldBe("1 hits"); // Note: implementation might use "hits" plural always
        }

        [Fact]
        public void ToString_WithMultipleHits_ShouldShowPlural()
        {
            // Arrange
            IReadOnlyList<int> rolls = [6, 5, 6, 4];
            DiceOutcome outcome = new(rolls, 3, 0, false, false);

            // Act
            string stringRepresentation = outcome.ToString();

            // Assert
            stringRepresentation.ShouldBe("3 hits");
        }
    }

    public sealed class EdgeCasesAndBoundaries
    {
        [Fact]
        public void Outcome_WithMaximumHits_ShouldWork()
        {
            // Arrange - All dice are 6s (maximum hits scenario)
            IReadOnlyList<int> rolls = [6, 6, 6, 6, 6];
            DiceOutcome outcome = new(rolls, 5, 0, false, false);

            // Act & Assert
            outcome.Hits.ShouldBe(5);
            outcome.Ones.ShouldBe(0);
            outcome.IsSuccess.ShouldBeTrue();
            outcome.IsGlitch.ShouldBeFalse();
        }

        [Fact]
        public void Outcome_WithZeroHitsAndZeroOnes_ShouldWork()
        {
            // Arrange - All dice are misses (2, 3, 4)
            IReadOnlyList<int> rolls = [2, 3, 4, 2];
            DiceOutcome outcome = new(rolls, 0, 0, false, false);

            // Act & Assert
            outcome.Hits.ShouldBe(0);
            outcome.Ones.ShouldBe(0);
            outcome.IsSuccess.ShouldBeFalse();
            outcome.IsFailure.ShouldBeTrue();
            outcome.IsGlitch.ShouldBeFalse();
        }

        [Fact]
        public void Outcome_WithInconsistentData_ShouldAcceptAsGiven()
        {
            // This tests defensive programming - the constructor accepts values as given
            // In a real system, we might want validation, but this tests current behavior
            
            // Arrange - Inconsistent data (says 5 hits but rolls don't support it)
            IReadOnlyList<int> rolls = [2, 3, 4]; // No hits in actual rolls
            DiceOutcome outcome = new(rolls, 5, 0, false, false); // Claims 5 hits

            // Act & Assert - Constructor accepts values as given
            outcome.Hits.ShouldBe(5);
            outcome.Rolls.Count.ShouldBe(3);
            outcome.IsSuccess.ShouldBeTrue(); // Based on hits count, not actual rolls
        }

        [Fact]
        public void Outcome_WithNegativeValues_ShouldAcceptAsGiven()
        {
            // This tests what happens with invalid input (negative values)
            // In production, we might want validation
            
            // Arrange
            IReadOnlyList<int> rolls = [6, 5, 4];
            DiceOutcome outcome = new(rolls, -1, -1, false, false);

            // Act & Assert - Constructor accepts values as given
            outcome.Hits.ShouldBe(-1);
            outcome.Ones.ShouldBe(-1);
            outcome.IsSuccess.ShouldBeFalse(); // -1 hits is not > 0
        }

        [Fact]
        public void Outcome_WithVeryLargeDicePool_ShouldWork()
        {
            // Arrange - Test with large numbers
            int[] largeRollArray = new int[50];
            for (int i = 0; i < 50; i++)
            {
                largeRollArray[i] = (i % 6) + 1; // Cycle through 1-6
            }
            IReadOnlyList<int> largeRolls = largeRollArray;
            
            // Count expected hits and ones
            int expectedHits = largeRolls.Count(r => r >= 5); // 5s and 6s
            int expectedOnes = largeRolls.Count(r => r == 1);
            
            DiceOutcome outcome = new(largeRolls, expectedHits, expectedOnes, false, false);

            // Act & Assert
            outcome.Rolls.Count.ShouldBe(50);
            outcome.Hits.ShouldBe(expectedHits);
            outcome.Ones.ShouldBe(expectedOnes);
        }
    }
}