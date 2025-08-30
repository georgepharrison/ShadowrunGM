using Shouldly;
using ShadowrunGM.ApiSdk.Common.Results;
using ShadowrunGM.Domain.Character.ValueObjects;
using ShadowrunGM.Domain.Tests.TestHelpers;
using Xunit;

namespace ShadowrunGM.Domain.Tests.Character;

/// <summary>
/// Contract tests that define the exact ValidationBuilder implementation expected for Character.Create().
/// These tests document the specific ValidationBuilder API calls and patterns that should be used.
/// All tests will FAIL until Character.Create() is refactored to use ValidationBuilder.
/// </summary>
public sealed class CharacterValidationBuilderContractTests
{
    /// <summary>
    /// This test documents the exact ValidationBuilder pattern that Character.Create() should implement:
    /// 
    /// ValidationBuilder&lt;Character&gt; builder = new();
    /// return builder
    ///     .RuleFor(x =&gt; x.Name, name)
    ///         .NotEmpty()
    ///         .MaximumLength(100)
    ///     .RuleFor(x =&gt; x.Attributes, attributes)
    ///         .Notnull()
    ///     .RuleFor(x =&gt; x.StartingEdge, startingEdge)
    ///         .InclusiveBetween(1, 7)
    ///     .Build(() =&gt; /* factory method */);
    /// </summary>
    [Fact]
    public void Create_ShouldUseValidationBuilderWithFluentAPI()
    {
        // Arrange - Invalid data that should trigger all validation rules
        string emptyName = string.Empty;
        AttributeSet nullAttributes = null!;
        int invalidEdge = -1;

        // Act
        Result<ShadowrunGM.Domain.Character.Character> result = 
            ShadowrunGM.Domain.Character.Character.Create(emptyName, nullAttributes, invalidEdge);

        // Assert - ValidationBuilder should provide structured validation errors
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        
        // ValidationBuilder should accumulate errors from all rules
        result.Failures.Count.ShouldBe(3);
        
        // Name validation - NotEmpty rule
        result.Failures.ShouldContainKey("Name");
        result.Failures["Name"].Length.ShouldBe(1);
        result.Failures["Name"][0].ShouldContain("Name");
        result.Failures["Name"][0].ShouldContain("empty", Case.Insensitive);
        
        // Attributes validation - NotNull rule  
        result.Failures.ShouldContainKey("Attributes");
        result.Failures["Attributes"].Length.ShouldBe(1);
        result.Failures["Attributes"][0].ShouldContain("Attributes");
        result.Failures["Attributes"][0].ShouldContain("null", Case.Insensitive);
        
        // StartingEdge validation - InclusiveBetween rule
        result.Failures.ShouldContainKey("StartingEdge");
        result.Failures["StartingEdge"].Length.ShouldBe(1);
        result.Failures["StartingEdge"][0].ShouldContain("StartingEdge");
        result.Failures["StartingEdge"][0].ShouldContain("1");
        result.Failures["StartingEdge"][0].ShouldContain("7");
        result.Failures["StartingEdge"][0].ShouldContain("between", Case.Insensitive);
    }

    /// <summary>
    /// This test verifies that when validation passes, the ValidationBuilder factory method
    /// is executed correctly to create the Character instance.
    /// </summary>
    [Fact]
    public void Create_WithValidInput_ShouldExecuteFactoryMethodCorrectly()
    {
        // Arrange - Valid data that should pass all validation rules
        string validName = "  Valid Character Name  "; // With whitespace to test trimming
        AttributeSet validAttributes = new AttributeSetBuilder().Build();
        int validEdge = 4;

        // Act
        Result<ShadowrunGM.Domain.Character.Character> result = 
            ShadowrunGM.Domain.Character.Character.Create(validName, validAttributes, validEdge);

        // Assert - ValidationBuilder factory should create Character correctly
        result.IsSuccess.ShouldBeTrue();
        result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character).ShouldBeTrue();
        character.ShouldNotBeNull();
        
        // Factory should handle business logic like trimming
        character.Name.ShouldBe("Valid Character Name");
        character.Attributes.ShouldBe(validAttributes);
        character.Edge.Current.ShouldBe(validEdge);
        character.Edge.Max.ShouldBe(validEdge);
        
