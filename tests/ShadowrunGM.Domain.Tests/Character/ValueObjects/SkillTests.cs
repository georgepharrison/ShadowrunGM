using FlowRight.Core.Results;
using ShadowrunGM.Domain.Character.ValueObjects;
using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Tests.Character.ValueObjects;

/// <summary>
/// Comprehensive tests for the Skill value object covering all business rules and operations.
/// Tests follow TDD principles - these tests FAIL first to define expected behavior.
/// </summary>
public sealed class SkillTests
{
    public sealed class Constructor
    {
        [Fact]
        public void Create_WithValidNameAndRating_ShouldSucceed()
        {
            // Arrange
            string skillName = "Firearms";
            int rating = 4;

            // Act
            Result<Skill> result = Skill.Create(skillName, rating);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();
            skill.Name.ShouldBe(skillName);
            skill.Rating.ShouldBe(rating);
            skill.Specialization.ShouldBeNull();
            skill.HasSpecialization.ShouldBeFalse();
        }

        [Fact]
        public void Create_WithValidNameRatingAndSpecialization_ShouldSucceed()
        {
            // Arrange
            string skillName = "Firearms";
            int rating = 4;
            string specialization = "Pistols";

            // Act
            Result<Skill> result = Skill.Create(skillName, rating, specialization);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();
            skill.Name.ShouldBe(skillName);
            skill.Rating.ShouldBe(rating);
            skill.Specialization.ShouldBe(specialization);
            skill.HasSpecialization.ShouldBeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Create_WithInvalidName_ShouldFail(string? invalidName)
        {
            // Act
            Result<Skill> result = Skill.Create(invalidName!, 4);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("name");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(13)]
        [InlineData(999)]
        public void Create_WithInvalidRating_ShouldFail(int invalidRating)
        {
            // Act
            Result<Skill> result = Skill.Create("Valid Skill", invalidRating);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("rating");
        }

        [Fact]
        public void Create_WithTooLongName_ShouldFail()
        {
            // Arrange
            string longName = new('A', 51);

            // Act
            Result<Skill> result = Skill.Create(longName, 4);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("name");
        }

        [Fact]
        public void Create_WithTooLongSpecialization_ShouldFail()
        {
            // Arrange
            string longSpecialization = new('A', 51);

            // Act
            Result<Skill> result = Skill.Create("Valid Skill", 4, longSpecialization);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("Specialization");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public void Create_WithEmptyOrWhitespaceSpecialization_ShouldTreatAsNull(string emptySpecialization)
        {
            // Act
            Result<Skill> result = Skill.Create("Firearms", 4, emptySpecialization);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();
            skill.Specialization.ShouldBeNull();
            skill.HasSpecialization.ShouldBeFalse();
        }

        [Theory]
        [InlineData("Firearms ", "Firearms")]
        [InlineData(" Athletics", "Athletics")]
        [InlineData(" Hacking ", "Hacking")]
        public void Create_ShouldTrimNameWhitespace(string nameWithWhitespace, string expectedTrimmedName)
        {
            // Act
            Result<Skill> result = Skill.Create(nameWithWhitespace, 4);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();
            skill.Name.ShouldBe(expectedTrimmedName);
        }

        [Theory]
        [InlineData("Pistols ", "Pistols")]
        [InlineData(" Rifles", "Rifles")]
        [InlineData(" Shotguns ", "Shotguns")]
        public void Create_ShouldTrimSpecializationWhitespace(string specializationWithWhitespace, string expectedTrimmedSpecialization)
        {
            // Act
            Result<Skill> result = Skill.Create("Firearms", 4, specializationWithWhitespace);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();
            skill.Specialization.ShouldBe(expectedTrimmedSpecialization);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(6)]
        [InlineData(12)]
        public void Create_WithValidRatingRange_ShouldSucceed(int validRating)
        {
            // Act
            Result<Skill> result = Skill.Create("Test Skill", validRating);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();
            skill.Rating.ShouldBe(validRating);
        }
    }

    public sealed class DicePoolCalculation
    {
        [Fact]
        public void GetDicePool_WithoutSpecializationUsage_ShouldReturnBaseRating()
        {
            // Arrange
            Result<Skill> skillResult = Skill.Create("Firearms", 4, "Pistols");
            skillResult.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();

            // Act
            int dicePool = skill.GetDicePool(isUsingSpecialization: false);

            // Assert
            dicePool.ShouldBe(4);
        }

        [Fact]
        public void GetDicePool_WithSpecializationUsage_ShouldReturnRatingPlusTwo()
        {
            // Arrange
            Result<Skill> skillResult = Skill.Create("Firearms", 4, "Pistols");
            skillResult.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();

            // Act
            int dicePool = skill.GetDicePool(isUsingSpecialization: true);

            // Assert
            dicePool.ShouldBe(6);
        }

        [Fact]
        public void GetDicePool_WithoutSpecialization_ShouldIgnoreSpecializationFlag()
        {
            // Arrange
            Result<Skill> skillResult = Skill.Create("Athletics", 3);
            skillResult.TryGetValue(out Skill? skillWithoutSpecialization).ShouldBeTrue();
            skillWithoutSpecialization.ShouldNotBeNull();

            // Act
            int dicePoolWithFlag = skillWithoutSpecialization.GetDicePool(isUsingSpecialization: true);
            int dicePoolWithoutFlag = skillWithoutSpecialization.GetDicePool(isUsingSpecialization: false);

            // Assert
            dicePoolWithFlag.ShouldBe(3);
            dicePoolWithoutFlag.ShouldBe(3);
            dicePoolWithFlag.ShouldBe(dicePoolWithoutFlag);
        }

        [Theory]
        [InlineData(1, false, 1)]
        [InlineData(1, true, 3)]
        [InlineData(4, false, 4)]
        [InlineData(4, true, 6)]
        [InlineData(6, false, 6)]
        [InlineData(6, true, 8)]
        [InlineData(12, false, 12)]
        [InlineData(12, true, 14)]
        public void GetDicePool_WithDifferentRatings_ShouldCalculateCorrectly(int rating, bool useSpecialization, int expectedDicePool)
        {
            // Arrange
            Result<Skill> skillResult = Skill.Create("Test Skill", rating, "Specialization");
            skillResult.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();

            // Act
            int dicePool = skill.GetDicePool(useSpecialization);

            // Assert
            dicePool.ShouldBe(expectedDicePool);
        }
    }

    public sealed class WithMethods
    {
        [Fact]
        public void WithRating_WithValidRating_ShouldCreateNewSkillWithUpdatedRating()
        {
            // Arrange
            Result<Skill> originalResult = Skill.Create("Firearms", 4, "Pistols");
            originalResult.TryGetValue(out Skill? originalSkill).ShouldBeTrue();
            originalSkill.ShouldNotBeNull();

            // Act
            Result<Skill> result = originalSkill.WithRating(6);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Skill? newSkill).ShouldBeTrue();
            newSkill.ShouldNotBeNull();
            newSkill.Name.ShouldBe(originalSkill.Name);
            newSkill.Rating.ShouldBe(6);
            newSkill.Specialization.ShouldBe(originalSkill.Specialization);
            newSkill.ShouldNotBeSameAs(originalSkill);
        }

        [Fact]
        public void WithSpecialization_WithValidSpecialization_ShouldCreateNewSkillWithUpdatedSpecialization()
        {
            // Arrange
            Result<Skill> originalResult = Skill.Create("Firearms", 4);
            originalResult.TryGetValue(out Skill? originalSkill).ShouldBeTrue();
            originalSkill.ShouldNotBeNull();

            // Act
            Result<Skill> result = originalSkill.WithSpecialization("Pistols");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Skill? newSkill).ShouldBeTrue();
            newSkill.ShouldNotBeNull();
            newSkill.Name.ShouldBe(originalSkill.Name);
            newSkill.Rating.ShouldBe(originalSkill.Rating);
            newSkill.Specialization.ShouldBe("Pistols");
            newSkill.HasSpecialization.ShouldBeTrue();
            newSkill.ShouldNotBeSameAs(originalSkill);
        }

