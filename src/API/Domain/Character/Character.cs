using ShadowrunGM.ApiSdk.Common.Results;
using ShadowrunGM.API.Application.Common.Results;
using ShadowrunGM.Domain.Character.Events;
using ShadowrunGM.Domain.Character.ValueObjects;
using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Character;

/// <summary>
/// Aggregate root representing a Shadowrun character.
/// </summary>
public sealed class Character : AggregateRoot
{
    private readonly List<Skill> _skills = [];

    private Character()
    {
    }

    /// <summary>
    /// Gets the unique identifier for this character.
    /// </summary>
    public CharacterId Id { get; private init; }

    /// <summary>
    /// Gets the character's name.
    /// </summary>
    public string Name { get; private init; } = string.Empty;

    /// <summary>
    /// Gets the character's attributes.
    /// </summary>
    public AttributeSet Attributes { get; private set; } = null!;

    /// <summary>
    /// Gets the character's Edge resource.
    /// </summary>
    public Edge Edge { get; private set; } = null!;

    /// <summary>
    /// Gets the character's health tracking.
    /// </summary>
    public ConditionMonitor Health { get; private set; } = null!;

    /// <summary>
    /// Gets the character's skills.
    /// </summary>
    public IReadOnlyList<Skill> Skills => _skills.AsReadOnly();

    /// <summary>
    /// Gets the date and time when the character was created.
    /// </summary>
    public DateTime CreatedAt { get; private init; }

    /// <summary>
    /// Gets the date and time when the character was last modified.
    /// </summary>
    public DateTime ModifiedAt { get; private set; }

    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    /// <returns>The entity identifier.</returns>
    public override object GetId() => Id;

    /// <summary>
    /// Creates a new character with the specified attributes.
    /// </summary>
    /// <param name="name">The character's name.</param>
    /// <param name="attributes">The character's attributes.</param>
    /// <param name="startingEdge">The character's starting Edge value.</param>
    /// <returns>A Result containing the new character or an error.</returns>
    public static Result<Character> Create(
        string name,
        AttributeSet attributes,
        int startingEdge)
    {
        ValidationBuilder<Character> builder = new();
        
        // Add manual validations first (since ValidationBuilder doesn't support all types)
        if (attributes is null)
        {
            builder.AddError("Attributes", "Attributes cannot be null");
        }
        
        if (startingEdge < 1 || startingEdge > 7)
        {
            builder.AddError("StartingEdge", "StartingEdge must be between 1 and 7 (inclusive)");
        }

        // Use ValidationBuilder fluent API for string validation  
        return builder
            .RuleFor(x => x.Name, name, "Character name")
                .NotEmpty()
                .WithMessage("Character name is required - Name cannot be empty")
                .MaximumLength(100)
                .WithMessage("Name maximum length is 100")
            .Build(() => CreateCharacterInstance(name, attributes!, startingEdge));
    }

    /// <summary>
    /// Factory method for creating the character instance after validation passes.
    /// </summary>
    private static Character CreateCharacterInstance(string name, AttributeSet attributes, int startingEdge)
    {
        DateTime now = DateTime.UtcNow;
        Character character = new()
        {
            Id = CharacterId.New(),
            Name = name.Trim(),
            Attributes = attributes,
            Edge = Edge.Create(startingEdge),
            Health = ConditionMonitor.ForAttributes(attributes),
            CreatedAt = now,
            ModifiedAt = now
        };

        character.RaiseDomainEvent(new CharacterCreated(character.Id, character.Name));
        return character;
    }

