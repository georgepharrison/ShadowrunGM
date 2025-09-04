# CharacterRepository Implementation Guide

This document details the implementation of the `CharacterRepository` class, following Domain-Driven Design principles with comprehensive Test-Driven Development practices.

## Architecture Overview

The `CharacterRepository` implements the `ICharacterRepository` interface defined in the Domain layer, providing concrete data persistence for Character aggregates using Entity Framework Core.

### Key Design Principles

- **Repository Pattern** - Encapsulates data access logic
- **FlowRight Result<T> Pattern** - Consistent error handling across all operations
- **Async/Await** - Non-blocking database operations
- **Logging** - Structured logging for observability
- **Validation** - Input validation with meaningful error messages

## Implementation Structure

### Dependencies and Constructor

```csharp
public sealed class CharacterRepository : ICharacterRepository
{
    private readonly ShadowrunContext _context;
    private readonly ILogger<CharacterRepository> _logger;

    public CharacterRepository(ShadowrunContext context, ILogger<CharacterRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
}
```

**Design Decisions:**
- **Constructor injection** for dependencies following DI container patterns
- **Null validation** with meaningful exceptions for required dependencies
- **Sealed class** to prevent inheritance and maintain clear boundaries

## Core Operations

### Query Operations

#### GetByIdAsync

```csharp
public async Task<Result<Character>> GetByIdAsync(CharacterId id, CancellationToken cancellationToken = default)
{
    try
    {
        if (id.Value == Guid.Empty)
            return Result.Failure<Character>("Character ID is invalid");

        Character? character = await _context.Characters
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (character == null)
            return Result.Failure<Character>("Character not found");

        return Result.Success(character);
    }
    catch (OperationCanceledException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving character with ID {CharacterId}", id);
        return Result.Failure<Character>("Error retrieving character from database");
    }
}
```

**Implementation Patterns:**
- **Guard clauses** - Early validation of input parameters
- **Explicit null checking** - Handle not found scenarios gracefully
- **Exception categorization** - Differentiate between cancellation and other exceptions
- **Structured logging** - Include relevant context in log messages
- **Generic error messages** - Prevent information leakage to clients

#### GetByNameAsync

```csharp
public async Task<Result<Character>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
{
    try
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Character>("Character name cannot be null or empty");

        Character? character = await _context.Characters
            .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower(), cancellationToken);

        if (character == null)
            return Result.Failure<Character>("Character not found");

        return Result.Success(character);
    }
    catch (OperationCanceledException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving character with name {CharacterName}", name);
        return Result.Failure<Character>("Error retrieving character from database");
    }
}
```

**Key Features:**
- **Case-insensitive search** using `ToLower()` comparison
- **String validation** with `IsNullOrWhiteSpace` check
- **Consistent error handling** pattern across all methods

### Command Operations

#### AddAsync

```csharp
public async Task<Result> AddAsync(Character aggregate, CancellationToken cancellationToken = default)
{
    try
    {
        if (aggregate == null)
            return Result.Failure("Character cannot be null");

        // Check for duplicate ID
        bool exists = await _context.Characters
            .AnyAsync(c => c.Id == aggregate.Id, cancellationToken);

        if (exists)
            return Result.Failure("A character with this ID already exists");

        _context.Characters.Add(aggregate);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
    catch (OperationCanceledException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error adding character with ID {CharacterId}", aggregate?.Id);
        return Result.Failure($"Error saving character to database: {ex.Message}");
    }
}
```

**Business Logic:**
- **Duplicate detection** - Prevents inserting characters with existing IDs
- **Null validation** - Ensures aggregate is not null before processing
- **Atomic operations** - Uses EF SaveChanges for transaction management
- **Error context** - Includes character ID in error logs when available

#### UpdateAsync

