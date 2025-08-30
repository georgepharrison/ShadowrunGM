using ShadowrunGM.ApiSdk.Common.Results;
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
        if (string.IsNullOrWhiteSpace(sender))
            return Result.Failure<ChatMessage>("Message sender is required.");

        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure<ChatMessage>("Message content is required.");

        if (sender.Length > 50)
            return Result.Failure<ChatMessage>("Sender name cannot exceed 50 characters.");

        if (content.Length > 5000)
            return Result.Failure<ChatMessage>("Message content cannot exceed 5000 characters.");

        return Result.Success(new ChatMessage(
            sender.Trim(),
            content.Trim(),
            type,
            DateTime.UtcNow));
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
        if (string.IsNullOrWhiteSpace(sender))
            return Result.Failure<ChatMessage>("Message sender is required.");

        if (string.IsNullOrWhiteSpace(content))
            return Result.Failure<ChatMessage>("Message content is required.");

        if (sender.Length > 50)
            return Result.Failure<ChatMessage>("Sender name cannot exceed 50 characters.");

        if (content.Length > 5000)
            return Result.Failure<ChatMessage>("Message content cannot exceed 5000 characters.");

        return Result.Success(new ChatMessage(
            sender.Trim(),
            content.Trim(),
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