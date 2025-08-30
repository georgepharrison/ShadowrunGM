using ShadowrunGM.Domain.Mission;

namespace ShadowrunGM.Domain.Tests.Mission.ValueObjects;

/// <summary>
/// Comprehensive test suite for ChatMessage value object behavior.
/// Tests creation, validation, message types, and immutability.
/// </summary>
public sealed class ChatMessageTests
{
    public sealed class Create
    {
        [Fact]
        public void Create_WithValidPlayerMessage_ShouldSucceed()
        {
            // Arrange
            string sender = "PlayerOne";
            string content = "I want to hack the mainframe!";
            MessageType type = MessageType.Player;

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, content, type);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Sender.ShouldBe(sender);
            message.Content.ShouldBe(content);
            message.Type.ShouldBe(type);
            message.Timestamp.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public void Create_WithValidGameMasterMessage_ShouldSucceed()
        {
            // Arrange
            string sender = "GM";
            string content = "Roll initiative!";
            MessageType type = MessageType.GameMaster;

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, content, type);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Sender.ShouldBe(sender);
            message.Content.ShouldBe(content);
            message.Type.ShouldBe(type);
        }

        [Fact]
        public void Create_WithValidSystemMessage_ShouldSucceed()
        {
            // Arrange
            string sender = "System";
            string content = "PlayerOne rolled 5 hits!";
            MessageType type = MessageType.System;

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, content, type);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Type.ShouldBe(type);
        }

        [Fact]
        public void Create_WithValidNarrativeMessage_ShouldSucceed()
        {
            // Arrange
            string sender = "Narrator";
            string content = "The neon lights flicker in the rain-soaked streets of Seattle...";
            MessageType type = MessageType.Narrative;

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, content, type);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Type.ShouldBe(type);
        }

        [Fact]
        public void Create_ShouldTrimSenderAndContent()
        {
            // Arrange
            string sender = "  Player  ";
            string content = "  Hello there!  ";
            MessageType type = MessageType.Player;

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, content, type);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Sender.ShouldBe("Player");
            message.Content.ShouldBe("Hello there!");
        }

        [Fact]
        public void Create_ShouldSetTimestampToUtcNow()
        {
            // Arrange
            DateTime beforeCreation = DateTime.UtcNow;
            string sender = "Player";
            string content = "Test message";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, content, MessageType.Player);
            DateTime afterCreation = DateTime.UtcNow;

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Timestamp.ShouldBeInRange(beforeCreation, afterCreation);
            message.Timestamp.Kind.ShouldBe(DateTimeKind.Utc);
        }
    }

    public sealed class SenderValidation
    {
        [Fact]
        public void Create_WithEmptySender_ShouldReturnFailure()
        {
            // Arrange
            string emptySender = string.Empty;
            string content = "Test message";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(emptySender, content, MessageType.Player);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Message sender is required.");
        }

        [Fact]
        public void Create_WithNullSender_ShouldReturnFailure()
        {
            // Arrange
            string nullSender = null!;
            string content = "Test message";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(nullSender, content, MessageType.Player);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Message sender is required.");
        }

        [Fact]
        public void Create_WithWhitespaceSender_ShouldReturnFailure()
        {
            // Arrange
            string whitespaceSender = "   ";
            string content = "Test message";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(whitespaceSender, content, MessageType.Player);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Message sender is required.");
        }

        [Fact]
        public void Create_WithSenderAtMaxLength_ShouldSucceed()
        {
            // Arrange
            string maxLengthSender = new('A', 50); // Exactly 50 characters
            string content = "Test message";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(maxLengthSender, content, MessageType.Player);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Sender.ShouldBe(maxLengthSender);
        }

        [Fact]
        public void Create_WithSenderExceedingMaxLength_ShouldReturnFailure()
        {
            // Arrange
            string tooLongSender = new('A', 51); // 51 characters, exceeds limit
            string content = "Test message";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(tooLongSender, content, MessageType.Player);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Sender name cannot exceed 50 characters.");
        }

        [Theory]
        [InlineData("A")]
        [InlineData("Player")]
        [InlineData("GameMaster")]
        [InlineData("System")]
        public void Create_WithValidSenderLengths_ShouldSucceed(string validSender)
        {
            // Arrange
            string content = "Test message";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(validSender, content, MessageType.Player);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Sender.ShouldBe(validSender);
        }
    }

    public sealed class ContentValidation
    {
        [Fact]
        public void Create_WithEmptyContent_ShouldReturnFailure()
        {
            // Arrange
            string sender = "Player";
            string emptyContent = string.Empty;

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, emptyContent, MessageType.Player);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Message content is required.");
        }

        [Fact]
        public void Create_WithNullContent_ShouldReturnFailure()
        {
            // Arrange
            string sender = "Player";
            string nullContent = null!;

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, nullContent, MessageType.Player);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Message content is required.");
        }

        [Fact]
        public void Create_WithWhitespaceContent_ShouldReturnFailure()
        {
            // Arrange
            string sender = "Player";
            string whitespaceContent = "   ";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, whitespaceContent, MessageType.Player);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Message content is required.");
        }

        [Fact]
        public void Create_WithContentAtMaxLength_ShouldSucceed()
        {
            // Arrange
            string sender = "Player";
            string maxLengthContent = new('A', 5000); // Exactly 5000 characters

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, maxLengthContent, MessageType.Player);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Content.ShouldBe(maxLengthContent);
        }

        [Fact]
        public void Create_WithContentExceedingMaxLength_ShouldReturnFailure()
        {
            // Arrange
            string sender = "Player";
            string tooLongContent = new('A', 5001); // 5001 characters, exceeds limit

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, tooLongContent, MessageType.Player);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Message content cannot exceed 5000 characters.");
        }

        [Fact]
        public void Create_WithShortContent_ShouldSucceed()
        {
            // Arrange
            string sender = "Player";
            string shortContent = "Hi";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, shortContent, MessageType.Player);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Content.ShouldBe(shortContent);
        }

        [Fact]
        public void Create_WithLongValidContent_ShouldSucceed()
        {
            // Arrange
            string sender = "Player";
            string longContent = "This is a very long message that contains a lot of text but is still within the valid range. " +
                               "It describes in detail the character's actions and thoughts as they navigate through the cyberpunk " +
                               "world of Shadowrun, encountering various obstacles and making important decisions that will affect " +
                               "the outcome of their mission.";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, longContent, MessageType.Player);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Content.ShouldBe(longContent);
        }

        [Fact]
        public void Create_WithSpecialCharactersInContent_ShouldSucceed()
        {
            // Arrange
            string sender = "Player";
            string contentWithSpecialChars = "I use my cyberdeck to hack into the system! @#$%^&*()_+-=[]{}|;':\",./<>?";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, contentWithSpecialChars, MessageType.Player);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Content.ShouldBe(contentWithSpecialChars);
        }

        [Fact]
        public void Create_WithUnicodeCharactersInContent_ShouldSucceed()
        {
            // Arrange
            string sender = "Player";
            string unicodeContent = "こんにちは世界! 🌟 This message contains Unicode characters: ñáéíóú";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, unicodeContent, MessageType.Player);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Content.ShouldBe(unicodeContent);
        }
    }

    public sealed class MessageTypeHandling
    {
        [Theory]
        [InlineData(MessageType.Player)]
        [InlineData(MessageType.GameMaster)]
        [InlineData(MessageType.System)]
        [InlineData(MessageType.Narrative)]
        public void Create_WithAllMessageTypes_ShouldSucceed(MessageType messageType)
        {
            // Arrange
            string sender = "TestSender";
            string content = "Test content";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, content, messageType);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Type.ShouldBe(messageType);
        }

        [Fact]
        public void Create_WithInvalidEnumValue_ShouldStillWork()
        {
            // Arrange - This tests behavior with invalid enum values
            string sender = "TestSender";
            string content = "Test content";
            MessageType invalidType = (MessageType)999;

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, content, invalidType);

            // Assert - The create method doesn't validate enum values, so it should work
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Type.ShouldBe(invalidType);
        }
    }

    public sealed class ValueObjectBehavior
    {
        [Fact]
        public void TwoChatMessages_WithSameValues_ShouldBeEqual()
        {
            // Arrange - Create two messages with identical content
            DateTime fixedTime = new(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            
            // We need to create messages with the same timestamp for proper equality testing
            // This requires understanding how ChatMessage handles timestamp in equality
            ChatMessage first = CreateMessageWithFixedTimestamp("Player", "Hello", MessageType.Player, fixedTime);
            ChatMessage second = CreateMessageWithFixedTimestamp("Player", "Hello", MessageType.Player, fixedTime);

            // Act & Assert
            first.ShouldBe(second);
            first.GetHashCode().ShouldBe(second.GetHashCode());
        }

        [Fact]
        public void TwoChatMessages_WithDifferentSender_ShouldNotBeEqual()
        {
            // Arrange
            DateTime fixedTime = new(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            ChatMessage first = CreateMessageWithFixedTimestamp("Player1", "Hello", MessageType.Player, fixedTime);
            ChatMessage second = CreateMessageWithFixedTimestamp("Player2", "Hello", MessageType.Player, fixedTime);

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void TwoChatMessages_WithDifferentContent_ShouldNotBeEqual()
        {
            // Arrange
            DateTime fixedTime = new(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            ChatMessage first = CreateMessageWithFixedTimestamp("Player", "Hello", MessageType.Player, fixedTime);
            ChatMessage second = CreateMessageWithFixedTimestamp("Player", "Goodbye", MessageType.Player, fixedTime);

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void TwoChatMessages_WithDifferentType_ShouldNotBeEqual()
        {
            // Arrange
            DateTime fixedTime = new(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            ChatMessage first = CreateMessageWithFixedTimestamp("Sender", "Message", MessageType.Player, fixedTime);
            ChatMessage second = CreateMessageWithFixedTimestamp("Sender", "Message", MessageType.GameMaster, fixedTime);

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void TwoChatMessages_WithDifferentTimestamp_ShouldNotBeEqual()
        {
            // Arrange
            DateTime time1 = new(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            DateTime time2 = new(2023, 1, 1, 12, 0, 1, DateTimeKind.Utc); // 1 second difference
            
            ChatMessage first = CreateMessageWithFixedTimestamp("Player", "Hello", MessageType.Player, time1);
            ChatMessage second = CreateMessageWithFixedTimestamp("Player", "Hello", MessageType.Player, time2);

            // Act & Assert
            first.ShouldNotBe(second);
        }

        [Fact]
        public void ChatMessage_ShouldBeImmutable()
        {
            // Arrange
            Result<ChatMessage> result = ChatMessage.Create("Player", "Test", MessageType.Player);
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();

            // Act & Assert - Value object should have no public setters
            typeof(ChatMessage).GetProperty("Sender")?.SetMethod.ShouldBeNull();
            typeof(ChatMessage).GetProperty("Content")?.SetMethod.ShouldBeNull();
            typeof(ChatMessage).GetProperty("Type")?.SetMethod.ShouldBeNull();
            typeof(ChatMessage).GetProperty("Timestamp")?.SetMethod.ShouldBeNull();
        }

        [Fact]
        public void ChatMessage_WithNullComparison_ShouldNotBeEqual()
        {
            // Arrange
            Result<ChatMessage> result = ChatMessage.Create("Player", "Test", MessageType.Player);
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();

            // Act & Assert
            message.ShouldNotBe(null);
            message!.Equals(null).ShouldBeFalse();
        }

        [Fact]
        public void ChatMessage_WithDifferentType_ShouldNotBeEqual()
        {
            // Arrange
            Result<ChatMessage> result = ChatMessage.Create("Player", "Test", MessageType.Player);
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            object differentType = "not a chat message";

            // Act & Assert
            message!.Equals(differentType).ShouldBeFalse();
        }

        // Helper method to create messages with fixed timestamps for equality testing
        private static ChatMessage CreateMessageWithFixedTimestamp(
            string sender, string content, MessageType type, DateTime timestamp)
        {
            Result<ChatMessage> result = ChatMessage.CreateForTesting(sender, content, type, timestamp);
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            return message!;
        }
    }

    public sealed class EdgeCases
    {
        [Fact]
        public void Create_WithBothSenderAndContentAtMaxLength_ShouldSucceed()
        {
            // Arrange
            string maxSender = new('A', 50);
            string maxContent = new('B', 5000);

            // Act
            Result<ChatMessage> result = ChatMessage.Create(maxSender, maxContent, MessageType.Player);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Sender.ShouldBe(maxSender);
            message.Content.ShouldBe(maxContent);
        }

        [Fact]
        public void Create_WithMinimumValidValues_ShouldSucceed()
        {
            // Arrange
            string minSender = "A";
            string minContent = "B";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(minSender, minContent, MessageType.Player);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Sender.ShouldBe(minSender);
            message.Content.ShouldBe(minContent);
        }

        [Fact]
        public void Create_WithNewlinesAndTabsInContent_ShouldSucceed()
        {
            // Arrange
            string sender = "Player";
            string contentWithWhitespace = "Line 1\nLine 2\r\nLine 3\tWith tab";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, contentWithWhitespace, MessageType.Player);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out ChatMessage? message).ShouldBeTrue();
            message.ShouldNotBeNull();
            message.Content.ShouldBe(contentWithWhitespace.Trim()); // Should be trimmed but preserve internal whitespace
        }

        [Fact]
        public void Create_WithMultipleValidationErrors_ShouldReturnFirstError()
        {
            // Arrange - Both sender and content are invalid
            string emptySender = string.Empty;
            string emptyContent = string.Empty;

            // Act
            Result<ChatMessage> result = ChatMessage.Create(emptySender, emptyContent, MessageType.Player);

            // Assert - Should return the first validation error encountered
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldBe("Message sender is required.");
        }

        [Fact]
        public void Create_RepeatedCalls_ShouldHaveDifferentTimestamps()
        {
            // Arrange
            string sender = "Player";
            string content = "Test message";

            // Act
            Result<ChatMessage> first = ChatMessage.Create(sender, content, MessageType.Player);
            Thread.Sleep(1); // Ensure different timestamps
            Result<ChatMessage> second = ChatMessage.Create(sender, content, MessageType.Player);

            // Assert
            first.IsSuccess.ShouldBeTrue();
            second.IsSuccess.ShouldBeTrue();
            
            first.TryGetValue(out ChatMessage? firstMessage).ShouldBeTrue();
            second.TryGetValue(out ChatMessage? secondMessage).ShouldBeTrue();
            
            firstMessage!.Timestamp.ShouldNotBe(secondMessage!.Timestamp);
            secondMessage.Timestamp.ShouldBeGreaterThan(firstMessage.Timestamp);
        }
    }

    public sealed class BoundaryTesting
    {
        [Theory]
        [InlineData(49)] // One below max
        [InlineData(50)] // Exactly at max
        public void Create_WithSenderAtBoundaryLengths_ShouldSucceed(int length)
        {
            // Arrange
            string sender = new('A', length);
            string content = "Test";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, content, MessageType.Player);

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Theory]
        [InlineData(51)] // One above max
        [InlineData(100)] // Way above max
        public void Create_WithSenderAboveBoundaryLengths_ShouldFail(int length)
        {
            // Arrange
            string sender = new('A', length);
            string content = "Test";

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, content, MessageType.Player);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("50 characters");
        }

        [Theory]
        [InlineData(4999)] // One below max
        [InlineData(5000)] // Exactly at max
        public void Create_WithContentAtBoundaryLengths_ShouldSucceed(int length)
        {
            // Arrange
            string sender = "Player";
            string content = new('A', length);

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, content, MessageType.Player);

            // Assert
            result.IsSuccess.ShouldBeTrue();
        }

        [Theory]
        [InlineData(5001)] // One above max
        [InlineData(10000)] // Way above max
        public void Create_WithContentAboveBoundaryLengths_ShouldFail(int length)
        {
            // Arrange
            string sender = "Player";
            string content = new('A', length);

            // Act
            Result<ChatMessage> result = ChatMessage.Create(sender, content, MessageType.Player);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("5000 characters");
        }
    }
}