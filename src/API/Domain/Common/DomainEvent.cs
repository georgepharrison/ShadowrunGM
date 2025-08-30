namespace ShadowrunGM.Domain.Common;

/// <summary>
/// Base class for all domain events.
/// </summary>
public abstract record DomainEvent
{
    /// <summary>
    /// Gets the date and time when the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}