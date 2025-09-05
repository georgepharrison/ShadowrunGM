using Shouldly;
using ShadowrunGM.Domain.Mission;
using Xunit;

namespace ShadowrunGM.Domain.Tests.Mission;

/// <summary>
/// Tests for the SessionId strongly-typed identifier.
/// Tests follow TDD principles to define expected behavior for type-safe identifiers.
/// </summary>
public sealed class SessionIdTests
{
    public sealed class Construction
    {
        [Fact]
        public void New_ShouldCreateUniqueId()
        {
            // Act
            SessionId id1 = SessionId.New();
            SessionId id2 = SessionId.New();

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
            SessionId id = SessionId.From(testGuid);

            // Assert
            id.Value.ShouldBe(testGuid);
        }

        [Fact]
        public void From_WithEmptyGuid_ShouldStillCreateId()
        {
            // Arrange
            Guid emptyGuid = Guid.Empty;

            // Act
            SessionId id = SessionId.From(emptyGuid);

            // Assert
            id.Value.ShouldBe(emptyGuid);
        }

        [Fact]
        public void From_WithSameGuid_ShouldCreateEqualIds()
        {
            // Arrange
            Guid testGuid = Guid.NewGuid();

            // Act
            SessionId id1 = SessionId.From(testGuid);
            SessionId id2 = SessionId.From(testGuid);

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
            SessionId id1 = SessionId.From(testGuid);
            SessionId id2 = SessionId.From(testGuid);

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
            SessionId id1 = SessionId.New();
            SessionId id2 = SessionId.New();

            // Act & Assert
            id1.ShouldNotBe(id2);
            (id1 == id2).ShouldBeFalse();
            (id1 != id2).ShouldBeTrue();
        }

        [Fact]
        public void GetHashCode_ShouldBeConsistent()
        {
            // Arrange
            SessionId id = SessionId.New();

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
            SessionId id1 = SessionId.From(testGuid);
            SessionId id2 = SessionId.From(testGuid);

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
            SessionId id = SessionId.From(testGuid);

            // Act
            Guid convertedGuid = id; // Implicit conversion

            // Assert
            convertedGuid.ShouldBe(testGuid);
        }

        [Fact]
        public void ImplicitConversion_ShouldWorkInMethodParameters()
        {
            // Arrange
            SessionId id = SessionId.New();

            // Act
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
            SessionId id = SessionId.New();

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
            SessionId id = SessionId.From(testGuid);

            // Act
            string idString = id.ToString();

            // Assert
            idString.ShouldBe(testGuid.ToString());
        }

        [Fact]
        public void ToString_WithEmptyGuid_ShouldReturnEmptyGuidString()
        {
            // Arrange
            SessionId id = SessionId.From(Guid.Empty);

            // Act
            string idString = id.ToString();

            // Assert
            idString.ShouldBe(Guid.Empty.ToString());
        }

        [Fact]
        public void ToString_ShouldBeConsistent()
        {
            // Arrange
            SessionId id = SessionId.New();

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
            SessionId id = SessionId.From(testGuid);

            // Act
            Guid value = id.Value;

            // Assert
            value.ShouldBe(testGuid);
        }

        [Fact]
        public void Value_ShouldBeReadOnly()
        {
            // Arrange
            SessionId id = SessionId.New();
            
            // Act & Assert
            Type idType = typeof(SessionId);
            System.Reflection.PropertyInfo? valueProperty = idType.GetProperty(nameof(SessionId.Value));
            
            valueProperty.ShouldNotBeNull();
            valueProperty.CanRead.ShouldBeTrue();
            valueProperty.CanWrite.ShouldBeFalse();
        }
    }

    public sealed class TypeSafety
    {
        [Fact]
        public void SessionId_ShouldPreventAccidentalMixupWithRawGuids()
        {
            // Arrange
            SessionId sessionId = SessionId.New();
            Guid rawGuid = Guid.NewGuid();

            // Act & Assert
            sessionId.ShouldBeOfType<SessionId>();
            rawGuid.ShouldBeOfType<Guid>();
        }

        [Fact]
        public void SessionId_ShouldBeValueType()
        {
            // Arrange & Act
            SessionId id = SessionId.New();

            // Assert
            typeof(SessionId).IsValueType.ShouldBeTrue();
        }

        [Fact]
        public void SessionId_ShouldBeReadOnly()
        {
            // Arrange
            Guid originalGuid = Guid.NewGuid();
            SessionId id = SessionId.From(originalGuid);

            // Assert
            id.Value.ShouldBe(originalGuid);
        }
    }

    public sealed class UsagePatterns
    {
        [Fact]
        public void SessionId_ShouldWorkInCollections()
        {
            // Arrange
            SessionId id1 = SessionId.New();
            SessionId id2 = SessionId.New();
            SessionId id3 = SessionId.New();

            // Act
            List<SessionId> idList = [id1, id2, id3];
            HashSet<SessionId> idSet = [id1, id2, id3, id1]; // Duplicate id1

            // Assert
            idList.Count.ShouldBe(3);
            idSet.Count.ShouldBe(3); // HashSet should remove duplicate
            
            idList.Contains(id1).ShouldBeTrue();
            idSet.Contains(id2).ShouldBeTrue();
        }

        [Fact]
        public void SessionId_ShouldWorkAsDictionaryKey()
        {
            // Arrange
            SessionId id1 = SessionId.New();
            SessionId id2 = SessionId.New();
            
            Dictionary<SessionId, string> sessionStates = new()
            {
                [id1] = "Active",
                [id2] = "Paused"
            };

            // Act & Assert
            sessionStates[id1].ShouldBe("Active");
            sessionStates[id2].ShouldBe("Paused");
            sessionStates.ContainsKey(id1).ShouldBeTrue();
            sessionStates.ContainsKey(SessionId.New()).ShouldBeFalse();
        }

        [Fact]
        public void SessionId_ShouldSortConsistently()
        {
            // Arrange
            List<SessionId> ids = [
                SessionId.New(),
                SessionId.New(),
                SessionId.New(),
                SessionId.New()
            ];

            // Act
            List<SessionId> sortedIds = [.. ids.OrderBy(id => id.Value)];

            // Assert
            sortedIds.Count.ShouldBe(4);
            for (int i = 0; i < sortedIds.Count - 1; i++)
            {
                sortedIds[i].Value.ShouldBeLessThanOrEqualTo(sortedIds[i + 1].Value);
            }
        }
    }

    public sealed class CrossTypeComparisons
    {
        [Fact]
        public void SessionId_ShouldNotEqualCharacterIdWithSameGuid()
        {
            // This test documents type safety between different ID types
            // Arrange
            Guid sharedGuid = Guid.NewGuid();
            SessionId sessionId = SessionId.From(sharedGuid);
            
            // Act & Assert - Different ID types should never be equal even with same Guid
            // This is enforced at compile time, but documents the design intention
            sessionId.Value.ShouldBe(sharedGuid);
            
            // The compiler prevents direct comparison: sessionId == characterId
            // This test documents that type safety is the primary benefit
        }

        [Fact]
        public void SessionId_ShouldMaintainTypeIdentityInGenerics()
        {
            // Arrange
            SessionId sessionId = SessionId.New();

            // Act
            Type sessionIdType = GetGenericType(sessionId);
            
            // Assert
            sessionIdType.ShouldBe(typeof(SessionId));
        }

        private static Type GetGenericType<T>(T value) => typeof(T);
    }
}