```csharp
public async Task<Result> UpdateAsync(Character aggregate, CancellationToken cancellationToken = default)
{
    try
    {
        if (aggregate == null)
            return Result.Failure("Character cannot be null");

        // Check if character exists
        bool exists = await _context.Characters
            .AnyAsync(c => c.Id == aggregate.Id, cancellationToken);

        if (!exists)
            return Result.Failure("Character not found");

        _context.Characters.Update(aggregate);
        
        int rowsAffected = await _context.SaveChangesAsync(cancellationToken);
        
        if (rowsAffected == 0)
            return Result.Failure("No changes were saved - possible concurrency conflict");

        return Result.Success();
    }
    catch (OperationCanceledException)
    {
        throw;
    }
    catch (DbUpdateConcurrencyException ex)
    {
        _logger.LogWarning(ex, "Concurrency conflict updating character with ID {CharacterId}", aggregate?.Id);
        return Result.Failure("Character was modified by another user - concurrency conflict");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error updating character with ID {CharacterId}", aggregate?.Id);
        return Result.Failure("Error updating character in database");
    }
}
```

**Advanced Features:**
- **Existence verification** - Ensures character exists before attempting update
- **Concurrency handling** - Specific handling for `DbUpdateConcurrencyException`
- **Affected rows check** - Validates that changes were actually saved
- **Specific exception handling** - Different log levels for different exception types

## Testing Strategy

### Test Structure

The repository uses comprehensive TDD with the following patterns:

#### Test Builders

```csharp
public class CharacterRepositoryBuilder
{
    private ShadowrunContext? _context;
    private ILogger<CharacterRepository>? _logger;

    public CharacterRepositoryBuilder WithInMemoryDatabase(string databaseName = "")
    {
        string dbName = string.IsNullOrEmpty(databaseName) ? Guid.NewGuid().ToString() : databaseName;
        
        DbContextOptions<ShadowrunContext> options = new DbContextOptionsBuilder<ShadowrunContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .EnableSensitiveDataLogging()
            .Options;

        _context = new ShadowrunContext(options);
        return this;
    }

    public CharacterRepositoryBuilder WithMockLogger()
    {
        _logger = Substitute.For<ILogger<CharacterRepository>>();
        return this;
    }

    public CharacterRepository Build()
    {
        _context ??= CreateInMemoryContext();
        _logger ??= Substitute.For<ILogger<CharacterRepository>>();

        return new CharacterRepository(_context, _logger);
    }
}
```

**Builder Pattern Benefits:**
- **Fluent API** for readable test setup
- **Default values** reduce boilerplate in simple tests
- **Flexibility** allows customization for specific test scenarios

#### Test Categories

**Success Path Tests:**
```csharp
[Fact]
public async Task GetByIdAsync_WithValidId_ReturnsCharacter()
{
    // Arrange
    Character character = CharacterBuilder.Create().Build();
    CharacterRepository repository = new CharacterRepositoryBuilder()
        .WithInMemoryDatabase()
        .Build();
    
    await repository.AddAsync(character);
    
    // Act
    Result<Character> result = await repository.GetByIdAsync(character.Id);
    
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Id.Should().Be(character.Id);
    result.Value.Name.Should().Be(character.Name);
}
```

**Failure Path Tests:**
```csharp
[Fact]
public async Task GetByIdAsync_WithInvalidId_ReturnsFailure()
{
    // Arrange
    CharacterId invalidId = new(Guid.Empty);
    CharacterRepository repository = new CharacterRepositoryBuilder()
        .WithInMemoryDatabase()
        .Build();
    
    // Act
    Result<Character> result = await repository.GetByIdAsync(invalidId);
    
    // Assert
    result.IsSuccess.Should().BeFalse();
    result.Error.Should().Be("Character ID is invalid");
}
```

**Exception Handling Tests:**
```csharp
[Fact]
public async Task UpdateAsync_WithConcurrentModification_ReturnsFailureResult()
{
    // Arrange
    Character character = CharacterBuilder.Create().Build();
    CharacterRepository repository1 = new CharacterRepositoryBuilder()
        .WithInMemoryDatabase("shared-db")
        .Build();
    
    CharacterRepository repository2 = new CharacterRepositoryBuilder()
        .WithInMemoryDatabase("shared-db") 
        .Build();
    
    await repository1.AddAsync(character);
    
    // Act - Simulate concurrent modification
    Result<Character> getResult1 = await repository1.GetByIdAsync(character.Id);
    Result<Character> getResult2 = await repository2.GetByIdAsync(character.Id);
    
    // Both modify and save
    Character char1 = getResult1.Value;
    Character char2 = getResult2.Value;
    
    await repository1.UpdateAsync(char1);
    Result updateResult2 = await repository2.UpdateAsync(char2);
    
    // Assert
    updateResult2.IsSuccess.Should().BeFalse();
    updateResult2.Error.Should().Contain("concurrency");
}
```

