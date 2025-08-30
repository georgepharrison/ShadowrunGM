using ShadowrunGM.ApiSdk.Common.Results;
using ShadowrunGM.Domain.Character.ValueObjects;

namespace ShadowrunGM.Infrastructure.Tests.TestHelpers;

/// <summary>
/// Builder pattern for creating Character test data.
/// </summary>
public sealed class CharacterBuilder
{
    private string _name = "Test Character";
    private int _body = 3;
    private int _agility = 4;
    private int _reaction = 3;
    private int _strength = 2;
    private int _willpower = 4;
    private int _logic = 5;
    private int _intuition = 4;
    private int _charisma = 3;
    private int _startingEdge = 3;

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
    /// Sets the character attributes using a builder function.
    /// </summary>
    /// <param name="builderAction">An action to configure attributes.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithAttributes(Func<CharacterBuilder, CharacterBuilder> builderAction)
    {
        return builderAction(this);
    }

    /// <summary>
    /// Sets the Body attribute.
    /// </summary>
    /// <param name="body">The Body attribute value.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithBody(int body)
    {
        _body = body;
        return this;
    }

    /// <summary>
    /// Sets the Agility attribute.
    /// </summary>
    /// <param name="agility">The Agility attribute value.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithAgility(int agility)
    {
        _agility = agility;
        return this;
    }

    /// <summary>
    /// Sets the Reaction attribute.
    /// </summary>
    /// <param name="reaction">The Reaction attribute value.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithReaction(int reaction)
    {
        _reaction = reaction;
        return this;
    }

    /// <summary>
    /// Sets the Strength attribute.
    /// </summary>
    /// <param name="strength">The Strength attribute value.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithStrength(int strength)
    {
        _strength = strength;
        return this;
    }

    /// <summary>
    /// Sets the Willpower attribute.
    /// </summary>
    /// <param name="willpower">The Willpower attribute value.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithWillpower(int willpower)
    {
        _willpower = willpower;
        return this;
    }

    /// <summary>
    /// Sets the Logic attribute.
    /// </summary>
    /// <param name="logic">The Logic attribute value.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithLogic(int logic)
    {
        _logic = logic;
        return this;
    }

    /// <summary>
    /// Sets the Intuition attribute.
    /// </summary>
    /// <param name="intuition">The Intuition attribute value.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithIntuition(int intuition)
    {
        _intuition = intuition;
        return this;
    }

    /// <summary>
    /// Sets the Charisma attribute.
    /// </summary>
    /// <param name="charisma">The Charisma attribute value.</param>
    /// <returns>The builder instance.</returns>
    public CharacterBuilder WithCharisma(int charisma)
    {
        _charisma = charisma;
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
    /// Builds a Character using Character.Create().
    /// This method returns the successful result directly for convenience in tests.
    /// </summary>
    /// <returns>A valid Character instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Character creation fails.</exception>
    public ShadowrunGM.Domain.Character.Character Build()
    {
        Result<ShadowrunGM.Domain.Character.Character> result = ShadowrunGM.Domain.Character.Character.Create(
            _name, _body, _agility, _reaction, _strength, _willpower, _logic, _intuition, _charisma, _startingEdge);

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
}