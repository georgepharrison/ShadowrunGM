using ShadowrunGM.API.Infrastructure.Repositories;

namespace ShadowrunGM.Infrastructure.Tests.Repositories;

/// <summary>
/// Comprehensive failing tests for CharacterRepository following strict TDD principles.
/// These tests define the expected behavior for Entity Framework Core integration.
/// ALL TESTS WILL INITIALLY FAIL - implementation comes after Red phase.
/// </summary>
public sealed class CharacterRepositoryTests
{
    #region Base Repository Interface Tests

    public sealed class GetByIdAsync
    {
        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnCharacter()
        {
            // Arrange
            (CharacterRepository repository, ShadowrunContext context) = new CharacterRepositoryBuilder()
                .WithInMemoryDatabase()
                .WithMockLogger()
                .BuildWithContext();
            
            Character expectedCharacter = new CharacterBuilder()
                .WithName("Test Runner")
                .Build();
            
            // Pre-seed the character in database
            context.Characters.Add(expectedCharacter);
            await context.SaveChangesAsync();

            // Act
            Result<Character> result = await repository.GetByIdAsync(expectedCharacter.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Character? character).ShouldBeTrue();
            character.ShouldNotBeNull();
            character.Id.ShouldBe(expectedCharacter.Id);
            character.Name.ShouldBe("Test Runner");
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ShouldReturnFailure()
        {
            // Arrange
            CharacterRepository repository = new CharacterRepositoryBuilder()
                .WithInMemoryDatabase()
                .WithMockLogger()
                .Build();
            
            CharacterId nonExistentId = CharacterId.New();

            // Act
            Result<Character> result = await repository.GetByIdAsync(nonExistentId);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("not found");
        }

        [Fact]
        public async Task GetByIdAsync_WithEmptyId_ShouldReturnFailure()
        {
            // Arrange
            CharacterRepository repository = new CharacterRepositoryBuilder()
                .WithInMemoryDatabase()
                .WithMockLogger()
                .Build();
            
            CharacterId emptyId = CharacterId.From(Guid.Empty);

            // Act
            Result<Character> result = await repository.GetByIdAsync(emptyId);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("invalid");
        }

        [Fact]
        public async Task GetByIdAsync_WhenDatabaseUnavailable_ShouldReturnFailure()
        {
            // Arrange
            CharacterRepository repository = new CharacterRepositoryBuilder()
                .WithFailingDatabase()
                .WithMockLogger()
                .Build();
            
            CharacterId characterId = CharacterId.New();

            // Act
            Result<Character> result = await repository.GetByIdAsync(characterId);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("database");
        }

        [Fact]
        public async Task GetByIdAsync_WithCancellationToken_ShouldRespectCancellation()
        {
            // Arrange
            CharacterRepository repository = new CharacterRepositoryBuilder()
                .WithInMemoryDatabase()
                .WithMockLogger()
                .Build();
            
            CharacterId characterId = CharacterId.New();
            CancellationTokenSource cts = new();
            cts.Cancel();

            // Act & Assert
            await Should.ThrowAsync<OperationCanceledException>(async () =>
            {
                await repository.GetByIdAsync(characterId, cts.Token);
            });
        }
    }

    public sealed class AddAsync
    {
        [Fact]
        public async Task AddAsync_WithValidCharacter_ShouldAddSuccessfully()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder()
                .WithName("New Character")
                .Build();

            // Act
            Result result = await repository.AddAsync(character);

            // Assert
            result.IsSuccess.ShouldBeTrue($"Error: {result.Error}");
            
            // Verify character was actually added to database
            Character? addedCharacter = await context.Characters.FindAsync(character.Id);
            addedCharacter.ShouldNotBeNull();
            addedCharacter.Name.ShouldBe("New Character");
        }

        [Fact]
        public async Task AddAsync_WithNullCharacter_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            Character nullCharacter = null!;

            // Act
            Result result = await repository.AddAsync(nullCharacter);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("null");
        }

        [Fact]
        public async Task AddAsync_WithDuplicateId_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character existingCharacter = new CharacterBuilder()
                .WithName("Existing Character")
                .Build();
            
            await repository.AddAsync(existingCharacter);
            
            Character duplicateCharacter = new CharacterBuilder()
                .WithName("Duplicate Character")
                .Build();
            
            // TODO: In actual implementation, this test would verify EF Core constraint violations
            // For now, we simulate the scenario - duplicate IDs should be rejected

