using ShadowrunGM.Domain.Character.ValueObjects;

namespace ShadowrunGM.Domain.Tests.Character.ValueObjects;

/// <summary>
/// Comprehensive test suite for ConditionMonitor value object behavior.
/// Tests damage tracking, overflow mechanics, healing, health state calculations, and immutability.
/// </summary>
public sealed class ConditionMonitorTests
{
    public sealed class ForAttributes
    {
        [Theory]
        [InlineData(1, 9, 9)] // Body 1 -> 8 + ceiling(1/2) = 8 + 1 = 9
        [InlineData(2, 9, 9)] // Body 2 -> 8 + ceiling(2/2) = 8 + 1 = 9
        [InlineData(3, 10, 10)] // Body 3 -> 8 + ceiling(3/2) = 8 + 2 = 10
        [InlineData(4, 10, 10)] // Body 4 -> 8 + ceiling(4/2) = 8 + 2 = 10
        [InlineData(6, 11, 11)] // Body 6 -> 8 + ceiling(6/2) = 8 + 3 = 11
        [InlineData(10, 13, 13)] // Body 10 -> 8 + ceiling(10/2) = 8 + 5 = 13
        public void ForAttributes_WithVariousBodyValues_ShouldCalculatePhysicalBoxesCorrectly(
            int body, int expectedPhysicalBoxes, int willpower)
        {
            // Arrange
            AttributeSet attributes = new AttributeSetBuilder()
                .WithBody(body)
                .WithWillpower(willpower) // Keep willpower constant for this test
                .Build();

            // Act
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(attributes);

            // Assert
            monitor.PhysicalBoxes.ShouldBe(expectedPhysicalBoxes);
            monitor.PhysicalDamage.ShouldBe(0);
            monitor.PhysicalRemaining.ShouldBe(expectedPhysicalBoxes);
        }

        [Theory]
        [InlineData(1, 9)] // Willpower 1 -> 8 + ceiling(1/2) = 8 + 1 = 9
        [InlineData(2, 9)] // Willpower 2 -> 8 + ceiling(2/2) = 8 + 1 = 9
        [InlineData(3, 10)] // Willpower 3 -> 8 + ceiling(3/2) = 8 + 2 = 10
        [InlineData(4, 10)] // Willpower 4 -> 8 + ceiling(4/2) = 8 + 2 = 10
        [InlineData(6, 11)] // Willpower 6 -> 8 + ceiling(6/2) = 8 + 3 = 11
        [InlineData(10, 13)] // Willpower 10 -> 8 + ceiling(10/2) = 8 + 5 = 13
        public void ForAttributes_WithVariousWillpowerValues_ShouldCalculateStunBoxesCorrectly(int willpower, int expectedStunBoxes)
        {
            // Arrange
            AttributeSet attributes = new AttributeSetBuilder()
                .WithBody(3) // Keep body constant for this test
                .WithWillpower(willpower)
                .Build();

            // Act
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(attributes);

            // Assert
            monitor.StunBoxes.ShouldBe(expectedStunBoxes);
            monitor.StunDamage.ShouldBe(0);
            monitor.StunRemaining.ShouldBe(expectedStunBoxes);
        }

        [Fact]
        public void ForAttributes_WithMinimumAttributes_ShouldCreateValidMonitor()
        {
            // Arrange
            AttributeSet attributes = new AttributeSetBuilder()
                .WithMinimumAttributes() // Body 1, Willpower 1
                .Build();

            // Act
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(attributes);

            // Assert
            monitor.PhysicalBoxes.ShouldBe(9); // 8 + ceiling(1/2) = 9
            monitor.StunBoxes.ShouldBe(9); // 8 + ceiling(1/2) = 9
            monitor.PhysicalDamage.ShouldBe(0);
            monitor.StunDamage.ShouldBe(0);
            monitor.IsUnconscious.ShouldBeFalse();
            monitor.IsDying.ShouldBeFalse();
        }