        // Factory should initialize all other properties
        character.Id.ShouldNotBe(default);
        character.CreatedAt.ShouldBeGreaterThan(DateTime.MinValue);
        character.ModifiedAt.ShouldBe(character.CreatedAt);
        character.Health.ShouldNotBeNull();
        character.Skills.Count.ShouldBe(0);
        character.DomainEvents.Count.ShouldBe(1);
    }

    /// <summary>
    /// This test verifies that ValidationBuilder chains multiple rules per property correctly.
    /// </summary>
    [Fact]
    public void Create_WithNameTooLong_ShouldTriggerMaximumLengthRule()
    {
        // Arrange - Name that's not empty but too long (should pass NotEmpty, fail MaximumLength)
        string tooLongName = new string('A', 101);
        AttributeSet validAttributes = new AttributeSetBuilder().Build();
        int validEdge = 3;

        // Act
        Result<ShadowrunGM.Domain.Character.Character> result = 
            ShadowrunGM.Domain.Character.Character.Create(tooLongName, validAttributes, validEdge);

        // Assert - Only MaximumLength rule should fire (NotEmpty should pass)
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.Count.ShouldBe(1);
        result.Failures.ShouldContainKey("Name");
        result.Failures["Name"].Length.ShouldBe(1);
        
        // MaximumLength rule should provide specific error message
        string errorMessage = result.Failures["Name"][0];
        errorMessage.ShouldContain("Name");
        errorMessage.ShouldContain("100");
        errorMessage.ShouldContain("maximum", Case.Insensitive);
    }

    /// <summary>
    /// This test documents the ValidationBuilder chaining pattern for continuing validation
    /// after a rule to validate other properties.
    /// </summary>
    [Fact]
    public void Create_WithNameValidButEdgeInvalid_ShouldShowOnlyEdgeError()
    {
        // Arrange - Valid name and attributes, invalid edge
        string validName = "Valid Name";
        AttributeSet validAttributes = new AttributeSetBuilder().Build();
        int invalidEdge = 10;

        // Act
        Result<ShadowrunGM.Domain.Character.Character> result = 
            ShadowrunGM.Domain.Character.Character.Create(validName, validAttributes, invalidEdge);

        // Assert - Only StartingEdge validation should fail
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.Count.ShouldBe(1);
        result.Failures.ShouldContainKey("StartingEdge");
        result.Failures["StartingEdge"].Length.ShouldBe(1);
        
        // InclusiveBetween rule should provide range information
        string errorMessage = result.Failures["StartingEdge"][0];
        errorMessage.ShouldContain("StartingEdge");
        errorMessage.ShouldContain("1");
        errorMessage.ShouldContain("7");
        errorMessage.ShouldContain("between", Case.Insensitive);
    }

    /// <summary>
    /// This test verifies that ValidationBuilder should use proper property names
    /// that match the lambda expressions in RuleFor calls.
    /// </summary>
    [Fact]
    public void Create_ValidationErrorKeys_ShouldMatchPropertyExpressions()
    {
        // Arrange - Multiple invalid inputs
        string emptyName = "";
        AttributeSet nullAttributes = null!;
        int invalidEdge = 0;

        // Act
        Result<ShadowrunGM.Domain.Character.Character> result = 
            ShadowrunGM.Domain.Character.Character.Create(emptyName, nullAttributes, invalidEdge);

        // Assert - Property names should match exactly what's in RuleFor expressions
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        
        // These are the exact property names expected from ValidationBuilder RuleFor expressions
        result.Failures.Keys.ShouldContain("Name");           // RuleFor(x => x.Name, name)
        result.Failures.Keys.ShouldContain("Attributes");     // RuleFor(x => x.Attributes, attributes)
        result.Failures.Keys.ShouldContain("StartingEdge");   // RuleFor(x => x.StartingEdge, startingEdge)
        
        // All keys should be Pascal case (C# property naming convention)
        foreach (string key in result.Failures.Keys)
        {
            char.IsUpper(key[0]).ShouldBeTrue($"Property key '{key}' should be Pascal case");
        }
    }

    /// <summary>
    /// This test documents that ValidationBuilder should support custom error messages
    /// with domain-appropriate language.
    /// </summary>
    [Fact]
    public void Create_ValidationErrors_ShouldUseDomainAppropriateMessages()
    {
        // Arrange
        string emptyName = string.Empty;
        AttributeSet validAttributes = new AttributeSetBuilder().Build();
        int validEdge = 3;

        // Act
        Result<ShadowrunGM.Domain.Character.Character> result = 
            ShadowrunGM.Domain.Character.Character.Create(emptyName, validAttributes, validEdge);

        // Assert - Error messages should be domain-specific and user-friendly
        result.IsFailure.ShouldBeTrue();
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldContainKey("Name");
        
        string errorMessage = result.Failures["Name"][0];
        // Message should mention "Character name" rather than just "Name"
        errorMessage.ShouldContain("Character name", Case.Insensitive);
        errorMessage.ShouldContain("required", Case.Insensitive);
    }

    /// <summary>
    /// This test verifies that ValidationBuilder Build method only executes the factory
    /// when all validation rules pass.
    /// </summary>
    [Fact]
    public void Create_WithValidationFailures_ShouldNotExecuteFactory()
    {
        // Arrange
        string invalidName = string.Empty;
        AttributeSet validAttributes = new AttributeSetBuilder().Build();
        int validEdge = 3;

        // Act
        Result<ShadowrunGM.Domain.Character.Character> result = 
            ShadowrunGM.Domain.Character.Character.Create(invalidName, validAttributes, validEdge);

        // Assert - Factory should not execute, no Character instance should be created
        result.IsFailure.ShouldBeTrue();
        result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character).ShouldBeFalse();
        character.ShouldBeNull();
        
        // Only validation errors should be present, no side effects from factory execution
        result.FailureType.ShouldBe(ResultFailureType.Validation);
        result.Failures.ShouldNotBeNull();
        result.Failures.Count.ShouldBeGreaterThan(0);
    }
}