            // Act
            Result result = await repository.AddAsync(duplicateCharacter);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("already exists");
        }

        [Fact]
        public async Task AddAsync_WhenDatabaseUnavailable_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateFailingContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder().Build();

            // Act
            Result result = await repository.AddAsync(character);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("database");
        }
    }

    public sealed class UpdateAsync
    {
        [Fact]
        public async Task UpdateAsync_WithValidCharacter_ShouldUpdateSuccessfully()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder()
                .WithName("Original Name")
                .Build();
            
            await repository.AddAsync(character);
            
            // Modify character
            character.AddSkill("Firearms", 4);

            // Act
            Result result = await repository.UpdateAsync(character);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            
            // Verify character was actually updated in database
            Character? updatedCharacter = await context.Characters.FindAsync(character.Id);
            updatedCharacter.ShouldNotBeNull();
            updatedCharacter.Skills.Count.ShouldBe(1);
            updatedCharacter.Skills.First().Name.ShouldBe("Firearms");
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentCharacter_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder().Build();

            // Act
            Result result = await repository.UpdateAsync(character);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("not found");
        }

        [Fact]
        public async Task UpdateAsync_WithNullCharacter_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            Character nullCharacter = null!;

            // Act
            Result result = await repository.UpdateAsync(nullCharacter);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("null");
        }

        [Fact]
        public async Task UpdateAsync_WithConcurrencyConflict_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder().Build();
            await repository.AddAsync(character);
            
            // TODO: Simulate concurrent modification for testing optimistic concurrency
            // In actual implementation, this would modify the character's timestamp/version
            // causing the subsequent update to fail with concurrency exception

            // Act
            Result result = await repository.UpdateAsync(character);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("concurrency");
        }
    }

    public sealed class DeleteAsync
    {
        [Fact]
        public async Task DeleteAsync_WithValidId_ShouldDeleteSuccessfully()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder().Build();
            await repository.AddAsync(character);

            // Act
            Result result = await repository.DeleteAsync(character.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            
            // Verify character was actually deleted from database
            Character? deletedCharacter = await context.Characters.FindAsync(character.Id);
            deletedCharacter.ShouldBeNull();
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            CharacterId nonExistentId = CharacterId.New();

            // Act
            Result result = await repository.DeleteAsync(nonExistentId);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("not found");
        }

        [Fact]
        public async Task DeleteAsync_WithEmptyId_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            CharacterId emptyId = CharacterId.From(Guid.Empty);

            // Act
            Result result = await repository.DeleteAsync(emptyId);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("invalid");
        }
    }

    public sealed class ExistsAsync
    {
        [Fact]
        public async Task ExistsAsync_WithExistingId_ShouldReturnTrue()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder().Build();
            await repository.AddAsync(character);

            // Act
            Result<bool> result = await repository.ExistsAsync(character.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out bool exists).ShouldBeTrue();
            exists.ShouldBeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistentId_ShouldReturnFalse()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            CharacterId nonExistentId = CharacterId.New();

            // Act
            Result<bool> result = await repository.ExistsAsync(nonExistentId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out bool exists).ShouldBeTrue();
            exists.ShouldBeFalse();
        }

        [Fact]
        public async Task ExistsAsync_WithEmptyId_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            CharacterId emptyId = CharacterId.From(Guid.Empty);

            // Act
            Result<bool> result = await repository.ExistsAsync(emptyId);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("invalid");
        }
    }

    #endregion Base Repository Interface Tests

    #region Character-Specific Repository Interface Tests

    public sealed class GetByUserIdAsync
    {
        [Fact]
        public async Task GetByUserIdAsync_WithValidUserId_ShouldReturnUserCharacters()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            string userId = "user123";
            
            Character character1 = new CharacterBuilder()
                .WithName("Character 1")
                .Build();
            Character character2 = new CharacterBuilder()
                .WithName("Character 2")
                .Build();
            Character character3 = new CharacterBuilder()
                .WithName("Other User Character")
                .Build();
            
            // TODO: In actual implementation, these characters would have UserId foreign key
            // For now, we simulate by adding to context - the repository will need to filter by UserId
            context.Characters.Add(character1);
            context.Characters.Add(character2);
            context.Characters.Add(character3);
            await context.SaveChangesAsync();

            // Act
            Result<IReadOnlyList<Character>> result = await repository.GetByUserIdAsync(userId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out IReadOnlyList<Character>? characters).ShouldBeTrue();
            characters.ShouldNotBeNull();
            characters.Count.ShouldBe(2);
            characters.ShouldContain(c => c.Name == "Character 1");
            characters.ShouldContain(c => c.Name == "Character 2");
            characters.ShouldNotContain(c => c.Name == "Other User Character");
        }

        [Fact]
        public async Task GetByUserIdAsync_WithNonExistentUserId_ShouldReturnEmptyList()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            string nonExistentUserId = "nonexistent-user";

            // Act
            Result<IReadOnlyList<Character>> result = await repository.GetByUserIdAsync(nonExistentUserId);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out IReadOnlyList<Character>? characters).ShouldBeTrue();
            characters.ShouldNotBeNull();
            characters.Count.ShouldBe(0);
        }

        [Fact]
        public async Task GetByUserIdAsync_WithNullUserId_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            string nullUserId = null!;

            // Act
            Result<IReadOnlyList<Character>> result = await repository.GetByUserIdAsync(nullUserId);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("userId");
        }

        [Fact]
        public async Task GetByUserIdAsync_WithEmptyUserId_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            string emptyUserId = string.Empty;

            // Act
            Result<IReadOnlyList<Character>> result = await repository.GetByUserIdAsync(emptyUserId);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("userId");
        }
    }

    public sealed class GetByNameAsync
    {
        [Fact]
        public async Task GetByNameAsync_WithExistingName_ShouldReturnCharacter()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder()
                .WithName("Shadow Runner")
                .Build();
            
            await repository.AddAsync(character);

            // Act
            Result<Character> result = await repository.GetByNameAsync("Shadow Runner");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Character? foundCharacter).ShouldBeTrue();
            foundCharacter.ShouldNotBeNull();
            foundCharacter.Name.ShouldBe("Shadow Runner");
            foundCharacter.Id.ShouldBe(character.Id);
        }

        [Fact]
        public async Task GetByNameAsync_WithNonExistentName_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);

            // Act
            Result<Character> result = await repository.GetByNameAsync("Non-Existent Character");

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("not found");
        }

        [Fact]
        public async Task GetByNameAsync_WithCaseInsensitiveSearch_ShouldReturnCharacter()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder()
                .WithName("Shadow Runner")
                .Build();
            
            await repository.AddAsync(character);

            // Act
            Result<Character> result = await repository.GetByNameAsync("SHADOW RUNNER");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Character? foundCharacter).ShouldBeTrue();
            foundCharacter.ShouldNotBeNull();
            foundCharacter.Name.ShouldBe("Shadow Runner");
        }

        [Fact]
        public async Task GetByNameAsync_WithNullName_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            string nullName = null!;

            // Act
            Result<Character> result = await repository.GetByNameAsync(nullName);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("name");
        }

        [Fact]
        public async Task GetByNameAsync_WithEmptyName_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);

            // Act
            Result<Character> result = await repository.GetByNameAsync(string.Empty);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("name");
        }
    }

    public sealed class GetActiveCharactersAsync
    {
        [Fact]
        public async Task GetActiveCharactersAsync_WithActiveCharacters_ShouldReturnOnlyActive()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character activeCharacter1 = new CharacterBuilder()
                .WithName("Active Character 1")
                .Build();
            Character activeCharacter2 = new CharacterBuilder()
                .WithName("Active Character 2")
                .Build();
            Character inactiveCharacter = new CharacterBuilder()
                .WithName("Inactive Character")
                .Build();
            
            // TODO: In actual implementation, characters would have IsActive/Status property
            // For now, we simulate - the repository will need to filter by active status
            context.Characters.Add(activeCharacter1);
            context.Characters.Add(activeCharacter2);
            context.Characters.Add(inactiveCharacter);
            await context.SaveChangesAsync();

            // Act
            Result<IReadOnlyList<Character>> result = await repository.GetActiveCharactersAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out IReadOnlyList<Character>? characters).ShouldBeTrue();
            characters.ShouldNotBeNull();
            characters.Count.ShouldBe(2);
            characters.ShouldContain(c => c.Name == "Active Character 1");
            characters.ShouldContain(c => c.Name == "Active Character 2");
            characters.ShouldNotContain(c => c.Name == "Inactive Character");
        }

        [Fact]
        public async Task GetActiveCharactersAsync_WithNoActiveCharacters_ShouldReturnEmptyList()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character inactiveCharacter = new CharacterBuilder()
                .WithName("Inactive Character")
                .Build();
            
            // TODO: In actual implementation, character would have IsActive/Status property
            context.Characters.Add(inactiveCharacter);
            await context.SaveChangesAsync();

            // Act
            Result<IReadOnlyList<Character>> result = await repository.GetActiveCharactersAsync();

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out IReadOnlyList<Character>? characters).ShouldBeTrue();
            characters.ShouldNotBeNull();
            characters.Count.ShouldBe(0);
        }

        [Fact]
        public async Task GetActiveCharactersAsync_WithDatabaseError_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateFailingContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);

            // Act
            Result<IReadOnlyList<Character>> result = await repository.GetActiveCharactersAsync();

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("database");
        }
    }

    public sealed class ExistsByNameAsync
    {
        [Fact]
        public async Task ExistsByNameAsync_WithExistingName_ShouldReturnTrue()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder()
                .WithName("Unique Character")
                .Build();
            
            await repository.AddAsync(character);

            // Act
            Result<bool> result = await repository.ExistsByNameAsync("Unique Character");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out bool exists).ShouldBeTrue();
            exists.ShouldBeTrue();
        }

        [Fact]
        public async Task ExistsByNameAsync_WithNonExistentName_ShouldReturnFalse()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);

            // Act
            Result<bool> result = await repository.ExistsByNameAsync("Non-Existent Character");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out bool exists).ShouldBeTrue();
            exists.ShouldBeFalse();
        }

        [Fact]
        public async Task ExistsByNameAsync_WithCaseInsensitiveSearch_ShouldReturnTrue()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder()
                .WithName("Case Sensitive Name")
                .Build();
            
            await repository.AddAsync(character);

            // Act
            Result<bool> result = await repository.ExistsByNameAsync("CASE SENSITIVE NAME");

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out bool exists).ShouldBeTrue();
            exists.ShouldBeTrue();
        }

        [Fact]
        public async Task ExistsByNameAsync_WithNullName_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            string nullName = null!;

            // Act
            Result<bool> result = await repository.ExistsByNameAsync(nullName);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("name");
        }

        [Fact]
        public async Task ExistsByNameAsync_WithEmptyName_ShouldReturnFailure()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);

            // Act
            Result<bool> result = await repository.ExistsByNameAsync(string.Empty);

            // Assert
            result.IsFailure.ShouldBeTrue();
            result.Error.ShouldContain("name");
        }
    }

    #endregion Character-Specific Repository Interface Tests

    #region Entity Framework Integration Tests

    public sealed class EntityFrameworkIntegration
    {
        [Fact]
        public async Task Repository_WithComplexCharacterData_ShouldPersistAllProperties()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder()
                .WithName("Complex Character")
                .WithAttributes(builder => builder
                    .WithBody(6)
                    .WithAgility(5)
                    .WithReaction(4)
                    .WithStrength(3)
                    .WithWillpower(4)
                    .WithLogic(5)
                    .WithIntuition(4)
                    .WithCharisma(3))
                .WithStartingEdge(4)
                .Build();
            
            // Add skills and modify health
            character.AddSkill("Firearms", 6, "Pistols");
            character.AddSkill("Hacking", 5, "Cybercombat");
            character.TakePhysicalDamage(2);
            character.TakeStunDamage(1);
            character.SpendEdge(1, "Push the Limit");

            // Act
            await repository.AddAsync(character);
            
            // Retrieve and verify
            Result<Character> result = await repository.GetByIdAsync(character.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Character? retrievedCharacter).ShouldBeTrue();
            retrievedCharacter.ShouldNotBeNull();
            
            // Verify all properties persisted correctly
            retrievedCharacter.Name.ShouldBe("Complex Character");
            retrievedCharacter.Attributes.Body.ShouldBe(6);
            retrievedCharacter.Attributes.Agility.ShouldBe(5);
            retrievedCharacter.Skills.Count.ShouldBe(2);
            retrievedCharacter.Skills.ShouldContain(s => s.Name == "Firearms" && s.Specialization == "Pistols");
            retrievedCharacter.Health.PhysicalDamage.ShouldBe(2);
            retrievedCharacter.Health.StunDamage.ShouldBe(1);
            retrievedCharacter.Edge.Current.ShouldBe(3);
        }

        [Fact]
        public async Task Repository_WithValueObjectMapping_ShouldHandleComplexTypes()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder().Build();

            // Act
            await repository.AddAsync(character);
            Result<Character> result = await repository.GetByIdAsync(character.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Character? retrievedCharacter).ShouldBeTrue();
            retrievedCharacter.ShouldNotBeNull();
            
            // Verify value objects are properly mapped
            retrievedCharacter.Id.ShouldBe(character.Id);
            retrievedCharacter.Attributes.ShouldNotBeNull();
            retrievedCharacter.Edge.ShouldNotBeNull();
            retrievedCharacter.Health.ShouldNotBeNull();
        }

        [Fact]
        public async Task Repository_WithDomainEvents_ShouldNotPersistEvents()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            Character character = new CharacterBuilder().Build();
            character.SpendEdge(1, "Test Event");

            // Act
            await repository.AddAsync(character);
            Result<Character> result = await repository.GetByIdAsync(character.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Character? retrievedCharacter).ShouldBeTrue();
            retrievedCharacter.ShouldNotBeNull();
            
            // Domain events should not be persisted to database
            // They should be cleared after persistence
            retrievedCharacter.DomainEvents.Count.ShouldBe(0);
        }

        [Fact]
        public async Task Repository_WithTimestamps_ShouldHandleTemporalData()
        {
            // Arrange
            ShadowrunContext context = CreateInMemoryContext();
            ILogger<CharacterRepository> logger = Substitute.For<ILogger<CharacterRepository>>();
            CharacterRepository repository = new(context, logger);
            
            DateTime beforeCreation = DateTime.UtcNow;
            Character character = new CharacterBuilder().Build();
            DateTime afterCreation = DateTime.UtcNow;

            // Act
            await repository.AddAsync(character);
            Result<Character> result = await repository.GetByIdAsync(character.Id);

            // Assert
            result.IsSuccess.ShouldBeTrue();
            result.TryGetValue(out Character? retrievedCharacter).ShouldBeTrue();
            retrievedCharacter.ShouldNotBeNull();
            
            // Verify timestamps are preserved
            retrievedCharacter.CreatedAt.ShouldBeGreaterThanOrEqualTo(beforeCreation);
            retrievedCharacter.CreatedAt.ShouldBeLessThanOrEqualTo(afterCreation);
            retrievedCharacter.ModifiedAt.ShouldBe(retrievedCharacter.CreatedAt);
        }
    }

    #endregion Entity Framework Integration Tests

    #region Test Implementation Notes

    /*
     * IMPLEMENTATION NOTES FOR TDD:
     * 
     * These tests define the expected behavior for CharacterRepository.
     * ALL TESTS WILL FAIL until the implementation is created.
     * 
     * Key Implementation Requirements:
     * 1. CharacterRepository class in ShadowrunGM.API.Infrastructure.Repositories namespace
     * 2. Implements ICharacterRepository interface
     * 3. Uses ShadowrunContext for Entity Framework operations
     * 4. Returns Result<T> for all operations following established patterns
     * 5. Handles all error scenarios gracefully
     * 6. Uses ILogger<CharacterRepository> for logging
     * 7. Character entity needs EF Core configuration (similar to existing Import configurations)
     * 8. ShadowrunContext needs DbSet<Character> Characters property
     * 
     * Missing Implementation Components:
     * - CharacterRepository class
     * - Character entity EF configuration
     * - CharacterConfiguration : IEntityTypeConfiguration<Character>
     * - ShadowrunContext.Characters DbSet property
     * - Database migration for Characters table
     * 
     * Test Categories Covered:
     * - Base repository operations (CRUD)
     * - Character-specific queries
     * - Error handling and edge cases
     * - Entity Framework integration
     * - Concurrency handling
     * - Result<T> pattern usage
     */

    #endregion Test Implementation Notes

    #region Helper Methods

    /// <summary>
    /// Creates an in-memory Entity Framework context for testing.
    /// </summary>
    private static ShadowrunContext CreateInMemoryContext()
    {
        string databaseName = Guid.NewGuid().ToString();
        
        DbContextOptions<ShadowrunContext> options = new DbContextOptionsBuilder<ShadowrunContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new ShadowrunContext(options);
    }

    /// <summary>
    /// Creates a context that will fail on operations for testing error scenarios.
    /// </summary>
    private static ShadowrunContext CreateFailingContext()
    {
        // This will fail because it tries to connect to a non-existent database
        DbContextOptions<ShadowrunContext> options = new DbContextOptionsBuilder<ShadowrunContext>()
            .UseNpgsql("Host=nonexistent;Database=nonexistent;Username=nonexistent;Password=nonexistent")
            .Options;

        return new ShadowrunContext(options);
    }

    #endregion Helper Methods
}