    /// <summary>
    /// Creates a new character with skills.
    /// </summary>
    /// <param name="name">The character's name.</param>
    /// <param name="attributes">The character's attributes.</param>
    /// <param name="startingEdge">The character's starting Edge value.</param>
    /// <param name="skills">The character's initial skills.</param>
    /// <returns>A Result containing the new character or an error.</returns>
    public static Result<Character> Create(
        string name,
        AttributeSet attributes,
        int startingEdge,
        IEnumerable<Skill> skills)
    {
        Result<Character> characterResult = Create(name, attributes, startingEdge);
        if (!characterResult.IsSuccess)
            return characterResult;

        if (!characterResult.TryGetValue(out Character? character) || character == null)
            return characterResult;

        if (skills != null)
        {
            foreach (Skill skill in skills)
            {
                Result<Skill> addResult = character.AddSkill(skill.Name, skill.Rating, skill.Specialization);
                if (!addResult.IsSuccess)
                    return Result.Failure<Character>($"Failed to add skill {skill.Name}: {addResult.Error}");
            }
        }

        return Result.Success(character);
    }

    /// <summary>
    /// Spends Edge for a specific purpose.
    /// </summary>
    /// <param name="amount">The amount of Edge to spend.</param>
    /// <param name="purpose">The purpose for spending Edge.</param>
    /// <returns>A Result containing the EdgeSpent event or an error.</returns>
    public Result<EdgeSpent> SpendEdge(int amount, string purpose)
    {
        if (string.IsNullOrWhiteSpace(purpose))
            return Result.Failure<EdgeSpent>("Purpose for spending Edge is required.");

        Result<Edge> edgeResult = Edge.Spend(amount);
        if (!edgeResult.IsSuccess)
            return Result.Failure<EdgeSpent>(edgeResult.Error);

        if (!edgeResult.TryGetValue(out Edge? newEdge) || newEdge == null)
            return Result.Failure<EdgeSpent>("Failed to update Edge");
        
        Edge = newEdge;
        ModifiedAt = DateTime.UtcNow;

        EdgeSpent domainEvent = new(Id, amount, purpose);
        RaiseDomainEvent(domainEvent);

        return Result.Success(domainEvent);
    }

    /// <summary>
    /// Regains Edge.
    /// </summary>
    /// <param name="amount">The amount of Edge to regain.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public Result<Edge> RegainEdge(int amount)
    {
        Result<Edge> edgeResult = Edge.Regain(amount);
        if (!edgeResult.IsSuccess)
            return edgeResult;

        if (!edgeResult.TryGetValue(out Edge? newEdge) || newEdge == null)
            return edgeResult;
        
        Edge = newEdge;
        ModifiedAt = DateTime.UtcNow;

        return edgeResult;
    }

