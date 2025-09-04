using FlowRight.Core.Results;
using ShadowrunGM.Domain.Character.ValueObjects;

namespace ShadowrunGM.Domain.Tests.TestHelpers;

/// <summary>
/// Builder pattern for creating Character test data.
/// </summary>
public sealed class CharacterBuilder
{
    private string _name = "Test Character";
    private AttributeSet _attributes = new AttributeSetBuilder().Build();
    private int _startingEdge = 3;
    private List<Skill>? _skills;

    /// <summary>
    /// Sets the character name.
    /// </summary>
    /// <param name="name">The character name.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the character attributes.
    /// </summary>
    /// <param name="attributes">The character attributes.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithAttributes(AttributeSet attributes)
    {
        _attributes = attributes;
        return this;
    }

    /// <summary>
    /// Sets the character attributes using a builder function.
    /// </summary>
    /// <param name="builderAction">An action to configure the AttributeSetBuilder.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithAttributes(Func<AttributeSetBuilder, AttributeSetBuilder> builderAction)
    {
        AttributeSetBuilder attributeBuilder = new();
        _attributes = builderAction(attributeBuilder).Build();
        return this;
    }

    /// <summary>
    /// Sets the starting Edge value.
    /// </summary>
    /// <param name="startingEdge">The starting Edge value.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithStartingEdge(int startingEdge)
    {
        _startingEdge = startingEdge;
        return this;
    }

    /// <summary>
    /// Sets the character skills.
    /// </summary>
    /// <param name="skills">The character skills.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithSkills(IEnumerable<Skill> skills)
    {
        _skills = skills.ToList();
        return this;
    }

    /// <summary>
    /// Sets empty character name for testing validation.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithEmptyName()
    {
        _name = string.Empty;
        return this;
    }

    /// <summary>
    /// Sets null character name for testing validation.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithNullName()
    {
        _name = null!;
        return this;
    }

    /// <summary>
    /// Sets whitespace-only character name for testing validation.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithWhitespaceName()
    {
        _name = "   ";
        return this;
    }

    /// <summary>
    /// Sets a character name that exceeds the maximum length.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithTooLongName()
    {
        _name = new string('A', 101); // Exceeds 100 character limit
        return this;
    }

    /// <summary>
    /// Sets a character name at the maximum length boundary.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithMaxLengthName()
    {
        _name = new string('A', 100); // Exactly at 100 character limit
        return this;
    }

    /// <summary>
    /// Sets null AttributeSet for testing validation.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithNullAttributes()
    {
        _attributes = null!;
        return this;
    }

    /// <summary>
    /// Sets starting Edge below the minimum valid value.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithTooLowStartingEdge()
    {
        _startingEdge = 0;
        return this;
    }

    /// <summary>
    /// Sets starting Edge above the maximum valid value.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithTooHighStartingEdge()
    {
        _startingEdge = 8;
        return this;
    }

    /// <summary>
    /// Sets starting Edge at the minimum boundary.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithMinimumStartingEdge()
    {
        _startingEdge = 1;
        return this;
    }

    /// <summary>
    /// Sets starting Edge at the maximum boundary.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithMaximumStartingEdge()
    {
        _startingEdge = 7;
        return this;
    }

    /// <summary>
    /// Builds a Character using Character.Create() without skills.
    /// This method returns the successful result directly for convenience in tests.
    /// </summary>
    /// <returns>A valid Character instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Character creation fails.</exception>
    public ShadowrunGM.Domain.Character.Character Build()
    {
        Result<ShadowrunGM.Domain.Character.Character> result = ShadowrunGM.Domain.Character.Character.Create(_name, _attributes, _startingEdge, _skills ?? []);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to create Character: {result.Error}");
        }

        if (!result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character) || character == null)
        {
            throw new InvalidOperationException("Character creation succeeded but returned null value");
        }

        return character;
    }

    /// <summary>
    /// Builds a Character using Character.Create() with skills.
    /// This method returns the successful result directly for convenience in tests.
    /// </summary>
    /// <returns>A valid Character instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Character creation fails.</exception>
    public ShadowrunGM.Domain.Character.Character BuildWithSkills()
    {
        Result<ShadowrunGM.Domain.Character.Character> result = ShadowrunGM.Domain.Character.Character.Create(_name, _attributes, _startingEdge, _skills ?? []);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to create Character: {result.Error}");
        }

        if (!result.TryGetValue(out ShadowrunGM.Domain.Character.Character? character) || character == null)
        {
            throw new InvalidOperationException("Character creation succeeded but returned null value");
        }

        return character;
    }

    /// <summary>
    /// Attempts to build a Character using Character.Create(), returning the Result.
    /// Use this method when you want to test failure scenarios.
    /// </summary>
    /// <returns>A Result containing the Character or an error.</returns>
    public Result<ShadowrunGM.Domain.Character.Character> TryBuild() =>
        ShadowrunGM.Domain.Character.Character.Create(_name, _attributes, _startingEdge, _skills ?? []);

    /// <summary>
    /// Attempts to build a Character using Character.Create() with skills, returning the Result.
    /// Use this method when you want to test failure scenarios with skills.
    /// </summary>
    /// <returns>A Result containing the Character or an error.</returns>
    public Result<ShadowrunGM.Domain.Character.Character> TryBuildWithSkills() =>
        ShadowrunGM.Domain.Character.Character.Create(_name, _attributes, _startingEdge, _skills ?? []);
}