using FlowRight.Core.Results;
using ShadowrunGM.Domain.Character.ValueObjects;

namespace ShadowrunGM.Domain.Tests.Character;

/// <summary>
/// Failing tests that define how Character.Create() should behave when refactored to use ValidationBuilder.
/// These tests use the current Result API but define the expected ValidationBuilder behavior patterns.
/// </summary>
public sealed class CharacterValidationTests
{
    public sealed class NameValidationRequirements
    {
        [Fact]
        public void Create_WithEmptyName_ShouldReturnStructuredValidationError()
        {
            // Arrange
            string emptyName = string.Empty;
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int startingEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(emptyName, attributes, startingEdge, []);

            // Assert - This test will FAIL until ValidationBuilder is implemented
            // Current implementation returns single error string, ValidationBuilder should return structured errors
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation); // This will FAIL - currently returns Error type
            result.Failures.ShouldContainKey("Name"); // This will FAIL - currently no structured errors
            result.Failures["Name"].ShouldContain(error => 
                error.Contains("Name") && error.Contains("empty", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Create_WithNullName_ShouldReturnStructuredValidationError()
        {
            // Arrange
            string nullName = null!;
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int startingEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(nullName, attributes, startingEdge, []);

            // Assert - This will FAIL until ValidationBuilder NotEmpty rule is implemented
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            result.Failures.ShouldContainKey("Name");
            result.Failures["Name"].ShouldContain(error => 
                error.Contains("Name") && error.Contains("empty", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Create_WithTooLongName_ShouldReturnStructuredValidationError()
        {
            // Arrange
            string tooLongName = new string('A', 101);
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int startingEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(tooLongName, attributes, startingEdge, []);

            // Assert - This will FAIL until ValidationBuilder MaximumLength rule is implemented
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            result.Failures.ShouldContainKey("Name");
            result.Failures["Name"].ShouldContain(error => 
                error.Contains("Name") && error.Contains("100") && 
                error.Contains("maximum", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Create_WithValidName_ShouldSucceed()
        {
            // Arrange
            string validName = "Valid Character Name";
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int startingEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(validName, attributes, startingEdge, []);

            // Assert - This should work with current implementation
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character).ShouldBeTrue();
            character.ShouldNotBeNull();
            character.Name.ShouldBe(validName);
        }
    }

    public sealed class AttributeSetValidationRequirements
    {
        [Fact]
        public void Create_WithNullAttributes_ShouldReturnStructuredValidationError()
        {
            // Arrange
            string validName = "Test Character";
            AttributeSet nullAttributes = null!;
            int startingEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(validName, nullAttributes, startingEdge, []);

            // Assert - This will FAIL until ValidationBuilder NotNull rule is implemented
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            result.Failures.ShouldContainKey("Attributes");
            result.Failures["Attributes"].ShouldContain(error => 
                error.Contains("Attributes") && error.Contains("null", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Create_WithValidAttributes_ShouldSucceed()
        {
            // Arrange
            string validName = "Test Character";
            AttributeSet validAttributes = new AttributeSetBuilder().Build();
            int startingEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(validName, validAttributes, startingEdge, []);

            // Assert - This should work with current implementation
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character).ShouldBeTrue();
            character.ShouldNotBeNull();
            character.Attributes.ShouldBe(validAttributes);
        }
    }

    public sealed class StartingEdgeValidationRequirements
    {
        [Fact]
        public void Create_WithTooLowStartingEdge_ShouldReturnStructuredValidationError()
        {
            // Arrange
            string validName = "Test Character";
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int tooLowEdge = 0;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(validName, attributes, tooLowEdge, []);

            // Assert - This will FAIL until ValidationBuilder InclusiveBetween rule is implemented
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            result.Failures.ShouldContainKey("StartingEdge");
            result.Failures["StartingEdge"].ShouldContain(error => 
                error.Contains("StartingEdge") && error.Contains("1") && error.Contains("7") &&
                error.Contains("between", StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public void Create_WithTooHighStartingEdge_ShouldReturnStructuredValidationError()
        {
            // Arrange
            string validName = "Test Character";
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int tooHighEdge = 8;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(validName, attributes, tooHighEdge, []);

            // Assert - This will FAIL until ValidationBuilder InclusiveBetween rule is implemented
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            result.Failures.ShouldContainKey("StartingEdge");
            result.Failures["StartingEdge"].ShouldContain(error => 
                error.Contains("StartingEdge") && error.Contains("1") && error.Contains("7") &&
                error.Contains("between", StringComparison.OrdinalIgnoreCase));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        [InlineData(7)]
        public void Create_WithValidStartingEdge_ShouldSucceed(int validEdge)
        {
            // Arrange
            string validName = "Test Character";
            AttributeSet attributes = new AttributeSetBuilder().Build();

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(validName, attributes, validEdge, []);

            // Assert - This should work with current implementation
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character).ShouldBeTrue();
            character.ShouldNotBeNull();
            character.Edge.Current.ShouldBe(validEdge);
            character.Edge.Max.ShouldBe(validEdge);
        }
    }

    public sealed class ValidationBuilderBehaviorRequirements
    {
        [Fact]
        public void Create_WithMultipleValidationFailures_ShouldReturnAllValidationErrors()
        {
            // Arrange
            string emptyName = string.Empty;
            AttributeSet nullAttributes = null!;
            int invalidEdge = 0;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(emptyName, nullAttributes, invalidEdge, []);

            // Assert - This will FAIL until ValidationBuilder accumulates all errors
            // Current implementation stops at first error (manual if statements)
            // ValidationBuilder should collect ALL validation errors
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            
            // ValidationBuilder should provide all three validation errors
            result.Failures.Count.ShouldBe(3);
            result.Failures.ShouldContainKey("Name");
            result.Failures.ShouldContainKey("Attributes");
            result.Failures.ShouldContainKey("StartingEdge");
            
            // Each property should have at least one validation error
            result.Failures["Name"].Length.ShouldBeGreaterThan(0);
            result.Failures["Attributes"].Length.ShouldBeGreaterThan(0);
            result.Failures["StartingEdge"].Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public void Create_WithValidationError_ShouldUseValidationFailureType()
        {
            // Arrange
            string emptyName = string.Empty;
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int validEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(emptyName, attributes, validEdge, []);

            // Assert - This will FAIL until ValidationBuilder is implemented
            // Current implementation uses generic Error type
            // ValidationBuilder should use specific Validation failure type
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
        }

        [Fact]
        public void Create_WithValidationBuilder_ShouldProvideStructuredErrorFormat()
        {
            // Arrange
            string tooLongName = new string('A', 101);
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int validEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(tooLongName, attributes, validEdge, []);

            // Assert - This defines the expected ValidationBuilder error structure
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            result.Failures.ShouldNotBeNull();
            result.Failures.ShouldBeOfType<Dictionary<string, string[]>>();
            
            // Property name should match the RuleFor expression
            result.Failures.ShouldContainKey("Name");
            
            // Error array should contain meaningful validation messages
            result.Failures["Name"].ShouldBeOfType<string[]>();
            result.Failures["Name"].Length.ShouldBe(1);
            result.Failures["Name"][0].ShouldNotBeNullOrWhiteSpace();
        }
    }

    public sealed class ExpectedValidationRulePatterns
    {
        [Fact]
        public void Create_ShouldImplement_NameNotEmptyRule()
        {
            // This test documents that Character.Create should use:
            // ValidationBuilder<Character> builder = new();
            // return builder.RuleFor(x => x.Name, name).NotEmpty()
            
            // Arrange
            string emptyName = string.Empty;
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int validEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(emptyName, attributes, validEdge, []);

            // Assert - NotEmpty rule should produce this specific error pattern
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            result.Failures.ShouldContainKey("Name");
            
            // NotEmpty rule typically produces standardized error messages
            string errorMessage = result.Failures["Name"][0];
            errorMessage.ShouldContain("Name");
            errorMessage.ShouldContain("empty", Case.Insensitive);
        }

        [Fact]
        public void Create_ShouldImplement_NameMaximumLengthRule()
        {
            // This test documents that Character.Create should use:
            // builder.RuleFor(x => x.Name, name).MaximumLength(100)
            
            // Arrange
            string tooLongName = new string('A', 101);
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int validEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(tooLongName, attributes, validEdge, []);

            // Assert - MaximumLength rule should produce this specific error pattern
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            result.Failures.ShouldContainKey("Name");
            
            // MaximumLength rule typically includes the limit in the message
            string errorMessage = result.Failures["Name"][0];
            errorMessage.ShouldContain("Name");
            errorMessage.ShouldContain("100");
            errorMessage.ShouldContain("maximum", Case.Insensitive);
        }

        [Fact]
        public void Create_ShouldImplement_AttributesNotNullRule()
        {
            // This test documents that Character.Create should use:
            // builder.RuleFor(x => x.Attributes, attributes).Notnull()
            
            // Arrange
            string validName = "Test";
            AttributeSet nullAttributes = null!;
            int validEdge = 3;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(validName, nullAttributes, validEdge, []);

            // Assert - NotNull rule should produce this specific error pattern
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            result.Failures.ShouldContainKey("Attributes");
            
            // NotNull rule typically produces standardized error messages
            string errorMessage = result.Failures["Attributes"][0];
            errorMessage.ShouldContain("Attributes");
            errorMessage.ShouldContain("null", Case.Insensitive);
        }

        [Fact]
        public void Create_ShouldImplement_StartingEdgeInclusiveBetweenRule()
        {
            // This test documents that Character.Create should use:
            // builder.RuleFor(x => x.StartingEdge, startingEdge).InclusiveBetween(1, 7)
            
            // Arrange
            string validName = "Test";
            AttributeSet attributes = new AttributeSetBuilder().Build();
            int invalidEdge = 0;

            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(validName, attributes, invalidEdge, []);

            // Assert - InclusiveBetween rule should produce this specific error pattern
            result.IsFailure.ShouldBeTrue();
            result.FailureType.ShouldBe(ResultFailureType.Validation);
            result.Failures.ShouldContainKey("StartingEdge");
            
            // InclusiveBetween rule typically includes the range in the message
            string errorMessage = result.Failures["StartingEdge"][0];
            errorMessage.ShouldContain("StartingEdge");
            errorMessage.ShouldContain("1");
            errorMessage.ShouldContain("7");
            errorMessage.ShouldContain("between", Case.Insensitive);
        }
    }
}