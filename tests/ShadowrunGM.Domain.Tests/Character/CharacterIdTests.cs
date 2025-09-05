using Shouldly;
using ShadowrunGM.Domain.Character;
using Xunit;

namespace ShadowrunGM.Domain.Tests.Character;

/// <summary>
/// Tests for the CharacterId strongly-typed identifier.
/// Tests follow TDD principles to define expected behavior for type-safe identifiers.
/// </summary>
public sealed class CharacterIdTests
{
    public sealed class Construction
    {
        [Fact]
        public void New_ShouldCreateUniqueId()
        {
            // Act
            CharacterId id1 = CharacterId.New();
            CharacterId id2 = CharacterId.New();

            // Assert
            id1.ShouldNotBe(id2);
            id1.Value.ShouldNotBe(Guid.Empty);
            id2.Value.ShouldNotBe(Guid.Empty);
            id1.Value.ShouldNotBe(id2.Value);
        }

        [Fact]
        public void From_WithValidGuid_ShouldCreateId()
        {
            // Arrange
            Guid testGuid = Guid.NewGuid();

            // Act
            CharacterId id = CharacterId.From(testGuid);

            // Assert
            id.Value.ShouldBe(testGuid);
        }

        [Fact]
        public void From_WithEmptyGuid_ShouldStillCreateId()
        {
            // Arrange
            Guid emptyGuid = Guid.Empty;

            // Act
            CharacterId id = CharacterId.From(emptyGuid);

            // Assert
            id.Value.ShouldBe(emptyGuid);
            // Note: CharacterId doesn't validate - it's a simple wrapper
            // Validation should happen at the aggregate level if needed
        }

        [Fact]
        public void From_WithSameGuid_ShouldCreateEqualIds()
        {
            // Arrange
            Guid testGuid = Guid.NewGuid();

            // Act
            CharacterId id1 = CharacterId.From(testGuid);
            CharacterId id2 = CharacterId.From(testGuid);

            // Assert
            id1.ShouldBe(id2);
            id1.Value.ShouldBe(id2.Value);
        }
    }

    public sealed class RecordStructBehavior
    {
        [Fact]
        public void Equality_WithSameValue_ShouldBeEqual()
        {
            // Arrange
            Guid testGuid = Guid.NewGuid();

            // Act
            CharacterId id1 = CharacterId.From(testGuid);
            CharacterId id2 = CharacterId.From(testGuid);

            // Assert
            id1.ShouldBe(id2);
            id1.GetHashCode().ShouldBe(id2.GetHashCode());
            (id1 == id2).ShouldBeTrue();
            (id1 != id2).ShouldBeFalse();
        }

        [Fact]
        public void Equality_WithDifferentValues_ShouldNotBeEqual()
        {
            // Arrange
            CharacterId id1 = CharacterId.New();
            CharacterId id2 = CharacterId.New();

            // Act & Assert
            id1.ShouldNotBe(id2);
            (id1 == id2).ShouldBeFalse();
            (id1 != id2).ShouldBeTrue();
        }

        [Fact]
        public void GetHashCode_ShouldBeConsistent()
        {
            // Arrange
            CharacterId id = CharacterId.New();

            // Act
            int hashCode1 = id.GetHashCode();
            int hashCode2 = id.GetHashCode();

            // Assert
            hashCode1.ShouldBe(hashCode2);
        }

        [Fact]
        public void GetHashCode_WithEqualIds_ShouldHaveSameHashCode()
        {
            // Arrange
            Guid testGuid = Guid.NewGuid();
            CharacterId id1 = CharacterId.From(testGuid);
            CharacterId id2 = CharacterId.From(testGuid);

            // Act & Assert
            id1.GetHashCode().ShouldBe(id2.GetHashCode());
        }
    }

    public sealed class ImplicitConversion
    {
        [Fact]
        public void ImplicitConversionToGuid_ShouldReturnUnderlyingValue()
        {
            // Arrange
            Guid testGuid = Guid.NewGuid();
            CharacterId id = CharacterId.From(testGuid);

            // Act
            Guid convertedGuid = id; // Implicit conversion

            // Assert
            convertedGuid.ShouldBe(testGuid);
        }

        [Fact]
        public void ImplicitConversion_ShouldWorkInMethodParameters()
        {
            // Arrange
            CharacterId id = CharacterId.New();

            // Act - Method that expects Guid should accept CharacterId implicitly
            bool result = AcceptsGuid(id);

            // Assert
            result.ShouldBeTrue();
        }

        private static bool AcceptsGuid(Guid guid)
        {
            return guid != Guid.Empty;
        }

        [Fact]
        public void ImplicitConversion_ShouldWorkInAssignments()
        {
            // Arrange
            CharacterId id = CharacterId.New();

            // Act
            Guid assignedGuid = id;
            
            // Assert
            assignedGuid.ShouldBe(id.Value);
        }
    }