    /// <summary>
    /// Refreshes Edge to maximum.
    /// </summary>
    public void RefreshEdge()
    {
        Edge = Edge.Refresh();
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Burns Edge permanently.
    /// </summary>
    /// <param name="amount">The amount of Edge to burn.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public Result<Edge> BurnEdge(int amount = 1)
    {
        Result<Edge> edgeResult = Edge.Burn(amount);
        if (!edgeResult.IsSuccess)
            return edgeResult;

        if (!edgeResult.TryGetValue(out Edge? newEdge) || newEdge == null)
            return edgeResult;
        
        Edge = newEdge;
        ModifiedAt = DateTime.UtcNow;

        return edgeResult;
    }

    /// <summary>
    /// Takes Physical damage.
    /// </summary>
    /// <param name="amount">The amount of damage to take.</param>
    /// <returns>A Result containing the new health state or an error.</returns>
    public Result<ConditionMonitor> TakePhysicalDamage(int amount)
    {
        Result<ConditionMonitor> healthResult = Health.TakePhysicalDamage(amount);
        if (!healthResult.IsSuccess)
            return healthResult;

        if (!healthResult.TryGetValue(out ConditionMonitor? newHealth) || newHealth == null)
            return healthResult;
        
        Health = newHealth;
        ModifiedAt = DateTime.UtcNow;

        return healthResult;
    }

    /// <summary>
    /// Takes Stun damage.
    /// </summary>
    /// <param name="amount">The amount of damage to take.</param>
    /// <returns>A Result containing the new health state or an error.</returns>
    public Result<ConditionMonitor> TakeStunDamage(int amount)
    {
        Result<ConditionMonitor> healthResult = Health.TakeStunDamage(amount);
        if (!healthResult.IsSuccess)
            return healthResult;

        if (!healthResult.TryGetValue(out ConditionMonitor? newHealth) || newHealth == null)
            return healthResult;
        
        Health = newHealth;
        ModifiedAt = DateTime.UtcNow;

        return healthResult;
    }

    /// <summary>
    /// Heals Physical damage.
    /// </summary>
    /// <param name="amount">The amount of damage to heal.</param>
    /// <returns>A Result containing the new health state or an error.</returns>
    public Result<ConditionMonitor> HealPhysicalDamage(int amount)
    {
        Result<ConditionMonitor> healthResult = Health.HealPhysicalDamage(amount);
        if (!healthResult.IsSuccess)
            return healthResult;

        if (!healthResult.TryGetValue(out ConditionMonitor? newHealth) || newHealth == null)
            return healthResult;
        
        Health = newHealth;
        ModifiedAt = DateTime.UtcNow;

        return healthResult;
    }

    /// <summary>
    /// Heals Stun damage.
    /// </summary>
    /// <param name="amount">The amount of damage to heal.</param>
    /// <returns>A Result containing the new health state or an error.</returns>
    public Result<ConditionMonitor> HealStunDamage(int amount)
    {
        Result<ConditionMonitor> healthResult = Health.HealStunDamage(amount);
        if (!healthResult.IsSuccess)
            return healthResult;

        if (!healthResult.TryGetValue(out ConditionMonitor? newHealth) || newHealth == null)
            return healthResult;
        
        Health = newHealth;
        ModifiedAt = DateTime.UtcNow;

        return healthResult;
    }

    /// <summary>
    /// Heals all damage.
    /// </summary>
    public void HealAll()
    {
        Health = Health.HealAll();
        ModifiedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds a skill to the character.
    /// </summary>
    /// <param name="name">The skill name.</param>
    /// <param name="rating">The skill rating.</param>
    /// <param name="specialization">Optional skill specialization.</param>
    /// <returns>A Result containing the added skill or an error.</returns>
    public Result<Skill> AddSkill(string name, int rating, string? specialization = null)
    {
        if (_skills.Any(s => string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure<Skill>($"Skill '{name}' already exists.");

        Result<Skill> skillResult = Skill.Create(name, rating, specialization);
        if (!skillResult.IsSuccess)
            return skillResult;

        if (!skillResult.TryGetValue(out Skill? newSkill) || newSkill == null)
            return skillResult;
        
        _skills.Add(newSkill);
        ModifiedAt = DateTime.UtcNow;

        return skillResult;
    }

    /// <summary>
    /// Updates an existing skill.
    /// </summary>
    /// <param name="name">The skill name.</param>
    /// <param name="newRating">The new skill rating.</param>
    /// <returns>A Result indicating success or failure.</returns>
    public Result<Skill> UpdateSkill(string name, int newRating)
    {
        Skill? skill = _skills.FirstOrDefault(s => 
            string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

        if (skill == null)
            return Result.Failure<Skill>($"Skill '{name}' not found.");

        Result<Skill> updatedSkill = skill.WithRating(newRating);
        if (!updatedSkill.IsSuccess)
            return updatedSkill;

        int index = _skills.IndexOf(skill);
        if (!updatedSkill.TryGetValue(out Skill? newSkill) || newSkill == null)
            return updatedSkill;
        
        _skills[index] = newSkill;
        ModifiedAt = DateTime.UtcNow;

        return updatedSkill;
    }

    /// <summary>
    /// Removes a skill from the character.
    /// </summary>
    /// <param name="name">The skill name.</param>
    /// <returns>True if the skill was removed; otherwise, false.</returns>
    public bool RemoveSkill(string name)
    {
        Skill? skill = _skills.FirstOrDefault(s => 
            string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

        if (skill == null)
            return false;

        _skills.Remove(skill);
        ModifiedAt = DateTime.UtcNow;
        return true;
    }
}