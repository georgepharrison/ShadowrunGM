using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Character.Events;

/// <summary>
/// Domain event raised when Edge is spent.
/// </summary>
public sealed record EdgeSpent : DomainEvent
{
    /// <summary>
    /// Initializes a new instance of the EdgeSpent event.
    /// </summary>
    /// <param name="characterId">The ID of the character who spent Edge.</param>
    /// <param name="amount">The amount of Edge spent.</param>
    /// <param name="purpose">The purpose for spending Edge.</param>
    public EdgeSpent(CharacterId characterId, int amount, string purpose)
    {
        CharacterId = characterId;
        Amount = amount;
        Purpose = purpose;
    }

    /// <summary>
    /// Gets the ID of the character who spent Edge.
    /// </summary>
    public CharacterId CharacterId { get; }

    /// <summary>
    /// Gets the amount of Edge spent.
    /// </summary>
    public int Amount { get; }

    /// <summary>
    /// Gets the purpose for spending Edge.
    /// </summary>
    public string Purpose { get; }
}