        [Fact]
        public void ForAttributes_WithMaximumAttributes_ShouldCreateValidMonitor()
        {
            // Arrange
            AttributeSet attributes = new AttributeSetBuilder()
                .WithMaximumAttributes() // Body 10, Willpower 10
                .Build();

            // Act
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(attributes);

            // Assert
            monitor.PhysicalBoxes.ShouldBe(13); // 8 + ceiling(10/2) = 13
            monitor.StunBoxes.ShouldBe(13); // 8 + ceiling(10/2) = 13
            monitor.PhysicalDamage.ShouldBe(0);
            monitor.StunDamage.ShouldBe(0);
            monitor.IsUnconscious.ShouldBeFalse();
            monitor.IsDying.ShouldBeFalse();
        }
    }

    public sealed class Create
    {
        [Fact]
        public void Create_WithValidValues_ShouldReturnSuccessfulResult()
        {
            // Arrange
            int physicalBoxes = 10, stunBoxes = 11;
            int physicalDamage = 3, stunDamage = 2;

            // Act
            Result<ConditionMonitor> result = ConditionMonitor.Create(
                physicalBoxes, stunBoxes, physicalDamage, stunDamage);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? monitor).ShouldBeTrue();
            monitor.ShouldNotBeNull();
            monitor.PhysicalBoxes.ShouldBe(physicalBoxes);
            monitor.StunBoxes.ShouldBe(stunBoxes);
            monitor.PhysicalDamage.ShouldBe(physicalDamage);
            monitor.StunDamage.ShouldBe(stunDamage);
        }

        [Theory]
        [InlineData(7)]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_WithPhysicalBoxesBelowMinimum_ShouldReturnFailure(int invalidPhysicalBoxes)
        {
            // Arrange
            int stunBoxes = 8, physicalDamage = 0, stunDamage = 0;

            // Act
            Result<ConditionMonitor> result = ConditionMonitor.Create(
                invalidPhysicalBoxes, stunBoxes, physicalDamage, stunDamage);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Physical boxes must be at least 8.");
        }

        [Theory]
        [InlineData(7)]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_WithStunBoxesBelowMinimum_ShouldReturnFailure(int invalidStunBoxes)
        {
            // Arrange
            int physicalBoxes = 8, physicalDamage = 0, stunDamage = 0;

            // Act
            Result<ConditionMonitor> result = ConditionMonitor.Create(
                physicalBoxes, invalidStunBoxes, physicalDamage, stunDamage);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Stun boxes must be at least 8.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void Create_WithNegativePhysicalDamage_ShouldReturnFailure(int negativePhysicalDamage)
        {
            // Arrange
            int physicalBoxes = 8, stunBoxes = 8, stunDamage = 0;

            // Act
            Result<ConditionMonitor> result = ConditionMonitor.Create(
                physicalBoxes, stunBoxes, negativePhysicalDamage, stunDamage);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Physical damage cannot be negative.");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void Create_WithNegativeStunDamage_ShouldReturnFailure(int negativeStunDamage)
        {
            // Arrange
            int physicalBoxes = 8, stunBoxes = 8, physicalDamage = 0;

            // Act
            Result<ConditionMonitor> result = ConditionMonitor.Create(
                physicalBoxes, stunBoxes, physicalDamage, negativeStunDamage);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Stun damage cannot be negative.");
        }

        [Fact]
        public void Create_WithMinimumValidValues_ShouldSucceed()
        {
            // Arrange & Act
            Result<ConditionMonitor> result = ConditionMonitor.Create(8, 8, 0, 0);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? monitor).ShouldBeTrue();
            monitor.ShouldNotBeNull();
            monitor.PhysicalBoxes.ShouldBe(8);
            monitor.StunBoxes.ShouldBe(8);
            monitor.PhysicalDamage.ShouldBe(0);
            monitor.StunDamage.ShouldBe(0);
        }
    }

    public sealed class TakePhysicalDamage
    {
        [Fact]
        public void TakePhysicalDamage_WithValidAmount_ShouldIncreasePhysicalDamage()
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build()); // 10 physical boxes
            int damageAmount = 3;

