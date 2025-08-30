using ShadowrunGM.ApiSdk.Common.Results;
using ShadowrunGM.API.Application.Common.Results;
using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Character.ValueObjects;

/// <summary>
/// Represents a character's health tracking with Physical and Stun damage monitors.
/// </summary>
public sealed class ConditionMonitor : ValueObject
{
    private ConditionMonitor(
        int physicalBoxes,
        int stunBoxes,
        int physicalDamage,
        int stunDamage)
    {
        PhysicalBoxes = physicalBoxes;
        StunBoxes = stunBoxes;
        PhysicalDamage = physicalDamage;
        StunDamage = stunDamage;
    }

    /// <summary>
    /// Gets the total number of Physical damage boxes.
    /// </summary>
    public int PhysicalBoxes { get; }

    /// <summary>
    /// Gets the total number of Stun damage boxes.
    /// </summary>
    public int StunBoxes { get; }

    /// <summary>
    /// Gets the current Physical damage taken.
    /// </summary>
    public int PhysicalDamage { get; }

    /// <summary>
    /// Gets the current Stun damage taken.
    /// </summary>
    public int StunDamage { get; }

    /// <summary>
    /// Gets the remaining Physical boxes.
    /// </summary>
    public int PhysicalRemaining => Math.Max(0, PhysicalBoxes - PhysicalDamage);

    /// <summary>
    /// Gets the remaining Stun boxes.
    /// </summary>
    public int StunRemaining => Math.Max(0, StunBoxes - StunDamage);

    /// <summary>
    /// Gets a value indicating whether the character is unconscious (Stun track full).
    /// </summary>
    public bool IsUnconscious => StunDamage >= StunBoxes;

    /// <summary>
    /// Gets a value indicating whether the character is dying (Physical track full).
    /// </summary>
    public bool IsDying => PhysicalDamage >= PhysicalBoxes;

    /// <summary>
    /// Gets the wound modifier based on damage taken (every 3 boxes = -1 dice).
    /// </summary>
    public int WoundModifier
    {
        get
        {
            int physicalModifier = PhysicalDamage / 3;
            int stunModifier = StunDamage / 3;
            return -(physicalModifier + stunModifier);
        }
    }

    /// <summary>
    /// Creates a new ConditionMonitor based on character attributes.
    /// </summary>
    /// <param name="attributes">The character's attributes.</param>
    /// <returns>A new ConditionMonitor instance.</returns>
    public static ConditionMonitor ForAttributes(AttributeSet attributes)
    {
        int physicalBoxes = 8 + (int)Math.Ceiling(attributes.Body / 2.0);
        int stunBoxes = 8 + (int)Math.Ceiling(attributes.Willpower / 2.0);
        
        return new ConditionMonitor(physicalBoxes, stunBoxes, 0, 0);
    }

    /// <summary>
    /// Creates a new ConditionMonitor with specific values.
    /// </summary>
    /// <param name="physicalBoxes">Total Physical damage boxes.</param>
    /// <param name="stunBoxes">Total Stun damage boxes.</param>
    /// <param name="physicalDamage">Current Physical damage.</param>
    /// <param name="stunDamage">Current Stun damage.</param>
    /// <returns>A Result containing the new ConditionMonitor or an error.</returns>
    public static Result<ConditionMonitor> Create(
        int physicalBoxes,
        int stunBoxes,
        int physicalDamage,
        int stunDamage) =>
        new ValidationBuilder<ConditionMonitor>()
            .RuleFor(x => x.PhysicalBoxes, physicalBoxes)
                .GreaterThanOrEqualTo(8)
                .WithMessage("Physical boxes must be at least 8")
            .RuleFor(x => x.StunBoxes, stunBoxes)
                .GreaterThanOrEqualTo(8)
                .WithMessage("Stun boxes must be at least 8")
            .RuleFor(x => x.PhysicalDamage, physicalDamage)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Physical damage cannot be negative")
            .RuleFor(x => x.StunDamage, stunDamage)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Stun damage cannot be negative")
            .Build(() => new ConditionMonitor(physicalBoxes, stunBoxes, physicalDamage, stunDamage));

    /// <summary>
    /// Takes Physical damage.
    /// </summary>
    /// <param name="amount">The amount of damage to take.</param>
    /// <returns>A Result containing the new ConditionMonitor or an error.</returns>
    public Result<ConditionMonitor> TakePhysicalDamage(int amount)
    {
        if (amount < 0)
            return Result.Failure<ConditionMonitor>("Damage amount cannot be negative.");

        int newDamage = PhysicalDamage + amount;
        
        return Result.Success(
            new ConditionMonitor(PhysicalBoxes, StunBoxes, newDamage, StunDamage));
    }

    /// <summary>
    /// Takes Stun damage.
    /// </summary>
    /// <param name="amount">The amount of damage to take.</param>
    /// <returns>A Result containing the new ConditionMonitor or an error.</returns>
    public Result<ConditionMonitor> TakeStunDamage(int amount)
    {
        if (amount < 0)
            return Result.Failure<ConditionMonitor>("Damage amount cannot be negative.");

        int newStunDamage = StunDamage + amount;
        int overflow = 0;
        
        if (newStunDamage > StunBoxes)
        {
            overflow = newStunDamage - StunBoxes;
            newStunDamage = StunBoxes;
        }

        int newPhysicalDamage = PhysicalDamage + overflow;
        
        return Result.Success(
            new ConditionMonitor(PhysicalBoxes, StunBoxes, newPhysicalDamage, newStunDamage));
    }

    /// <summary>
    /// Heals Physical damage.
    /// </summary>
    /// <param name="amount">The amount of damage to heal.</param>
    /// <returns>A Result containing the new ConditionMonitor or an error.</returns>
    public Result<ConditionMonitor> HealPhysicalDamage(int amount)
    {
        if (amount < 0)
            return Result.Failure<ConditionMonitor>("Heal amount cannot be negative.");

        int newDamage = Math.Max(0, PhysicalDamage - amount);
        
        return Result.Success(
            new ConditionMonitor(PhysicalBoxes, StunBoxes, newDamage, StunDamage));
    }

    /// <summary>
    /// Heals Stun damage.
    /// </summary>
    /// <param name="amount">The amount of damage to heal.</param>
    /// <returns>A Result containing the new ConditionMonitor or an error.</returns>
    public Result<ConditionMonitor> HealStunDamage(int amount)
    {
        if (amount < 0)
            return Result.Failure<ConditionMonitor>("Heal amount cannot be negative.");

        int newDamage = Math.Max(0, StunDamage - amount);
        
        return Result.Success(
            new ConditionMonitor(PhysicalBoxes, StunBoxes, PhysicalDamage, newDamage));
    }

    /// <summary>
    /// Heals all damage.
    /// </summary>
    /// <returns>A new ConditionMonitor with no damage.</returns>
    public ConditionMonitor HealAll()
    {
        return new ConditionMonitor(PhysicalBoxes, StunBoxes, 0, 0);
    }

    /// <summary>
    /// Gets the atomic values that define this value object.
    /// </summary>
    /// <returns>The collection of atomic values.</returns>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return PhysicalBoxes;
        yield return StunBoxes;
        yield return PhysicalDamage;
        yield return StunDamage;
    }

    /// <summary>
    /// Returns the string representation of the ConditionMonitor.
    /// </summary>
    /// <returns>The string representation.</returns>
    public override string ToString() => 
        $"Physical: {PhysicalRemaining}/{PhysicalBoxes}, Stun: {StunRemaining}/{StunBoxes}";
}