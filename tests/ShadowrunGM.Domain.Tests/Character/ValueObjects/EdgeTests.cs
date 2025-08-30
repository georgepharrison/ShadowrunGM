using ShadowrunGM.Domain.Character.ValueObjects;

namespace ShadowrunGM.Domain.Tests.Character.ValueObjects;

/// <summary>
/// Comprehensive test suite for Edge value object behavior.
/// Tests creation, spend/regain/burn mechanics, validation, and immutability.
/// </summary>
public sealed class EdgeTests
{
    public sealed class Create
    {
        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(7)]
        public void Create_WithValidStartingEdge_ShouldSetCurrentAndMaxToSameValue(int startingEdge)
        {
            // Act
            Edge edge = Edge.Create(startingEdge);

            // Assert
            edge.Current.ShouldBe(startingEdge);
            edge.Max.ShouldBe(startingEdge);
            edge.HasEdge.ShouldBeTrue();
            edge.IsAtMax.ShouldBeTrue();
        }

        [Fact]
        public void Create_WithMinimumEdge_ShouldSucceed()
        {
            // Act
            Edge edge = Edge.Create(1);

            // Assert
            edge.Current.ShouldBe(1);
            edge.Max.ShouldBe(1);
            edge.HasEdge.ShouldBeTrue();
            edge.IsAtMax.ShouldBeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Create_WithInvalidStartingEdge_ShouldThrowException(int invalidStartingEdge)
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => Edge.Create(invalidStartingEdge))
                .Message.ShouldContain("Maximum Edge must be at least 1");
        }
    }

    public sealed class CreateWithValues
    {
        [Theory]
        [InlineData(2, 5)]
        [InlineData(0, 3)]
        [InlineData(7, 7)]
        public void CreateWithValues_WithValidCurrentAndMax_ShouldSetValuesCorrectly(int current, int max)
        {
            // Act
            Edge edge = Edge.CreateWithValues(current, max);

            // Assert
            edge.Current.ShouldBe(current);
            edge.Max.ShouldBe(max);
            edge.HasEdge.ShouldBe(current > 0);
            edge.IsAtMax.ShouldBe(current == max);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void CreateWithValues_WithInvalidMax_ShouldThrowException(int invalidMax)
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => Edge.CreateWithValues(0, invalidMax))
                .Message.ShouldContain("Maximum Edge must be at least 1");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void CreateWithValues_WithNegativeCurrent_ShouldThrowException(int negativeCurrent)
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => Edge.CreateWithValues(negativeCurrent, 3))
                .Message.ShouldContain("Current Edge cannot be negative");
        }

        [Fact]
        public void CreateWithValues_WithCurrentExceedingMax_ShouldThrowException()
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() => Edge.CreateWithValues(5, 3))
                .Message.ShouldContain("Current Edge cannot exceed maximum Edge");
        }

        [Fact]
        public void CreateWithValues_WithZeroCurrent_ShouldIndicateNoEdgeAvailable()
        {
            // Act
            Edge edge = Edge.CreateWithValues(0, 3);

            // Assert
            edge.Current.ShouldBe(0);
            edge.Max.ShouldBe(3);
            edge.HasEdge.ShouldBeFalse();
            edge.IsAtMax.ShouldBeFalse();
        }
    }

    public sealed class Spend
    {
        [Fact]
        public void Spend_WithValidAmount_ShouldReduceCurrentEdge()
        {
            // Arrange
            Edge edge = Edge.Create(5);

            // Act
            Result<Edge> result = edge.Spend(2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(3);
            newEdge.Max.ShouldBe(5); // Max unchanged
            newEdge.HasEdge.ShouldBeTrue();
            newEdge.IsAtMax.ShouldBeFalse();
        }

        [Fact]
        public void Spend_AllAvailableEdge_ShouldResultInZeroCurrent()
        {
            // Arrange
            Edge edge = Edge.Create(3);

            // Act
            Result<Edge> result = edge.Spend(3);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(0);
            newEdge.Max.ShouldBe(3);
            newEdge.HasEdge.ShouldBeFalse();
            newEdge.IsAtMax.ShouldBeFalse();
        }

        [Fact]
        public void Spend_MoreThanAvailable_ShouldReturnFailure()
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(2, 5);

            // Act
            Result<Edge> result = edge.Spend(3);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Cannot spend 3 Edge. Only 2 available.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void Spend_WithZeroOrNegativeAmount_ShouldReturnFailure(int invalidAmount)
        {
            // Arrange
            Edge edge = Edge.Create(5);

            // Act
            Result<Edge> result = edge.Spend(invalidAmount);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Spend amount must be positive.");
        }

        [Fact]
        public void Spend_WhenNoEdgeAvailable_ShouldReturnFailure()
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(0, 3);

            // Act
            Result<Edge> result = edge.Spend(1);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Cannot spend 1 Edge. Only 0 available.");
        }

        [Fact]
        public void Spend_ShouldBeImmutable_OriginalEdgeUnchanged()
        {
            // Arrange
            Edge originalEdge = Edge.Create(5);

            // Act
            Result<Edge> result = originalEdge.Spend(2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            
            // Original edge should remain unchanged
            originalEdge.Current.ShouldBe(5);
            originalEdge.Max.ShouldBe(5);
            
            // New edge should have the changes
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge!.Current.ShouldBe(3);
            newEdge.Max.ShouldBe(5);
        }
    }

    public sealed class Regain
    {
        [Fact]
        public void Regain_WithValidAmount_ShouldIncreaseCurrentEdge()
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(2, 5);

            // Act
            Result<Edge> result = edge.Regain(2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(4);
            newEdge.Max.ShouldBe(5); // Max unchanged
            newEdge.HasEdge.ShouldBeTrue();
            newEdge.IsAtMax.ShouldBeFalse();
        }

        [Fact]
        public void Regain_ToMaximum_ShouldCapAtMaxValue()
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(3, 5);

            // Act
            Result<Edge> result = edge.Regain(5); // More than max - 3 = 2

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(5); // Capped at max
            newEdge.Max.ShouldBe(5);
            newEdge.HasEdge.ShouldBeTrue();
            newEdge.IsAtMax.ShouldBeTrue();
        }

        [Fact]
        public void Regain_ExactlyToMaximum_ShouldSetToMax()
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(2, 5);

            // Act
            Result<Edge> result = edge.Regain(3);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(5);
            newEdge.Max.ShouldBe(5);
            newEdge.IsAtMax.ShouldBeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void Regain_WithZeroOrNegativeAmount_ShouldReturnFailure(int invalidAmount)
        {
            // Arrange
            Edge edge = Edge.Create(3);

            // Act
            Result<Edge> result = edge.Regain(invalidAmount);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Regain amount must be positive.");
        }

        [Fact]
        public void Regain_WhenAlreadyAtMax_ShouldRemainAtMax()
        {
            // Arrange
            Edge edge = Edge.Create(5); // Already at max

            // Act
            Result<Edge> result = edge.Regain(2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(5); // Still at max
            newEdge.Max.ShouldBe(5);
            newEdge.IsAtMax.ShouldBeTrue();
        }

        [Fact]
        public void Regain_FromZeroCurrent_ShouldWork()
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(0, 4);

            // Act
            Result<Edge> result = edge.Regain(2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(2);
            newEdge.Max.ShouldBe(4);
            newEdge.HasEdge.ShouldBeTrue();
        }
    }

    public sealed class Refresh
    {
        [Fact]
        public void Refresh_ShouldSetCurrentToMax()
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(2, 6);

            // Act
            Edge refreshedEdge = edge.Refresh();

            // Assert
            refreshedEdge.Current.ShouldBe(6);
            refreshedEdge.Max.ShouldBe(6);
            refreshedEdge.HasEdge.ShouldBeTrue();
            refreshedEdge.IsAtMax.ShouldBeTrue();
        }

        [Fact]
        public void Refresh_WhenAlreadyAtMax_ShouldRemainUnchanged()
        {
            // Arrange
            Edge edge = Edge.Create(4); // Already at max

            // Act
            Edge refreshedEdge = edge.Refresh();

            // Assert
            refreshedEdge.Current.ShouldBe(4);
            refreshedEdge.Max.ShouldBe(4);
            refreshedEdge.IsAtMax.ShouldBeTrue();
        }

        [Fact]
        public void Refresh_FromZeroCurrent_ShouldRestoreToMax()
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(0, 5);

            // Act
            Edge refreshedEdge = edge.Refresh();

            // Assert
            refreshedEdge.Current.ShouldBe(5);
            refreshedEdge.Max.ShouldBe(5);
            refreshedEdge.HasEdge.ShouldBeTrue();
            refreshedEdge.IsAtMax.ShouldBeTrue();
        }

        [Fact]
        public void Refresh_ShouldBeImmutable_OriginalEdgeUnchanged()
        {
            // Arrange
            Edge originalEdge = Edge.CreateWithValues(2, 5);

            // Act
            Edge refreshedEdge = originalEdge.Refresh();

            // Assert
            // Original edge should remain unchanged
            originalEdge.Current.ShouldBe(2);
            originalEdge.Max.ShouldBe(5);
            
            // Refreshed edge should be at max
            refreshedEdge.Current.ShouldBe(5);
            refreshedEdge.Max.ShouldBe(5);
        }
    }

    public sealed class Burn
    {
        [Fact]
        public void Burn_WithValidAmount_ShouldReduceBothCurrentAndMax()
        {
            // Arrange
            Edge edge = Edge.Create(5);

            // Act
            Result<Edge> result = edge.Burn(2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(3); // Current adjusted down
            newEdge.Max.ShouldBe(3); // Max reduced
            newEdge.HasEdge.ShouldBeTrue();
            newEdge.IsAtMax.ShouldBeTrue();
        }

        [Fact]
        public void Burn_WithDefaultAmount_ShouldBurnOnePoint()
        {
            // Arrange
            Edge edge = Edge.Create(4);

            // Act
            Result<Edge> result = edge.Burn(); // Default amount = 1

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(3);
            newEdge.Max.ShouldBe(3);
            newEdge.IsAtMax.ShouldBeTrue();
        }

        [Fact]
        public void Burn_WhenCurrentLowerThanMax_ShouldAdjustCurrentToMax()
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(2, 5); // Current < Max

            // Act
            Result<Edge> result = edge.Burn(2);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(2); // Min(2, 3) = 2
            newEdge.Max.ShouldBe(3); // 5 - 2 = 3
            newEdge.HasEdge.ShouldBeTrue();
            newEdge.IsAtMax.ShouldBeTrue();
        }

        [Fact]
        public void Burn_WhenCurrentHigherThanNewMax_ShouldCapCurrentAtNewMax()
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(4, 6);

            // Act
            Result<Edge> result = edge.Burn(3); // NewMax = 6 - 3 = 3

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(3); // Min(4, 3) = 3
            newEdge.Max.ShouldBe(3);
            newEdge.IsAtMax.ShouldBeTrue();
        }

        [Fact]
        public void Burn_MoreThanMaxAvailable_ShouldReturnFailure()
        {
            // Arrange
            Edge edge = Edge.Create(3);

            // Act
            Result<Edge> result = edge.Burn(4); // Can't burn more than max

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Cannot burn 4 Edge. Maximum is only 3.");
        }

        [Fact]
        public void Burn_ToReduceMaxBelowOne_ShouldReturnFailure()
        {
            // Arrange
            Edge edge = Edge.Create(2);

            // Act
            Result<Edge> result = edge.Burn(2); // Would leave max = 0

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Cannot burn Edge below 1.");
        }

        [Fact]
        public void Burn_ExactlyToOneMaximum_ShouldSucceed()
        {
            // Arrange
            Edge edge = Edge.Create(3);

            // Act
            Result<Edge> result = edge.Burn(2); // Max becomes 1

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Edge? newEdge).ShouldBeTrue();
            newEdge.ShouldNotBeNull();
            newEdge.Current.ShouldBe(1);
            newEdge.Max.ShouldBe(1);
            newEdge.HasEdge.ShouldBeTrue();
            newEdge.IsAtMax.ShouldBeTrue();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void Burn_WithZeroOrNegativeAmount_ShouldReturnFailure(int invalidAmount)
        {
            // Arrange
            Edge edge = Edge.Create(5);

            // Act
            Result<Edge> result = edge.Burn(invalidAmount);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Burn amount must be positive.");
        }
    }

    public sealed class Properties
    {
        [Theory]
        [InlineData(0, false)]
        [InlineData(1, true)]
        [InlineData(5, true)]
        public void HasEdge_ShouldReflectCurrentEdgeValue(int current, bool expectedHasEdge)
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(current, 5);

            // Act & Assert
            edge.HasEdge.ShouldBe(expectedHasEdge);
        }

        [Theory]
        [InlineData(0, 5, false)]
        [InlineData(3, 5, false)]
        [InlineData(5, 5, true)]
        public void IsAtMax_ShouldReflectWhetherCurrentEqualsMax(int current, int max, bool expectedIsAtMax)
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(current, max);

            // Act & Assert
            edge.IsAtMax.ShouldBe(expectedIsAtMax);
        }
    }

    public sealed class ValueObjectBehavior
    {
        [Fact]
        public void TwoEdges_WithSameValues_ShouldBeEqual()
        {
            // Arrange
            Edge first = Edge.CreateWithValues(3, 5);
            Edge second = Edge.CreateWithValues(3, 5);

            // Act & Assert
            first.ShouldBe(second);
            first.GetHashCode().ShouldBe(second.GetHashCode());
        }

        [Fact]
        public void TwoEdges_WithDifferentCurrentValues_ShouldNotBeEqual()
        {
            // Arrange
            Edge first = Edge.CreateWithValues(2, 5);
            Edge second = Edge.CreateWithValues(3, 5);

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void TwoEdges_WithDifferentMaxValues_ShouldNotBeEqual()
        {
            // Arrange
            Edge first = Edge.CreateWithValues(3, 4);
            Edge second = Edge.CreateWithValues(3, 5);

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void Edge_ShouldBeImmutable()
        {
            // Arrange
            Edge edge = Edge.Create(5);

            // Act & Assert - Value object should have no public setters
            typeof(Edge).GetProperty("Current")?.SetMethod.ShouldBeNull();
            typeof(Edge).GetProperty("Max")?.SetMethod.ShouldBeNull();
        }

        [Fact]
        public void ToString_ShouldProvideReadableFormat()
        {
            // Arrange
            Edge edge = Edge.CreateWithValues(3, 5);

            // Act
            string stringRepresentation = edge.ToString();

            // Assert
            stringRepresentation.ShouldBe("3/5");
        }

        [Fact]
        public void ToString_WhenAtMax_ShouldShowSameValues()
        {
            // Arrange
            Edge edge = Edge.Create(4);

            // Act
            string stringRepresentation = edge.ToString();

            // Assert
            stringRepresentation.ShouldBe("4/4");
        }

        [Fact]
        public void Edge_WithNullComparison_ShouldNotBeEqual()
        {
            // Arrange
            Edge edge = Edge.Create(3);

            // Act & Assert
            edge.ShouldNotBe(null);
            edge.Equals(null).ShouldBeFalse();
        }

        [Fact]
        public void Edge_WithDifferentType_ShouldNotBeEqual()
        {
            // Arrange
            Edge edge = Edge.Create(3);
            object differentType = "not an edge";

            // Act & Assert
            edge.Equals(differentType).ShouldBeFalse();
        }
    }

    public sealed class EdgeSequenceScenarios
    {
        [Fact]
        public void SpendRefreshSequence_ShouldWorkCorrectly()
        {
            // Arrange
            Edge edge = Edge.Create(5);

            // Act - Spend then refresh
            Result<Edge> spentResult = edge.Spend(3);
            spentResult.TryGetValue(out Edge? spentEdge).ShouldBeTrue();
            Edge refreshedEdge = spentEdge!.Refresh();

            // Assert
            refreshedEdge.Current.ShouldBe(5); // Back to max
            refreshedEdge.Max.ShouldBe(5);
            refreshedEdge.IsAtMax.ShouldBeTrue();
        }

        [Fact]
        public void SpendBurnSequence_ShouldWorkCorrectly()
        {
            // Arrange
            Edge edge = Edge.Create(6);

            // Act - Spend then burn
            Result<Edge> spentResult = edge.Spend(2); // 4/6
            spentResult.TryGetValue(out Edge? spentEdge).ShouldBeTrue();
            
            Result<Edge> burnedResult = spentEdge!.Burn(2); // Should become 2/4, but current was 4, so Min(4,4) = 4

            // Assert
            burnedResult.IsSuccess.ShouldBeTrue();
            burnedResult.TryGetValue(out Edge? burnedEdge).ShouldBeTrue();
            burnedEdge.ShouldNotBeNull();
            burnedEdge.Current.ShouldBe(4); // Min(4, 4)
            burnedEdge.Max.ShouldBe(4); // 6 - 2
            burnedEdge.IsAtMax.ShouldBeTrue();
        }

        [Fact]
        public void ComplexEdgeManipulation_ShouldMaintainInvariants()
        {
            // Arrange
            Edge edge = Edge.Create(7);

            // Act - Complex sequence
            Result<Edge> step1 = edge.Spend(3); // 4/7
            step1.TryGetValue(out Edge? edge1).ShouldBeTrue();
            
            Result<Edge> step2 = edge1!.Regain(1); // 5/7
            step2.TryGetValue(out Edge? edge2).ShouldBeTrue();
            
            Result<Edge> step3 = edge2!.Burn(2); // 5/5
            step3.TryGetValue(out Edge? edge3).ShouldBeTrue();
            
            Result<Edge> step4 = edge3!.Spend(2); // 3/5
            step4.TryGetValue(out Edge? finalEdge).ShouldBeTrue();

            // Assert - All invariants maintained throughout
            finalEdge.ShouldNotBeNull();
            finalEdge.Current.ShouldBe(3);
            finalEdge.Max.ShouldBe(5);
            finalEdge.HasEdge.ShouldBeTrue();
            finalEdge.IsAtMax.ShouldBeFalse();
            
            // Original edge should be unchanged
            edge.Current.ShouldBe(7);
            edge.Max.ShouldBe(7);
        }
    }
}