    public sealed class StringRepresentation
    {
        [Fact]
        public void ToString_ShouldReturnGuidStringRepresentation()
        {
            // Arrange
            Guid testGuid = Guid.NewGuid();
            CharacterId id = CharacterId.From(testGuid);

            // Act
            string idString = id.ToString();

            // Assert
            idString.ShouldBe(testGuid.ToString());
        }

        [Fact]
        public void ToString_WithEmptyGuid_ShouldReturnEmptyGuidString()
        {
            // Arrange
            CharacterId id = CharacterId.From(Guid.Empty);

            // Act
            string idString = id.ToString();

            // Assert
            idString.ShouldBe(Guid.Empty.ToString());
        }

        [Fact]
        public void ToString_ShouldBeConsistent()
        {
            // Arrange
            CharacterId id = CharacterId.New();

            // Act
            string string1 = id.ToString();
            string string2 = id.ToString();

            // Assert
            string1.ShouldBe(string2);
        }
    }

    public sealed class ValueProperty
    {
        [Fact]
        public void Value_ShouldReturnUnderlyingGuid()
        {
            // Arrange
            Guid testGuid = Guid.NewGuid();
            CharacterId id = CharacterId.From(testGuid);

            // Act
            Guid value = id.Value;

            // Assert
            value.ShouldBe(testGuid);
        }

        [Fact]
        public void Value_ShouldBeReadOnly()
        {
            // Arrange
            CharacterId id = CharacterId.New();
            
            // Act & Assert - Value should be read-only property
            Type idType = typeof(CharacterId);
            System.Reflection.PropertyInfo? valueProperty = idType.GetProperty(nameof(CharacterId.Value));
            
            valueProperty.ShouldNotBeNull();
            valueProperty.CanRead.ShouldBeTrue();
            valueProperty.CanWrite.ShouldBeFalse(); // Should be read-only
        }
    }

    public sealed class TypeSafety
    {
        [Fact]
        public void CharacterId_ShouldPreventAccidentalMixupWithRawGuids()
        {
            // This test documents the type safety benefit of strongly-typed identifiers
            // Arrange
            CharacterId characterId = CharacterId.New();
            Guid rawGuid = Guid.NewGuid();

            // Act & Assert - These should be different types
            characterId.ShouldBeOfType<CharacterId>();
            rawGuid.ShouldBeOfType<Guid>();
            
            // CharacterId should not be directly assignable from Guid without explicit conversion
            // (This is enforced by the compiler, not a runtime test)
        }

        [Fact]
        public void CharacterId_ShouldBeValueType()
        {
            // Arrange & Act
            CharacterId id = CharacterId.New();

            // Assert - Should be a value type (struct) for performance
            typeof(CharacterId).IsValueType.ShouldBeTrue();
        }

        [Fact]
        public void CharacterId_ShouldBeReadOnly()
        {
            // This test documents immutability expectations
            // Arrange
            Guid originalGuid = Guid.NewGuid();
            CharacterId id = CharacterId.From(originalGuid);

            // Assert - CharacterId should be immutable (readonly record struct)
            id.Value.ShouldBe(originalGuid);
            
            // The record struct ensures immutability at compile time
            // No runtime modification should be possible
        }
    }

    public sealed class UsagePatterns
    {
        [Fact]
        public void CharacterId_ShouldWorkInCollections()
        {
            // Arrange
            CharacterId id1 = CharacterId.New();
            CharacterId id2 = CharacterId.New();
            CharacterId id3 = CharacterId.New();

            // Act
            List<CharacterId> idList = [id1, id2, id3];
            HashSet<CharacterId> idSet = [id1, id2, id3, id1]; // Duplicate id1

            // Assert
            idList.Count.ShouldBe(3);
            idSet.Count.ShouldBe(3); // HashSet should remove duplicate
            
            idList.Contains(id1).ShouldBeTrue();
            idSet.Contains(id2).ShouldBeTrue();
        }

        [Fact]
        public void CharacterId_ShouldWorkAsDictionaryKey()
        {
            // Arrange
            CharacterId id1 = CharacterId.New();
            CharacterId id2 = CharacterId.New();
            
            Dictionary<CharacterId, string> characterNames = new()
            {
                [id1] = "Alice",
                [id2] = "Bob"
            };

            // Act & Assert
            characterNames[id1].ShouldBe("Alice");
            characterNames[id2].ShouldBe("Bob");
            characterNames.ContainsKey(id1).ShouldBeTrue();
            characterNames.ContainsKey(CharacterId.New()).ShouldBeFalse();
        }

        [Fact]
        public void CharacterId_ShouldSortConsistently()
        {
            // Arrange
            List<CharacterId> ids = [
                CharacterId.New(),
                CharacterId.New(),
                CharacterId.New(),
                CharacterId.New()
            ];

            // Act
            List<CharacterId> sortedIds = [.. ids.OrderBy(id => id.Value)];

            // Assert
            sortedIds.Count.ShouldBe(4);
            // Should be able to sort by the underlying Guid values
            for (int i = 0; i < sortedIds.Count - 1; i++)
            {
                sortedIds[i].Value.ShouldBeLessThanOrEqualTo(sortedIds[i + 1].Value);
            }
        }
    }
}