using FlowRight.Core.Results;
using FlowRight.Validation.Builders;
using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Mission;

/// <summary>
/// Represents a chat message in a game session.
/// </summary>
public sealed class ChatMessage : ValueObject
{
    private ChatMessage(string sender, string content, MessageType type, DateTime timestamp)
    {
        Sender = sender;
        Content = content;
        Type = type;
        Timestamp = timestamp;
    }

    /// <summary>
    /// Gets the message sender.
    /// </summary>
    public string Sender { get; }

    /// <summary>
    /// Gets the message content.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets the message type.
    /// </summary>
    public MessageType Type { get; }

    /// <summary>
    /// Gets the message timestamp.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    /// Creates a new chat message.
    /// </summary>
    /// <param name="sender">The message sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="type">The message type.</param>
    /// <returns>A Result containing the new message or an error.</returns>
    public static Result<ChatMessage> Create(string sender, string content, MessageType type)
    {
        return ValidateAndCreate(sender, content, type, DateTime.UtcNow);
    }

    /// <summary>
    /// Creates a new chat message with specified timestamp (for testing).
    /// </summary>
    /// <param name="sender">The message sender.</param>
    /// <param name="content">The message content.</param>
    /// <param name="type">The message type.</param>
    /// <param name="timestamp">The message timestamp.</param>
    /// <returns>A Result containing the new message or an error.</returns>
    internal static Result<ChatMessage> CreateForTesting(string sender, string content, MessageType type, DateTime timestamp)
    {
        return ValidateAndCreate(sender, content, type, timestamp);
    }

    /// <summary>
    /// Validates the input and creates a new chat message.
    /// </summary>
    private static Result<ChatMessage> ValidateAndCreate(string sender, string content, MessageType type, DateTime timestamp)
    {
        // Trim values before validation
        string trimmedSender = sender?.Trim() ?? string.Empty;
        string trimmedContent = content?.Trim() ?? string.Empty;

        ValidationBuilder<ChatMessage> builder = new();

        return builder
            .RuleFor(x => x.Sender, trimmedSender)
                .NotEmpty()
                .WithMessage("Message sender is required")
                .MaximumLength(50)
                .WithMessage("Sender name cannot exceed 50 characters")
            .RuleFor(x => x.Content, trimmedContent)
                .NotEmpty()
                .WithMessage("Message content is required")
                .MaximumLength(5000)
                .WithMessage("Message content cannot exceed 5000 characters")
            .Build(() => new ChatMessage(
                trimmedSender,
                trimmedContent,
                type,
                timestamp));
    }

    /// <summary>
    /// Gets the atomic values that define this value object.
    /// </summary>
    /// <returns>The collection of atomic values.</returns>
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Sender;
        yield return Content;
        yield return Type;
        yield return Timestamp;
    }
}

/// <summary>
/// Represents the type of a chat message.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// A message from the player.
    /// </summary>
    Player,

    /// <summary>
    /// A message from the game master (AI).
    /// </summary>
    GameMaster,

    /// <summary>
    /// A system message (dice rolls, events, etc.).
    /// </summary>
    System,

    /// <summary>
    /// A narrative description.
    /// </summary>
    Narrative
}