            // Act
            Result<ConditionMonitor> result = monitor.TakePhysicalDamage(damageAmount);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? newMonitor).ShouldBeTrue();
            newMonitor.ShouldNotBeNull();
            newMonitor.PhysicalDamage.ShouldBe(3);
            newMonitor.PhysicalRemaining.ShouldBe(7);
            newMonitor.StunDamage.ShouldBe(0); // Stun damage unchanged
        }

        [Fact]
        public void TakePhysicalDamage_ExceedingPhysicalBoxes_ShouldAllowOverflow()
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build()); // 10 physical boxes
            int excessiveDamage = 15;

            // Act
            Result<ConditionMonitor> result = monitor.TakePhysicalDamage(excessiveDamage);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? newMonitor).ShouldBeTrue();
            newMonitor.ShouldNotBeNull();
            newMonitor.PhysicalDamage.ShouldBe(15);
            newMonitor.PhysicalRemaining.ShouldBe(0); // Can't go below 0
            newMonitor.IsDying.ShouldBeTrue();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void TakePhysicalDamage_WithNegativeAmount_ShouldReturnFailure(int negativeDamage)
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build());

            // Act
            Result<ConditionMonitor> result = monitor.TakePhysicalDamage(negativeDamage);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Damage amount cannot be negative.");
        }

        [Fact]
        public void TakePhysicalDamage_WithZeroAmount_ShouldSucceedWithNoChange()
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build());

            // Act
            Result<ConditionMonitor> result = monitor.TakePhysicalDamage(0);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? newMonitor).ShouldBeTrue();
            newMonitor.ShouldNotBeNull();
            newMonitor.PhysicalDamage.ShouldBe(0);
            newMonitor.StunDamage.ShouldBe(0);
        }

        [Fact]
        public void TakePhysicalDamage_Accumulates_WithMultipleCalls()
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build());

            // Act
            Result<ConditionMonitor> firstResult = monitor.TakePhysicalDamage(2);
            firstResult.TryGetValue(out ConditionMonitor? firstMonitor).ShouldBeTrue();
            
            Result<ConditionMonitor> secondResult = firstMonitor!.TakePhysicalDamage(3);

            // Assert
            secondResult.IsSuccess.ShouldBeTrue();
            secondResult.TryGetValue(out ConditionMonitor? finalMonitor).ShouldBeTrue();
            finalMonitor.ShouldNotBeNull();
            finalMonitor.PhysicalDamage.ShouldBe(5);
            finalMonitor.PhysicalRemaining.ShouldBe(5);
        }
    }

    public sealed class TakeStunDamage
    {
        [Fact]
        public void TakeStunDamage_WithinStunBoxes_ShouldIncreaseStunDamage()
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build()); // 10 stun boxes
            int damageAmount = 3;

            // Act
            Result<ConditionMonitor> result = monitor.TakeStunDamage(damageAmount);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? newMonitor).ShouldBeTrue();
            newMonitor.ShouldNotBeNull();
            newMonitor.StunDamage.ShouldBe(3);
            newMonitor.StunRemaining.ShouldBe(7);
            newMonitor.PhysicalDamage.ShouldBe(0); // Physical damage unchanged
        }

        [Fact]
        public void TakeStunDamage_ExceedingStunBoxes_ShouldOverflowToPhysical()
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build()); // 10 stun boxes
            int excessiveDamage = 13; // 10 to fill stun, 3 overflow to physical

            // Act
            Result<ConditionMonitor> result = monitor.TakeStunDamage(excessiveDamage);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? newMonitor).ShouldBeTrue();
            newMonitor.ShouldNotBeNull();
            newMonitor.StunDamage.ShouldBe(10); // Capped at stun boxes
            newMonitor.StunRemaining.ShouldBe(0);
            newMonitor.PhysicalDamage.ShouldBe(3); // Overflow
            newMonitor.PhysicalRemaining.ShouldBe(7);
            newMonitor.IsUnconscious.ShouldBeTrue();
        }

        [Fact]
        public void TakeStunDamage_ExactlyFillingStunTrack_ShouldNotOverflow()
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build()); // 10 stun boxes
            int exactDamage = 10;

            // Act
            Result<ConditionMonitor> result = monitor.TakeStunDamage(exactDamage);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? newMonitor).ShouldBeTrue();
            newMonitor.ShouldNotBeNull();
            newMonitor.StunDamage.ShouldBe(10);
            newMonitor.StunRemaining.ShouldBe(0);
            newMonitor.PhysicalDamage.ShouldBe(0); // No overflow
            newMonitor.IsUnconscious.ShouldBeTrue();
            newMonitor.IsDying.ShouldBeFalse();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void TakeStunDamage_WithNegativeAmount_ShouldReturnFailure(int negativeDamage)
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build());

            // Act
            Result<ConditionMonitor> result = monitor.TakeStunDamage(negativeDamage);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Damage amount cannot be negative.");
        }

        [Fact]
        public void TakeStunDamage_WithExistingPhysicalDamage_ShouldOverflowCorrectly()
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build()); // 10 physical, 10 stun boxes
            
            // First take some physical damage
            Result<ConditionMonitor> physicalResult = monitor.TakePhysicalDamage(2);
            physicalResult.TryGetValue(out ConditionMonitor? damagedMonitor).ShouldBeTrue();
            
            // Then take stun damage that overflows
            int stunDamage = 12; // 10 to fill stun, 2 overflow to physical

            // Act
            Result<ConditionMonitor> result = damagedMonitor!.TakeStunDamage(stunDamage);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? newMonitor).ShouldBeTrue();
            newMonitor.ShouldNotBeNull();
            newMonitor.StunDamage.ShouldBe(10); // Capped at stun boxes
            newMonitor.PhysicalDamage.ShouldBe(4); // 2 existing + 2 overflow
            newMonitor.IsUnconscious.ShouldBeTrue();
            newMonitor.IsDying.ShouldBeFalse();
        }
    }

    public sealed class HealPhysicalDamage
    {
        [Fact]
        public void HealPhysicalDamage_WithValidAmount_ShouldReducePhysicalDamage()
        {
            // Arrange
            Result<ConditionMonitor> damagedResult = ConditionMonitor.Create(10, 10, 5, 0);
            damagedResult.TryGetValue(out ConditionMonitor? damagedMonitor).ShouldBeTrue();

            // Act
            Result<ConditionMonitor> result = damagedMonitor!.HealPhysicalDamage(2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? healedMonitor).ShouldBeTrue();
            healedMonitor.ShouldNotBeNull();
            healedMonitor.PhysicalDamage.ShouldBe(3);
            healedMonitor.PhysicalRemaining.ShouldBe(7);
            healedMonitor.StunDamage.ShouldBe(0); // Unchanged
        }

        [Fact]
        public void HealPhysicalDamage_ExceedingDamage_ShouldNotGoNegative()
        {
            // Arrange
            Result<ConditionMonitor> damagedResult = ConditionMonitor.Create(10, 10, 3, 0);
            damagedResult.TryGetValue(out ConditionMonitor? damagedMonitor).ShouldBeTrue();

            // Act - Heal more than current damage
            Result<ConditionMonitor> result = damagedMonitor!.HealPhysicalDamage(5);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? healedMonitor).ShouldBeTrue();
            healedMonitor.ShouldNotBeNull();
            healedMonitor.PhysicalDamage.ShouldBe(0); // Capped at 0
            healedMonitor.PhysicalRemaining.ShouldBe(10);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void HealPhysicalDamage_WithNegativeAmount_ShouldReturnFailure(int negativeHeal)
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build());

            // Act
            Result<ConditionMonitor> result = monitor.HealPhysicalDamage(negativeHeal);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Heal amount cannot be negative.");
        }

        [Fact]
        public void HealPhysicalDamage_WithZeroAmount_ShouldSucceedWithNoChange()
        {
            // Arrange
            Result<ConditionMonitor> damagedResult = ConditionMonitor.Create(10, 10, 5, 0);
            damagedResult.TryGetValue(out ConditionMonitor? damagedMonitor).ShouldBeTrue();

            // Act
            Result<ConditionMonitor> result = damagedMonitor!.HealPhysicalDamage(0);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? healedMonitor).ShouldBeTrue();
            healedMonitor.ShouldNotBeNull();
            healedMonitor.PhysicalDamage.ShouldBe(5); // No change
            healedMonitor.StunDamage.ShouldBe(0); // No change
        }
    }

    public sealed class HealStunDamage
    {
        [Fact]
        public void HealStunDamage_WithValidAmount_ShouldReduceStunDamage()
        {
            // Arrange
            Result<ConditionMonitor> damagedResult = ConditionMonitor.Create(10, 10, 0, 6);
            damagedResult.TryGetValue(out ConditionMonitor? damagedMonitor).ShouldBeTrue();

            // Act
            Result<ConditionMonitor> result = damagedMonitor!.HealStunDamage(3);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? healedMonitor).ShouldBeTrue();
            healedMonitor.ShouldNotBeNull();
            healedMonitor.StunDamage.ShouldBe(3);
            healedMonitor.StunRemaining.ShouldBe(7);
            healedMonitor.PhysicalDamage.ShouldBe(0); // Unchanged
        }

        [Fact]
        public void HealStunDamage_ExceedingDamage_ShouldNotGoNegative()
        {
            // Arrange
            Result<ConditionMonitor> damagedResult = ConditionMonitor.Create(10, 10, 0, 4);
            damagedResult.TryGetValue(out ConditionMonitor? damagedMonitor).ShouldBeTrue();

            // Act - Heal more than current damage
            Result<ConditionMonitor> result = damagedMonitor!.HealStunDamage(7);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ConditionMonitor? healedMonitor).ShouldBeTrue();
            healedMonitor.ShouldNotBeNull();
            healedMonitor.StunDamage.ShouldBe(0); // Capped at 0
            healedMonitor.StunRemaining.ShouldBe(10);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void HealStunDamage_WithNegativeAmount_ShouldReturnFailure(int negativeHeal)
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build());

            // Act
            Result<ConditionMonitor> result = monitor.HealStunDamage(negativeHeal);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Heal amount cannot be negative.");
        }
    }

    public sealed class HealAll
    {
        [Fact]
        public void HealAll_WithDamage_ShouldResetBothDamageTracksToZero()
        {
            // Arrange
            Result<ConditionMonitor> damagedResult = ConditionMonitor.Create(10, 11, 5, 7);
            damagedResult.TryGetValue(out ConditionMonitor? damagedMonitor).ShouldBeTrue();

            // Act
            ConditionMonitor healedMonitor = damagedMonitor!.HealAll();

            // Assert
            healedMonitor.PhysicalDamage.ShouldBe(0);
            healedMonitor.StunDamage.ShouldBe(0);
            healedMonitor.PhysicalRemaining.ShouldBe(10);
            healedMonitor.StunRemaining.ShouldBe(11);
            healedMonitor.IsUnconscious.ShouldBeFalse();
            healedMonitor.IsDying.ShouldBeFalse();
        }

        [Fact]
        public void HealAll_WithNoDamage_ShouldRemainUnchanged()
        {
            // Arrange
            ConditionMonitor healthyMonitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build());

            // Act
            ConditionMonitor healedMonitor = healthyMonitor.HealAll();

            // Assert
            healedMonitor.PhysicalDamage.ShouldBe(0);
            healedMonitor.StunDamage.ShouldBe(0);
            healedMonitor.PhysicalRemaining.ShouldBe(healthyMonitor.PhysicalRemaining);
            healedMonitor.StunRemaining.ShouldBe(healthyMonitor.StunRemaining);
        }
    }

    public sealed class HealthStateCalculations
    {
        [Fact]
        public void IsUnconscious_WhenStunTrackFull_ShouldReturnTrue()
        {
            // Arrange
            Result<ConditionMonitor> result = ConditionMonitor.Create(10, 8, 0, 8); // Stun track full

            // Act & Assert
            result.TryGetValue(out ConditionMonitor? monitor).ShouldBeTrue();
            monitor!.IsUnconscious.ShouldBeTrue();
            monitor.IsDying.ShouldBeFalse();
        }

        [Fact]
        public void IsUnconscious_WhenStunTrackNotFull_ShouldReturnFalse()
        {
            // Arrange
            Result<ConditionMonitor> result = ConditionMonitor.Create(10, 8, 0, 7); // One stun box remaining

            // Act & Assert
            result.TryGetValue(out ConditionMonitor? monitor).ShouldBeTrue();
            monitor!.IsUnconscious.ShouldBeFalse();
        }

        [Fact]
        public void IsDying_WhenPhysicalTrackFull_ShouldReturnTrue()
        {
            // Arrange
            Result<ConditionMonitor> result = ConditionMonitor.Create(9, 10, 9, 0); // Physical track full

            // Act & Assert
            result.TryGetValue(out ConditionMonitor? monitor).ShouldBeTrue();
            monitor!.IsDying.ShouldBeTrue();
            monitor.IsUnconscious.ShouldBeFalse();
        }

        [Fact]
        public void IsDying_WhenPhysicalTrackNotFull_ShouldReturnFalse()
        {
            // Arrange
            Result<ConditionMonitor> result = ConditionMonitor.Create(9, 10, 8, 0); // One physical box remaining

            // Act & Assert
            result.TryGetValue(out ConditionMonitor? monitor).ShouldBeTrue();
            monitor!.IsDying.ShouldBeFalse();
        }

        [Fact]
        public void BothStates_CanBeTrueSimultaneously()
        {
            // Arrange
            Result<ConditionMonitor> result = ConditionMonitor.Create(8, 8, 8, 8); // Both tracks full

            // Act & Assert
            result.TryGetValue(out ConditionMonitor? monitor).ShouldBeTrue();
            monitor!.IsUnconscious.ShouldBeTrue();
            monitor.IsDying.ShouldBeTrue();
        }

        [Theory]
        [InlineData(0, 0, 0)] // No damage = no modifier
        [InlineData(2, 1, 0)] // 2 physical + 1 stun = 3 total, 3/3 = 1, but -1 dice
        [InlineData(3, 0, -1)] // 3 physical = -1 dice
        [InlineData(0, 3, -1)] // 3 stun = -1 dice
        [InlineData(6, 0, -2)] // 6 physical = -2 dice
        [InlineData(0, 6, -2)] // 6 stun = -2 dice
        [InlineData(3, 6, -3)] // 3 physical + 6 stun = 9 total, 9/3 = 3, -3 dice
        [InlineData(9, 9, -6)] // 9 physical + 9 stun = 18 total, 18/3 = 6, -6 dice
        public void WoundModifier_ShouldCalculateCorrectly(int physicalDamage, int stunDamage, int expectedModifier)
        {
            // Arrange
            Result<ConditionMonitor> result = ConditionMonitor.Create(15, 15, physicalDamage, stunDamage);

            // Act & Assert
            result.TryGetValue(out ConditionMonitor? monitor).ShouldBeTrue();
            monitor!.WoundModifier.ShouldBe(expectedModifier);
        }

        [Theory]
        [InlineData(10, 5)] // 10 boxes - 5 damage = 5 remaining
        [InlineData(8, 0)] // 8 boxes - 0 damage = 8 remaining
        [InlineData(10, 15)] // 10 boxes - 15 damage = 0 remaining (can't go negative)
        public void PhysicalRemaining_ShouldCalculateCorrectly(int physicalBoxes, int physicalDamage)
        {
            // Arrange
            Result<ConditionMonitor> result = ConditionMonitor.Create(physicalBoxes, 10, physicalDamage, 0);
            int expectedRemaining = Math.Max(0, physicalBoxes - physicalDamage);

            // Act & Assert
            result.TryGetValue(out ConditionMonitor? monitor).ShouldBeTrue();
            monitor!.PhysicalRemaining.ShouldBe(expectedRemaining);
        }

        [Theory]
        [InlineData(10, 3)] // 10 boxes - 3 damage = 7 remaining
        [InlineData(8, 0)] // 8 boxes - 0 damage = 8 remaining
        [InlineData(10, 12)] // 10 boxes - 12 damage = 0 remaining (can't go negative)
        public void StunRemaining_ShouldCalculateCorrectly(int stunBoxes, int stunDamage)
        {
            // Arrange
            Result<ConditionMonitor> result = ConditionMonitor.Create(10, stunBoxes, 0, stunDamage);
            int expectedRemaining = Math.Max(0, stunBoxes - stunDamage);

            // Act & Assert
            result.TryGetValue(out ConditionMonitor? monitor).ShouldBeTrue();
            monitor!.StunRemaining.ShouldBe(expectedRemaining);
        }
    }

    public sealed class ValueObjectBehavior
    {
        [Fact]
        public void TwoConditionMonitors_WithSameValues_ShouldBeEqual()
        {
            // Arrange
            Result<ConditionMonitor> first = ConditionMonitor.Create(10, 11, 3, 2);
            Result<ConditionMonitor> second = ConditionMonitor.Create(10, 11, 3, 2);

            // Act & Assert
            first.TryGetValue(out ConditionMonitor? firstMonitor).ShouldBeTrue();
            second.TryGetValue(out ConditionMonitor? secondMonitor).ShouldBeTrue();
            
            firstMonitor.ShouldBe(secondMonitor);
            firstMonitor!.GetHashCode().ShouldBe(secondMonitor!.GetHashCode());
        }

        [Fact]
        public void TwoConditionMonitors_WithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            Result<ConditionMonitor> first = ConditionMonitor.Create(10, 11, 3, 2);
            Result<ConditionMonitor> second = ConditionMonitor.Create(10, 11, 4, 2); // Different physical damage

            // Act & Assert
            first.TryGetValue(out ConditionMonitor? firstMonitor).ShouldBeTrue();
            second.TryGetValue(out ConditionMonitor? secondMonitor).ShouldBeTrue();
            
            firstMonitor.ShouldNotBe(secondMonitor);
        }

        [Fact]
        public void ConditionMonitor_ShouldBeImmutable()
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build());

            // Act & Assert - Value object should have no public setters
            typeof(ConditionMonitor).GetProperty("PhysicalBoxes")?.SetMethod.ShouldBeNull();
            typeof(ConditionMonitor).GetProperty("StunBoxes")?.SetMethod.ShouldBeNull();
            typeof(ConditionMonitor).GetProperty("PhysicalDamage")?.SetMethod.ShouldBeNull();
            typeof(ConditionMonitor).GetProperty("StunDamage")?.SetMethod.ShouldBeNull();
        }

        [Fact]
        public void ToString_ShouldProvideReadableFormat()
        {
            // Arrange
            Result<ConditionMonitor> result = ConditionMonitor.Create(10, 11, 3, 2);
            result.TryGetValue(out ConditionMonitor? monitor).ShouldBeTrue();

            // Act
            string stringRepresentation = monitor!.ToString();

            // Assert
            stringRepresentation.ShouldBe("Physical: 7/10, Stun: 9/11");
        }

        [Fact]
        public void ConditionMonitor_WithNullComparison_ShouldNotBeEqual()
        {
            // Arrange
            ConditionMonitor monitor = ConditionMonitor.ForAttributes(
                new AttributeSetBuilder().Build());

            // Act & Assert
            monitor.ShouldNotBe(null);
            monitor.Equals(null).ShouldBeFalse();
        }
    }
}