        [Fact]
        public void WithSpecialization_WithNull_ShouldRemoveSpecialization()
        {
            // Arrange
            Result<Skill> originalResult = Skill.Create("Firearms", 4, "Pistols");
            originalResult.TryGetValue(out Skill? originalSkill).ShouldBeTrue();
            originalSkill.ShouldNotBeNull();

            // Act
            Result<Skill> result = originalSkill.WithSpecialization(null);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Skill? newSkill).ShouldBeTrue();
            newSkill.ShouldNotBeNull();
            newSkill.Specialization.ShouldBeNull();
            newSkill.HasSpecialization.ShouldBeFalse();
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(13)]
        public void WithRating_WithInvalidRating_ShouldFail(int invalidRating)
        {
            // Arrange
            Result<Skill> originalResult = Skill.Create("Firearms", 4);
            originalResult.TryGetValue(out Skill? originalSkill).ShouldBeTrue();
            originalSkill.ShouldNotBeNull();

            // Act
            Result<Skill> result = originalSkill.WithRating(invalidRating);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("rating");
        }

        [Fact]
        public void WithSpecialization_WithTooLongSpecialization_ShouldFail()
        {
            // Arrange
            Result<Skill> originalResult = Skill.Create("Firearms", 4);
            originalResult.TryGetValue(out Skill? originalSkill).ShouldBeTrue();
            originalSkill.ShouldNotBeNull();
            string longSpecialization = new('A', 51);

            // Act
            Result<Skill> result = originalSkill.WithSpecialization(longSpecialization);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("Specialization");
        }
    }

    public sealed class ValueObjectBehavior
    {
        [Fact]
        public void Skill_ShouldInheritFromValueObject()
        {
            // Arrange & Act
            Result<Skill> skillResult = Skill.Create("Test Skill", 4);
            skillResult.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();

            // Assert
            skill.ShouldBeAssignableTo<ValueObject>();
        }

        [Fact]
        public void Equality_WithSameValues_ShouldBeEqual()
        {
            // Arrange
            Result<Skill> skill1Result = Skill.Create("Firearms", 4, "Pistols");
            skill1Result.TryGetValue(out Skill? skill1).ShouldBeTrue();
            skill1.ShouldNotBeNull();
            
            Result<Skill> skill2Result = Skill.Create("Firearms", 4, "Pistols");
            skill2Result.TryGetValue(out Skill? skill2).ShouldBeTrue();
            skill2.ShouldNotBeNull();

            // Act & Assert
            skill1.ShouldBe(skill2);
            skill1.GetHashCode().ShouldBe(skill2.GetHashCode());
            (skill1 == skill2).ShouldBeTrue();
        }

        [Fact]
        public void Equality_WithDifferentName_ShouldNotBeEqual()
        {
            // Arrange
            Result<Skill> skill1Result = Skill.Create("Firearms", 4);
            skill1Result.TryGetValue(out Skill? skill1).ShouldBeTrue();
            skill1.ShouldNotBeNull();
            
            Result<Skill> skill2Result = Skill.Create("Athletics", 4);
            skill2Result.TryGetValue(out Skill? skill2).ShouldBeTrue();
            skill2.ShouldNotBeNull();

            // Act & Assert
            skill1.ShouldNotBe(skill2);
            (skill1 == skill2).ShouldBeFalse();
        }

        [Fact]
        public void Equality_WithDifferentRating_ShouldNotBeEqual()
        {
            // Arrange
            Result<Skill> skill1Result = Skill.Create("Firearms", 4);
            skill1Result.TryGetValue(out Skill? skill1).ShouldBeTrue();
            skill1.ShouldNotBeNull();
            
            Result<Skill> skill2Result = Skill.Create("Firearms", 6);
            skill2Result.TryGetValue(out Skill? skill2).ShouldBeTrue();
            skill2.ShouldNotBeNull();

            // Act & Assert
            skill1.ShouldNotBe(skill2);
        }

        [Fact]
        public void Equality_WithDifferentSpecialization_ShouldNotBeEqual()
        {
            // Arrange
            Result<Skill> skill1Result = Skill.Create("Firearms", 4, "Pistols");
            skill1Result.TryGetValue(out Skill? skill1).ShouldBeTrue();
            skill1.ShouldNotBeNull();
            
            Result<Skill> skill2Result = Skill.Create("Firearms", 4, "Rifles");
            skill2Result.TryGetValue(out Skill? skill2).ShouldBeTrue();
            skill2.ShouldNotBeNull();

            // Act & Assert
            skill1.ShouldNotBe(skill2);
        }

        [Fact]
        public void Equality_WithOneHavingSpecializationOtherNot_ShouldNotBeEqual()
        {
            // Arrange
            Result<Skill> skill1Result = Skill.Create("Firearms", 4, "Pistols");
            skill1Result.TryGetValue(out Skill? skill1).ShouldBeTrue();
            skill1.ShouldNotBeNull();
            
            Result<Skill> skill2Result = Skill.Create("Firearms", 4);
            skill2Result.TryGetValue(out Skill? skill2).ShouldBeTrue();
            skill2.ShouldNotBeNull();

            // Act & Assert
            skill1.ShouldNotBe(skill2);
        }

        [Fact]
        public void Equality_WithNull_ShouldNotBeEqual()
        {
            // Arrange
            Result<Skill> skillResult = Skill.Create("Test Skill", 4);
            skillResult.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();

            // Act & Assert
            skill.ShouldNotBe(null);
            (skill == null).ShouldBeFalse();
        }
    }

    public sealed class HasSpecializationProperty
    {
        [Fact]
        public void HasSpecialization_WithSpecialization_ShouldReturnTrue()
        {
            // Arrange
            Result<Skill> skillResult = Skill.Create("Firearms", 4, "Pistols");
            skillResult.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();

            // Act & Assert
            skill.HasSpecialization.ShouldBeTrue();
        }

        [Fact]
        public void HasSpecialization_WithoutSpecialization_ShouldReturnFalse()
        {
            // Arrange
            Result<Skill> skillResult = Skill.Create("Firearms", 4);
            skillResult.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();

            // Act & Assert
            skill.HasSpecialization.ShouldBeFalse();
        }

        [Fact]
        public void HasSpecialization_WithNullSpecialization_ShouldReturnFalse()
        {
            // Arrange
            Result<Skill> skillResult = Skill.Create("Firearms", 4, null);
            skillResult.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();

            // Act & Assert
            skill.HasSpecialization.ShouldBeFalse();
        }

        [Fact]
        public void HasSpecialization_WithEmptySpecialization_ShouldReturnFalse()
        {
            // Arrange - Empty specializations get converted to null during creation
            Result<Skill> skillResult = Skill.Create("Firearms", 4, "");
            skillResult.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();

            // Act & Assert
            skill.HasSpecialization.ShouldBeFalse();
        }
    }

    public sealed class ToStringMethod
    {
        [Fact]
        public void ToString_WithoutSpecialization_ShouldShowNameAndRating()
        {
            // Arrange
            Result<Skill> skillResult = Skill.Create("Firearms", 4);
            skillResult.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();

            // Act
            string result = skill.ToString();

            // Assert
            result.ShouldBe("Firearms 4");
        }

        [Fact]
        public void ToString_WithSpecialization_ShouldShowNameRatingAndSpecialization()
        {
            // Arrange
            Result<Skill> skillResult = Skill.Create("Firearms", 4, "Pistols");
            skillResult.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();

            // Act
            string result = skill.ToString();

            // Assert
            result.ShouldBe("Firearms 4 (Pistols)");
        }

        [Theory]
        [InlineData("Athletics", 0, null, "Athletics 0")]
        [InlineData("Hacking", 12, "Cybercombat", "Hacking 12 (Cybercombat)")]
        [InlineData("Longarms", 6, "Sniper Rifles", "Longarms 6 (Sniper Rifles)")]
        public void ToString_WithVariousInputs_ShouldFormatCorrectly(string name, int rating, string? specialization, string expectedResult)
        {
            // Arrange
            Result<Skill> skillResult = Skill.Create(name, rating, specialization);
            skillResult.TryGetValue(out Skill? skill).ShouldBeTrue();
            skill.ShouldNotBeNull();

            // Act
            string result = skill.ToString();

            // Assert
            result.ShouldBe(expectedResult);
        }
    }

    public sealed class ImmutabilityTests
    {
        [Fact]
        public void Skill_ShouldBeImmutable_WithMethodsReturningNewInstances()
        {
            // Arrange
            Result<Skill> originalResult = Skill.Create("Firearms", 4, "Pistols");
            originalResult.TryGetValue(out Skill? originalSkill).ShouldBeTrue();
            originalSkill.ShouldNotBeNull();

            // Act
            Result<Skill> ratingChangedResult = originalSkill.WithRating(6);
            ratingChangedResult.TryGetValue(out Skill? ratingChanged).ShouldBeTrue();
            ratingChanged.ShouldNotBeNull();
            
            Result<Skill> specializationChangedResult = originalSkill.WithSpecialization("Rifles");
            specializationChangedResult.TryGetValue(out Skill? specializationChanged).ShouldBeTrue();
            specializationChanged.ShouldNotBeNull();

            // Assert - Original should be unchanged
            originalSkill.Rating.ShouldBe(4);
            originalSkill.Specialization.ShouldBe("Pistols");

            // Assert - New instances should have updated values
            ratingChanged.Rating.ShouldBe(6);
            ratingChanged.Specialization.ShouldBe("Pistols");
            
            specializationChanged.Rating.ShouldBe(4);
            specializationChanged.Specialization.ShouldBe("Rifles");

            // Assert - All instances should be different objects
            originalSkill.ShouldNotBeSameAs(ratingChanged);
            originalSkill.ShouldNotBeSameAs(specializationChanged);
            ratingChanged.ShouldNotBeSameAs(specializationChanged);
        }

        [Fact]
        public void Skill_ShouldEnforceValidationRulesConsistently()
        {
            // Arrange
            Result<Skill> validSkillResult = Skill.Create("Firearms", 4, "Pistols");
            validSkillResult.TryGetValue(out Skill? validSkill).ShouldBeTrue();
            validSkill.ShouldNotBeNull();

            // Act & Assert - All creation/modification methods should enforce same rules
            Skill.Create("", 4).IsFailure.ShouldBeTrue(); // Empty name
            Skill.Create("Valid", -1).IsFailure.ShouldBeTrue(); // Invalid rating
            Skill.Create("Valid", 4, new string('A', 51)).IsFailure.ShouldBeTrue(); // Too long specialization
            
            validSkill.WithRating(-1).IsFailure.ShouldBeTrue(); // Invalid rating update
            validSkill.WithSpecialization(new string('A', 51)).IsFailure.ShouldBeTrue(); // Too long specialization update
        }

        [Fact]
        public void Skill_ShouldHandleSpecializationLogicConsistently()
        {
            // Arrange & Act
            Result<Skill> withSpecializationResult = Skill.Create("Firearms", 4, "Pistols");
            withSpecializationResult.TryGetValue(out Skill? withSpecialization).ShouldBeTrue();
            withSpecialization.ShouldNotBeNull();
            
            Result<Skill> withoutSpecializationResult = Skill.Create("Athletics", 3);
            withoutSpecializationResult.TryGetValue(out Skill? withoutSpecialization).ShouldBeTrue();
            withoutSpecialization.ShouldNotBeNull();
            
            Result<Skill> withEmptySpecializationResult = Skill.Create("Hacking", 5, "");
            withEmptySpecializationResult.TryGetValue(out Skill? withEmptySpecialization).ShouldBeTrue();
            withEmptySpecialization.ShouldNotBeNull();

            // Assert - Specialization logic should be consistent
            withSpecialization.HasSpecialization.ShouldBeTrue();
            withSpecialization.GetDicePool(true).ShouldBe(6); // 4 + 2 bonus
            withSpecialization.GetDicePool(false).ShouldBe(4); // No bonus
            
            withoutSpecialization.HasSpecialization.ShouldBeFalse();
            withoutSpecialization.GetDicePool(true).ShouldBe(3); // No bonus possible
            withoutSpecialization.GetDicePool(false).ShouldBe(3); // Same as above
            
            withEmptySpecialization.HasSpecialization.ShouldBeFalse(); // Empty string becomes null
            withEmptySpecialization.Specialization.ShouldBeNull();
        }
    }
}