### Test Assertions with Shouldly

The tests use Shouldly for fluent assertions:

```csharp
// Result<T> specific assertions
result.IsSuccess.Should().BeTrue();
result.IsFailure.Should().BeFalse();
result.Value.Should().NotBeNull();
result.Error.Should().Be("Expected error message");

// Domain entity assertions
character.Id.Should().Be(expectedId);
character.Name.Should().Be(expectedName);
character.Attributes.Should().NotBeNull();
character.Edge.Current.Should().BeGreaterThan(0);
```

## Error Handling Patterns

### Result<T> Integration

All repository methods return `Result<T>` objects:

```csharp
// Success cases
return Result.Success(character);
return Result.Success();

// Failure cases
return Result.Failure<Character>("Character not found");
return Result.Failure("Operation failed");

// Exception handling
catch (Exception ex)
{
    _logger.LogError(ex, "Context message with {Parameter}", paramValue);
    return Result.Failure<T>("Generic error message");
}
```

### Exception Categories

1. **OperationCanceledException** - Re-thrown to maintain cancellation semantics
2. **DbUpdateConcurrencyException** - Specific handling with appropriate user message
3. **General Exceptions** - Logged with context, generic error returned

### Logging Strategy

```csharp
// Error logging with context
_logger.LogError(ex, "Error retrieving character with ID {CharacterId}", id);

// Warning for business logic issues
_logger.LogWarning(ex, "Concurrency conflict updating character with ID {CharacterId}", aggregate?.Id);

// Structured logging with parameters
_logger.LogInformation("Character {CharacterName} created with ID {CharacterId}", 
    character.Name, character.Id);
```

## Performance Considerations

### Database Queries

- **Async operations** prevent thread blocking
- **Cancellation tokens** enable responsive cancellation
- **FirstOrDefaultAsync** for single entity queries
- **AnyAsync** for existence checks (more efficient than Count > 0)

### Memory Management

- **Scoped repository lifetime** prevents memory leaks
- **Proper disposal** handled by DI container
- **Minimal object allocation** in hot paths

### Connection Management

- **Connection pooling** via Entity Framework
- **Proper async patterns** prevent connection exhaustion
- **Transaction scope** managed by SaveChangesAsync

## Integration with CQRS

### Command Handlers

```csharp
public class CreateCharacterCommandHandler : IRequestHandler<CreateCharacterCommand, Result<Guid>>
{
    private readonly ICharacterRepository _repository;

    public async Task<Result<Guid>> Handle(CreateCharacterCommand request, CancellationToken cancellationToken)
    {
        Result<Character> characterResult = Character.Create(
            request.Name, 
            request.Attributes, 
            request.StartingEdge);

        if (!characterResult.IsSuccess)
            return Result.Failure<Guid>(characterResult.Error);

        Result addResult = await _repository.AddAsync(characterResult.Value, cancellationToken);
        
        return addResult.Match(
            onSuccess: () => Result.Success(characterResult.Value.Id.Value),
            onFailure: error => Result.Failure<Guid>(error)
        );
    }
}
```

### Query Handlers

```csharp
public class GetCharacterByIdQueryHandler : IRequestHandler<GetCharacterByIdQuery, Result<CharacterDto>>
{
    private readonly ICharacterRepository _repository;

    public async Task<Result<CharacterDto>> Handle(GetCharacterByIdQuery request, CancellationToken cancellationToken)
    {
        Result<Character> result = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        return result.Match(
            onSuccess: character => Result.Success(CharacterDto.FromDomain(character)),
            onFailure: error => Result.Failure<CharacterDto>(error)
        );
    }
}
```

## Future Enhancements

### Planned Improvements

- **Read/Write separation** for CQRS optimization
- **Bulk operations** for importing large datasets
- **Soft delete** support for audit trails
- **Event sourcing** integration for character history
- **Caching layer** for frequently accessed characters

### Performance Optimizations

- **Database indexing** strategy for common queries
- **Connection pooling** tuning
- **Query optimization** using EF Core query analysis
- **Pagination** for large result sets