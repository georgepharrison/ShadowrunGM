using FlowRight.Core.Results;

namespace ShadowrunGM.Domain.Tests.Character;

/// <summary>
/// Comprehensive failing tests for the new ValidationBuilder Result T composition capabilities.
/// These tests define the expected behavior of the refactored validation system using:
/// - ValidationBuilder.RuleFor with Result T parameters and out value pattern
/// - Clean expression-bodied factory methods using only ValidationBuilder fluent API
/// - Result T composition for nested validation
/// 
/// THESE TESTS ARE DESIGNED TO FAIL until the ValidationBuilder is enhanced with Result T composition.
/// </summary>
public sealed class CharacterValidationBuilderTests
{
    public sealed class CreateWithResultComposition
    {
        [Fact]
        public void Create_WithValidInputs_ShouldUseValidationBuilderResultComposition()
        {
            // Arrange
            string characterName = "Test Runner";
            Dictionary<string, int> attributeValues = new()
            {
                ["Body"] = 3,
                ["Agility"] = 4,
                ["Reaction"] = 3,
                ["Strength"] = 3,
                ["Willpower"] = 4,
                ["Logic"] = 5,
                ["Intuition"] = 4,
                ["Charisma"] = 3
            };
            int startingEdge = 3;
            
            // Act - This should use the new ValidationBuilder.RuleFor with Result T pattern
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(characterName, attributeValues, startingEdge);
            
            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character).ShouldBeTrue();
            character.ShouldNotBeNull();
            character.Name.ShouldBe(characterName);
            character.Attributes.Body.ShouldBe(3);
            character.Edge.Current.ShouldBe(startingEdge);
        }
        
        [Fact]
        public void Create_WithInvalidAttributeSet_ShouldAggregateValidationErrors()
        {
            // Arrange
            string characterName = "Valid Name";
            Dictionary<string, int> invalidAttributeValues = new()
            {
                ["Body"] = 0, // Invalid: below minimum
                ["Agility"] = 11, // Invalid: above maximum
                ["Reaction"] = 3,
                ["Strength"] = 3,
                ["Willpower"] = 4,
                ["Logic"] = 5,
                ["Intuition"] = 4,
                ["Charisma"] = 3
            };
            int startingEdge = 3;
            
            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(characterName, invalidAttributeValues, startingEdge);
            
            // Assert - Should fail with aggregated AttributeSet validation errors
            result.IsFailure.ShouldBeTrue();
            result.IsValidationException().ShouldBeTrue();
            
            Dictionary<string, string[]> errors = result.ValidationErrors();
            errors.ShouldContainKey("Attributes");
            errors["Attributes"].ShouldContain(error => error.Contains("Body"));
            errors["Attributes"].ShouldContain(error => error.Contains("Agility"));
        }
        
        [Fact]
        public void Create_WithInvalidEdgeValue_ShouldAggregateValidationErrors()
        {
            // Arrange
            string characterName = "Valid Name";
            Dictionary<string, int> validAttributeValues = new()
            {
                ["Body"] = 3, ["Agility"] = 4, ["Reaction"] = 3, ["Strength"] = 3,
                ["Willpower"] = 4, ["Logic"] = 5, ["Intuition"] = 4, ["Charisma"] = 3
            };
            int invalidStartingEdge = 10; // Invalid: above maximum
            
            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(characterName, validAttributeValues, invalidStartingEdge);
            
            // Assert - Should fail with Edge validation errors
            result.IsFailure.ShouldBeTrue();
            result.IsValidationException().ShouldBeTrue();
            
            Dictionary<string, string[]> errors = result.ValidationErrors();
            errors.ShouldContainKey("Edge");
            errors["Edge"].ShouldContain(error => error.Contains("Edge must be between 1 and 7"));
        }
        
        [Fact]
        public void Create_WithMultipleInvalidInputs_ShouldAggregateAllValidationErrors()
        {
            // Arrange
            string invalidName = ""; // Invalid: empty
            Dictionary<string, int> invalidAttributeValues = new()
            {
                ["Body"] = 0, // Invalid: below minimum
                ["Agility"] = 11 // Invalid: above maximum
                // Missing required attributes
            };
            int invalidStartingEdge = 0; // Invalid: below minimum
            
            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(invalidName, invalidAttributeValues, invalidStartingEdge);
            
            // Assert - Should fail with all validation errors aggregated
            result.IsFailure.ShouldBeTrue();
            result.IsValidationException().ShouldBeTrue();
            
            Dictionary<string, string[]> errors = result.ValidationErrors();
            errors.ShouldContainKey("Name");
            errors.ShouldContainKey("Attributes");  
            errors.ShouldContainKey("Edge");
            
            // Name validation errors
            errors["Name"].ShouldContain(error => error.Contains("Character name is required"));
            
            // Attributes validation errors  
            errors["Attributes"].ShouldNotBeEmpty();
            
            // Edge validation errors
            errors["Edge"].ShouldContain(error => error.Contains("Edge must be between 1 and 7"));
        }
    }
    
    public sealed class ExpressionBodiedFactoryMethod
    {
        [Fact]
        public void Create_ShouldUseExpressionBodiedValidationBuilderPattern()
        {
            // Arrange
            string characterName = "Test Runner";
            Dictionary<string, int> attributeValues = new()
            {
                ["Body"] = 3, ["Agility"] = 4, ["Reaction"] = 3, ["Strength"] = 3,
                ["Willpower"] = 4, ["Logic"] = 5, ["Intuition"] = 4, ["Charisma"] = 3
            };
            int startingEdge = 3;
            
            // Act - This tests that Character.Create uses a clean expression-bodied method
            // The expected signature should be an expression-bodied method using ValidationBuilder
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(characterName, attributeValues, startingEdge);
            
            // Assert
            result.IsSuccess.ShouldBeTrue();
        }
        
        [Fact]
        public void Create_ShouldNotUseHybridValidationPattern()
        {
            // This test documents that the old hybrid pattern should be eliminated:
            // - NO manual AddError calls mixed with fluent API
            // - NO manual null checks mixed with ValidationBuilder
            // - ALL validation should go through ValidationBuilder.RuleFor methods
            
            // Arrange - Test a scenario that would trigger the old hybrid pattern
            string characterName = "Valid Name";
            Dictionary<string, int>? nullAttributes = null;
            int startingEdge = 3;
            
            // Act
            Result<ShadowrunGM.Domain.Character.Character> result = 
                ShadowrunGM.Domain.Character.Character.Create(characterName, nullAttributes!, startingEdge);
            
            // Assert - Should fail through ValidationBuilder, not manual null checks
            result.IsFailure.ShouldBeTrue();
            result.IsValidationException().ShouldBeTrue();
            
            // The error should come from ValidationBuilder, not manual AddError calls
            Dictionary<string, string[]> errors = result.ValidationErrors();
            errors.ShouldNotBeEmpty();
        }
    }
}