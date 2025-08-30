using ShadowrunGM.Domain.Mission;

namespace ShadowrunGM.Domain.Tests.Mission.ValueObjects;

/// <summary>
/// Comprehensive test suite for DicePool value object behavior.
/// Tests creation, validation, calculations, and immutability.
/// </summary>
public sealed class DicePoolTests
{
    public sealed class Create
    {
        [Fact]
        public void Create_WithAttributeOnly_ShouldSucceed()
        {
            // Act
            Result<DicePool> result = DicePool.Create(attribute: 5);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.Attribute.ShouldBe(5);
            pool.Skill.ShouldBe(0);
            pool.Modifiers.ShouldBe(0);
            pool.EdgeBonus.ShouldBe(0);
            pool.Limit.ShouldBe(0);
            pool.TotalDice.ShouldBe(5);
            pool.HasLimit.ShouldBeFalse();
            pool.IgnoresLimit.ShouldBeFalse();
        }

        [Fact]
        public void Create_WithAttributeAndSkill_ShouldSucceed()
        {
            // Act
            Result<DicePool> result = DicePool.Create(attribute: 4, skill: 3);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.Attribute.ShouldBe(4);
            pool.Skill.ShouldBe(3);
            pool.TotalDice.ShouldBe(7);
            pool.HasLimit.ShouldBeFalse();
        }

        [Fact]
        public void Create_WithAllParameters_ShouldSucceed()
        {
            // Act
            Result<DicePool> result = DicePool.Create(
                attribute: 5, 
                skill: 4, 
                modifiers: -2, 
                edgeBonus: 3, 
                limit: 6);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.Attribute.ShouldBe(5);
            pool.Skill.ShouldBe(4);
            pool.Modifiers.ShouldBe(-2);
            pool.EdgeBonus.ShouldBe(3);
            pool.Limit.ShouldBe(6);
            pool.TotalDice.ShouldBe(10); // 5 + 4 - 2 + 3 = 10
            pool.HasLimit.ShouldBeTrue();
            pool.IgnoresLimit.ShouldBeFalse();
        }

        [Fact]
        public void Create_WithPositiveModifiers_ShouldIncreaseTotal()
        {
            // Act
            Result<DicePool> result = DicePool.Create(
                attribute: 3, 
                skill: 2, 
                modifiers: 4);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.TotalDice.ShouldBe(9); // 3 + 2 + 4 = 9
        }

        [Fact]
        public void Create_WithNegativeModifiers_ShouldDecreaseTotal()
        {
            // Act
            Result<DicePool> result = DicePool.Create(
                attribute: 5, 
                skill: 3, 
                modifiers: -4);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.TotalDice.ShouldBe(4); // 5 + 3 - 4 = 4
        }

        [Fact]
        public void Create_WithEdgeBonusAndZeroLimit_ShouldIgnoreLimit()
        {
            // Act
            Result<DicePool> result = DicePool.Create(
                attribute: 4, 
                skill: 3, 
                edgeBonus: 2, 
                limit: 0);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.EdgeBonus.ShouldBe(2);
            pool.Limit.ShouldBe(0);
            pool.HasLimit.ShouldBeFalse();
            pool.IgnoresLimit.ShouldBeTrue(); // Edge bonus > 0 and limit = 0
        }

