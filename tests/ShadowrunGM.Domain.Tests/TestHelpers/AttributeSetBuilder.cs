using ShadowrunGM.ApiSdk.Common.Results;
using ShadowrunGM.Domain.Character.ValueObjects;

namespace ShadowrunGM.Domain.Tests.TestHelpers;

/// <summary>
/// Builder pattern for creating AttributeSet instances in tests.
/// </summary>
public sealed class AttributeSetBuilder
{
    private int _body = 3;
    private int _agility = 3;
    private int _reaction = 3;
    private int _strength = 3;
    private int _willpower = 3;
    private int _logic = 3;
    private int _intuition = 3;
    private int _charisma = 3;

    /// <summary>
    /// Sets the Body attribute value.
    /// </summary>
    /// <param name="body">The Body attribute value.</param>
    /// <returns>The builder instance.</returns>
    public AttributeSetBuilder WithBody(int body)
    {
        _body = body;
        return this;
    }

    /// <summary>
    /// Sets the Agility attribute value.
    /// </summary>
    /// <param name="agility">The Agility attribute value.</param>
    /// <returns>The builder instance.</returns>
    public AttributeSetBuilder WithAgility(int agility)
    {
        _agility = agility;
        return this;
    }

    /// <summary>
    /// Sets the Reaction attribute value.
    /// </summary>
    /// <param name="reaction">The Reaction attribute value.</param>
    /// <returns>The builder instance.</returns>
    public AttributeSetBuilder WithReaction(int reaction)
    {
        _reaction = reaction;
        return this;
    }

    /// <summary>
    /// Sets the Strength attribute value.
    /// </summary>
    /// <param name="strength">The Strength attribute value.</param>
    /// <returns>The builder instance.</returns>
    public AttributeSetBuilder WithStrength(int strength)
    {
        _strength = strength;
        return this;
    }

    /// <summary>
    /// Sets the Willpower attribute value.
    /// </summary>
    /// <param name="willpower">The Willpower attribute value.</param>
    /// <returns>The builder instance.</returns>
    public AttributeSetBuilder WithWillpower(int willpower)
    {
        _willpower = willpower;
        return this;
    }

    /// <summary>
    /// Sets the Logic attribute value.
    /// </summary>
    /// <param name="logic">The Logic attribute value.</param>
    /// <returns>The builder instance.</returns>
    public AttributeSetBuilder WithLogic(int logic)
    {
        _logic = logic;
        return this;
    }

    /// <summary>
    /// Sets the Intuition attribute value.
    /// </summary>
    /// <param name="intuition">The Intuition attribute value.</param>
    /// <returns>The builder instance.</returns>
    public AttributeSetBuilder WithIntuition(int intuition)
    {
        _intuition = intuition;
        return this;
    }

    /// <summary>
    /// Sets the Charisma attribute value.
    /// </summary>
    /// <param name="charisma">The Charisma attribute value.</param>
    /// <returns>The builder instance.</returns>
    public AttributeSetBuilder WithCharisma(int charisma)
    {
        _charisma = charisma;
        return this;
    }

    /// <summary>
    /// Sets all attributes to the minimum valid value (1).
    /// </summary>
    /// <returns>The builder instance.</returns>
    public AttributeSetBuilder WithMinimumAttributes()
    {
        _body = 1;
        _agility = 1;
        _reaction = 1;
        _strength = 1;
        _willpower = 1;
        _logic = 1;
        _intuition = 1;
        _charisma = 1;
        return this;
    }

    /// <summary>
    /// Sets all attributes to the maximum valid value (10).
    /// </summary>
    /// <returns>The builder instance.</returns>
    public AttributeSetBuilder WithMaximumAttributes()
    {
        _body = 10;
        _agility = 10;
        _reaction = 10;
        _strength = 10;
        _willpower = 10;
        _logic = 10;
        _intuition = 10;
        _charisma = 10;
        return this;
    }

    /// <summary>
    /// Sets all attributes to the default average value (3).
    /// </summary>
    /// <returns>The builder instance.</returns>
    public AttributeSetBuilder WithAverageAttributes()
    {
        _body = 3;
        _agility = 3;
        _reaction = 3;
        _strength = 3;
        _willpower = 3;
        _logic = 3;
        _intuition = 3;
        _charisma = 3;
        return this;
    }

    /// <summary>
    /// Builds an AttributeSet with the configured values.
    /// This method returns the successful result directly for convenience in tests.
    /// </summary>
    /// <returns>A valid AttributeSet instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the AttributeSet creation fails.</exception>
    public AttributeSet Build()
    {
        // Check if any values exceed normal limits and need testing bypass
        bool needsBypass = _body > 10 || _agility > 10 || _reaction > 10 || _strength > 10 ||
                          _willpower > 10 || _logic > 10 || _intuition > 10 || _charisma > 10;

        if (needsBypass)
        {
            // Use the testing method that bypasses validation
            return AttributeSet.CreateForTesting(
                _body, _agility, _reaction, _strength, 
                _willpower, _logic, _intuition, _charisma);
        }

        Result<AttributeSet> result = AttributeSet.Create(
            _body, _agility, _reaction, _strength, 
            _willpower, _logic, _intuition, _charisma);

        if (!result.IsSuccess)
        {
            throw new InvalidOperationException($"Failed to create AttributeSet: {result.Error}");
        }

        if (!result.TryGetValue(out AttributeSet? attributeSet) || attributeSet == null)
        {
            throw new InvalidOperationException("AttributeSet creation succeeded but returned null value");
        }

        return attributeSet;
    }

    /// <summary>
    /// Attempts to build an AttributeSet with the configured values, returning the Result.
    /// Use this method when you want to test failure scenarios.
    /// </summary>
    /// <returns>A Result containing the AttributeSet or an error.</returns>
    public Result<AttributeSet> TryBuild() =>
        AttributeSet.Create(_body, _agility, _reaction, _strength, 
                           _willpower, _logic, _intuition, _charisma);
}