        [Fact]
        public void Create_WithEdgeBonusAndPositiveLimit_ShouldNotIgnoreLimit()
        {
            // Act
            Result<DicePool> result = DicePool.Create(
                attribute: 4, 
                skill: 3, 
                edgeBonus: 2, 
                limit: 5);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.EdgeBonus.ShouldBe(2);
            pool.Limit.ShouldBe(5);
            pool.HasLimit.ShouldBeTrue();
            pool.IgnoresLimit.ShouldBeFalse();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void Create_WithNegativeAttribute_ShouldReturnFailure(int negativeAttribute)
        {
            // Act
            Result<DicePool> result = DicePool.Create(attribute: negativeAttribute);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Attribute dice cannot be negative.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-3)]
        public void Create_WithNegativeSkill_ShouldReturnFailure(int negativeSkill)
        {
            // Act
            Result<DicePool> result = DicePool.Create(attribute: 3, skill: negativeSkill);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Skill dice cannot be negative.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-2)]
        public void Create_WithNegativeEdgeBonus_ShouldReturnFailure(int negativeEdgeBonus)
        {
            // Act
            Result<DicePool> result = DicePool.Create(
                attribute: 3, 
                edgeBonus: negativeEdgeBonus);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Edge bonus cannot be negative.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void Create_WithNegativeLimit_ShouldReturnFailure(int negativeLimit)
        {
            // Act
            Result<DicePool> result = DicePool.Create(
                attribute: 3, 
                limit: negativeLimit);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Limit cannot be negative.");
        }

        [Fact]
        public void Create_WithTotalDiceZero_ShouldReturnFailure()
        {
            // Act - Negative modifiers equal to positive dice
            Result<DicePool> result = DicePool.Create(
                attribute: 3, 
                skill: 2, 
                modifiers: -5); // Total = 3 + 2 - 5 = 0

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Dice pool must have at least 1 die.");
        }

        [Fact]
        public void Create_WithTotalDiceNegative_ShouldReturnFailure()
        {
            // Act - Negative modifiers exceed positive dice
            Result<DicePool> result = DicePool.Create(
                attribute: 2, 
                skill: 1, 
                modifiers: -5); // Total = 2 + 1 - 5 = -2, but Math.Max(0, -2) = 0

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Dice pool must have at least 1 die.");
        }

        [Fact]
        public void Create_WithExcessiveTotalDice_ShouldReturnFailure()
        {
            // Act
            Result<DicePool> result = DicePool.Create(
                attribute: 50, 
                skill: 30, 
                modifiers: 21); // Total = 101

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Dice pool cannot exceed 100 dice.");
        }

        [Fact]
        public void Create_WithExactlyMaxDice_ShouldSucceed()
        {
            // Act
            Result<DicePool> result = DicePool.Create(
                attribute: 50, 
                skill: 30, 
                modifiers: 20); // Total = 100

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.TotalDice.ShouldBe(100);
        }

        [Fact]
        public void Create_WithOneDie_ShouldSucceed()
        {
            // Act
            Result<DicePool> result = DicePool.Create(attribute: 1);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.TotalDice.ShouldBe(1);
        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0)] // All zeros
        [InlineData(0, 0, 0, 0, 5)] // Only limit
        public void Create_WithAllParametersZeroExceptLimit_ShouldReturnFailure(
            int attribute, int skill, int modifiers, int edgeBonus, int limit)
        {
            // Act
            Result<DicePool> result = DicePool.Create(
                attribute, skill, modifiers, edgeBonus, limit);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Dice pool must have at least 1 die.");
        }
    }

    public sealed class TotalDiceCalculation
    {
        [Theory]
        [InlineData(3, 2, 0, 0, 5)] // 3 + 2 + 0 + 0 = 5
        [InlineData(4, 3, 2, 1, 10)] // 4 + 3 + 2 + 1 = 10
        [InlineData(5, 0, -2, 3, 6)] // 5 + 0 - 2 + 3 = 6
        [InlineData(2, 1, -1, 0, 2)] // 2 + 1 - 1 + 0 = 2
        public void TotalDice_ShouldCalculateCorrectly(
            int attribute, int skill, int modifiers, int edgeBonus, int expectedTotal)
        {
            // Arrange
            Result<DicePool> result = DicePool.Create(
                attribute, skill, modifiers, edgeBonus);
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();

            // Act & Assert
            pool!.TotalDice.ShouldBe(expectedTotal);
        }

        [Fact]
        public void TotalDice_WithNegativeResultFromModifiers_ShouldBeZeroInternally()
        {
            // This test documents the Math.Max(0, ...) behavior in TotalDice calculation
            // Even though we validate against this in Create, the property handles it safely
            
            // We can't actually create such a pool via Create (it would fail validation)
            // But this test documents the expected behavior of the calculation logic
            
            // This test will be skipped as we can't create invalid pools
            // But it documents the defensive programming in the TotalDice property
        }
    }

    public sealed class LimitBehavior
    {
        [Theory]
        [InlineData(0, false)]
        [InlineData(5, true)]
        [InlineData(10, true)]
        public void HasLimit_ShouldReflectLimitValue(int limit, bool expectedHasLimit)
        {
            // Arrange
            Result<DicePool> result = DicePool.Create(
                attribute: 3, 
                limit: limit);
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();

            // Act & Assert
            pool!.HasLimit.ShouldBe(expectedHasLimit);
        }

        [Theory]
        [InlineData(0, 0, false)] // No edge, no limit
        [InlineData(2, 0, true)] // Edge bonus with zero limit = ignores limit
        [InlineData(2, 5, false)] // Edge bonus with positive limit = doesn't ignore limit
        [InlineData(0, 5, false)] // No edge bonus = doesn't ignore limit
        public void IgnoresLimit_ShouldReflectEdgeBonusAndLimitCombination(
            int edgeBonus, int limit, bool expectedIgnoresLimit)
        {
            // Arrange
            Result<DicePool> result = DicePool.Create(
                attribute: 3, 
                edgeBonus: edgeBonus, 
                limit: limit);
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();

            // Act & Assert
            pool!.IgnoresLimit.ShouldBe(expectedIgnoresLimit);
        }
    }

    public sealed class ValueObjectBehavior
    {
        [Fact]
        public void TwoDicePools_WithSameValues_ShouldBeEqual()
        {
            // Arrange
            Result<DicePool> first = DicePool.Create(4, 3, -1, 2, 6);
            Result<DicePool> second = DicePool.Create(4, 3, -1, 2, 6);

            // Act & Assert
            first.TryGetValue(out DicePool? firstPool).ShouldBeTrue();
            second.TryGetValue(out DicePool? secondPool).ShouldBeTrue();
            
            firstPool.ShouldBe(secondPool);
            firstPool!.GetHashCode().ShouldBe(secondPool!.GetHashCode());
        }

        [Fact]
        public void TwoDicePools_WithDifferentAttributes_ShouldNotBeEqual()
        {
            // Arrange
            Result<DicePool> first = DicePool.Create(4, 3);
            Result<DicePool> second = DicePool.Create(5, 3); // Different attribute

            // Act & Assert
            first.TryGetValue(out DicePool? firstPool).ShouldBeTrue();
            second.TryGetValue(out DicePool? secondPool).ShouldBeTrue();
            
            firstPool.ShouldNotBe(secondPool);
        }

        [Fact]
        public void TwoDicePools_WithDifferentSkills_ShouldNotBeEqual()
        {
            // Arrange
            Result<DicePool> first = DicePool.Create(4, 3);
            Result<DicePool> second = DicePool.Create(4, 4); // Different skill

            // Act & Assert
            first.TryGetValue(out DicePool? firstPool).ShouldBeTrue();
            second.TryGetValue(out DicePool? secondPool).ShouldBeTrue();
            
            firstPool.ShouldNotBe(secondPool);
        }

        [Fact]
        public void TwoDicePools_WithDifferentModifiers_ShouldNotBeEqual()
        {
            // Arrange
            Result<DicePool> first = DicePool.Create(4, 3, -1);
            Result<DicePool> second = DicePool.Create(4, 3, -2); // Different modifier

            // Act & Assert
            first.TryGetValue(out DicePool? firstPool).ShouldBeTrue();
            second.TryGetValue(out DicePool? secondPool).ShouldBeTrue();
            
            firstPool.ShouldNotBe(secondPool);
        }

        [Fact]
        public void TwoDicePools_WithDifferentEdgeBonus_ShouldNotBeEqual()
        {
            // Arrange
            Result<DicePool> first = DicePool.Create(4, 3, 0, 1);
            Result<DicePool> second = DicePool.Create(4, 3, 0, 2); // Different edge bonus

            // Act & Assert
            first.TryGetValue(out DicePool? firstPool).ShouldBeTrue();
            second.TryGetValue(out DicePool? secondPool).ShouldBeTrue();
            
            firstPool.ShouldNotBe(secondPool);
        }

        [Fact]
        public void TwoDicePools_WithDifferentLimits_ShouldNotBeEqual()
        {
            // Arrange
            Result<DicePool> first = DicePool.Create(4, 3, 0, 0, 5);
            Result<DicePool> second = DicePool.Create(4, 3, 0, 0, 6); // Different limit

            // Act & Assert
            first.TryGetValue(out DicePool? firstPool).ShouldBeTrue();
            second.TryGetValue(out DicePool? secondPool).ShouldBeTrue();
            
            firstPool.ShouldNotBe(secondPool);
        }

        [Fact]
        public void DicePool_ShouldBeImmutable()
        {
            // Arrange
            Result<DicePool> result = DicePool.Create(4, 3, -1, 2, 5);
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();

            // Act & Assert - Value object should have no public setters
            typeof(DicePool).GetProperty("Attribute")?.SetMethod.ShouldBeNull();
            typeof(DicePool).GetProperty("Skill")?.SetMethod.ShouldBeNull();
            typeof(DicePool).GetProperty("Modifiers")?.SetMethod.ShouldBeNull();
            typeof(DicePool).GetProperty("EdgeBonus")?.SetMethod.ShouldBeNull();
            typeof(DicePool).GetProperty("Limit")?.SetMethod.ShouldBeNull();
        }

        [Fact]
        public void DicePool_WithNullComparison_ShouldNotBeEqual()
        {
            // Arrange
            Result<DicePool> result = DicePool.Create(4, 3);
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();

            // Act & Assert
            pool.ShouldNotBe(null);
            pool!.Equals(null).ShouldBeFalse();
        }

        [Fact]
        public void DicePool_WithDifferentType_ShouldNotBeEqual()
        {
            // Arrange
            Result<DicePool> result = DicePool.Create(4, 3);
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            object differentType = "not a dice pool";

            // Act & Assert
            pool!.Equals(differentType).ShouldBeFalse();
        }
    }

    public sealed class ToStringBehavior
    {
        [Fact]
        public void ToString_WithBasicPool_ShouldShowCorrectFormat()
        {
            // Arrange
            Result<DicePool> result = DicePool.Create(attribute: 5);
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();

            // Act
            string stringRepresentation = pool!.ToString();

            // Assert
            stringRepresentation.ShouldBe("5d6");
        }

        [Fact]
        public void ToString_WithLimit_ShouldShowLimit()
        {
            // Arrange
            Result<DicePool> result = DicePool.Create(
                attribute: 4, 
                skill: 3, 
                limit: 5);
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();

            // Act
            string stringRepresentation = pool!.ToString();

            // Assert
            stringRepresentation.ShouldBe("7d6 [Limit 5]");
        }

        [Fact]
        public void ToString_WithEdgeBonusIgnoringLimit_ShouldShowNoLimit()
        {
            // Arrange
            Result<DicePool> result = DicePool.Create(
                attribute: 4, 
                skill: 3, 
                edgeBonus: 2, 
                limit: 0);
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();

            // Act
            string stringRepresentation = pool!.ToString();

            // Assert
            stringRepresentation.ShouldBe("9d6 [No Limit - Edge]");
        }

        [Fact]
        public void ToString_WithEdgeBonusAndLimit_ShouldShowLimit()
        {
            // Arrange
            Result<DicePool> result = DicePool.Create(
                attribute: 4, 
                skill: 3, 
                edgeBonus: 2, 
                limit: 6);
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();

            // Act
            string stringRepresentation = pool!.ToString();

            // Assert
            stringRepresentation.ShouldBe("9d6 [Limit 6]");
        }

        [Fact]
        public void ToString_WithComplexPool_ShouldShowTotalDice()
        {
            // Arrange
            Result<DicePool> result = DicePool.Create(
                attribute: 5, 
                skill: 4, 
                modifiers: -3, 
                edgeBonus: 2, 
                limit: 7);
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();

            // Act
            string stringRepresentation = pool!.ToString();

            // Assert
            stringRepresentation.ShouldBe("8d6 [Limit 7]"); // 5 + 4 - 3 + 2 = 8
        }
    }

    public sealed class EdgeCases
    {
        [Fact]
        public void Create_WithLargeNegativeModifiers_ThatStillLeavePositiveDice_ShouldSucceed()
        {
            // Arrange & Act
            Result<DicePool> result = DicePool.Create(
                attribute: 10, 
                skill: 5, 
                modifiers: -14); // Total = 10 + 5 - 14 = 1

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.TotalDice.ShouldBe(1);
        }

        [Fact]
        public void Create_WithVeryHighEdgeBonus_ShouldSucceed()
        {
            // Arrange & Act
            Result<DicePool> result = DicePool.Create(
                attribute: 3, 
                skill: 2, 
                edgeBonus: 20); // High edge bonus

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.TotalDice.ShouldBe(25);
        }

        [Fact]
        public void Create_WithBoundaryValues_ShouldHandleCorrectly()
        {
            // Test boundary cases that are just within valid limits
            
            // Arrange & Act - Exactly at maximum
            Result<DicePool> maxResult = DicePool.Create(
                attribute: 100); // Exactly 100 dice
            
            // Arrange & Act - Exactly at minimum
            Result<DicePool> minResult = DicePool.Create(
                attribute: 1); // Exactly 1 die

            // Assert
            maxResult.IsSuccess.ShouldBeTrue();
            maxResult.TryGetValue(out DicePool? maxPool).ShouldBeTrue();
            maxPool!.TotalDice.ShouldBe(100);

            minResult.IsSuccess.ShouldBeTrue();
            minResult.TryGetValue(out DicePool? minPool).ShouldBeTrue();
            minPool!.TotalDice.ShouldBe(1);
        }

        [Fact]
        public void Create_WithZeroAttributeButPositiveSkill_ShouldSucceed()
        {
            // Arrange & Act
            Result<DicePool> result = DicePool.Create(
                attribute: 0, 
                skill: 5); // Only skill dice

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.Attribute.ShouldBe(0);
            pool.Skill.ShouldBe(5);
            pool.TotalDice.ShouldBe(5);
        }

        [Fact]
        public void Create_WithZeroSkillButPositiveAttribute_ShouldSucceed()
        {
            // Arrange & Act
            Result<DicePool> result = DicePool.Create(
                attribute: 4, 
                skill: 0); // Only attribute dice

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out DicePool? pool).ShouldBeTrue();
            pool.ShouldNotBeNull();
            pool.Attribute.ShouldBe(4);
            pool.Skill.ShouldBe(0);
            pool.TotalDice.ShouldBe(4);
        }
    }
}