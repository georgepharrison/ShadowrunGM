# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Coding Standards

### Core Principles
- **SOLID Principles**: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **GoF Patterns**: Use when appropriate (Factory, Strategy, Observer) but avoid over-engineering
- **Domain-Driven Design**: Rich domain models, value objects, aggregates, domain events

### C# Language Standards
- **NO `var` keyword**: Always use explicit types for clarity
  ```csharp
  // ❌ Bad
  var character = new Character();
  var result = CalculateDicePool();
  
  // ✅ Good
  Character character = new();
  DicePool result = CalculateDicePool();
  ```

- **Use target-typed `new()` expressions**: When type is obvious from declaration
  ```csharp
  // ✅ Good
  Character character = new();
  List<Augmentation> augmentations = new();
  Dictionary<string, int> modifiers = new();
  ```

- **Use collection expressions `[]`**: For collection initialization
  ```csharp
  // ❌ Bad
  List<string> skills = new List<string>();
  IEnumerable<int> numbers = new int[] { 1, 2, 3 };
  
  // ✅ Good
  List<string> skills = [];
  IEnumerable<int> numbers = [1, 2, 3];
  ```

- **Expression-bodied members**: Use for single-line methods, properties, and constructors
  ```csharp
  // ✅ Good - Methods
  public string GetDisplayName() =>
      $"{FirstName} {LastName}";
  
  public int CalculateTotalCost() =>
      BaseCost + (Modifiers.Sum(m => m.Cost));
  
  // ✅ Good - Properties  
  public bool IsActive =>
      Status == CharacterStatus.Active && !IsArchived;
  
  public string FullDescription =>
      string.IsNullOrEmpty(Description) 
          ? Name 
          : $"{Name}: {Description}";
  
  // ✅ Good - Constructors (when simple)
  public CharacterId(Guid value) =>
      Value = value != Guid.Empty ? value : throw new ArgumentException("CharacterId cannot be empty");
  ```

- **Primary constructors**: Use when available and appropriate
  ```csharp
  // ✅ Good for simple services
  public sealed class CharacterService(ICharacterRepository repository, IValidator validator)
  {
      public async Task<Character> GetCharacterAsync(CharacterId id) =>
          await repository.GetByIdAsync(id);
  }
  ```

- **Private fields over constructor parameters**: For complex classes
  ```csharp
  // ✅ Good for aggregates and entities
  public sealed class Character
  {
      private readonly List<Augmentation> _augmentations = [];
      private readonly List<DomainEvent> _events = [];
      
      public Character(Metatype metatype)
      {
          Metatype = metatype;
      }
  }
  ```

- **Private field naming**: Use underscore prefix
  ```csharp
  private readonly ILogger _logger;
  private readonly List<Contact> _contacts = [];
  private Edge _currentEdge;
  ```

### Result Pattern - CRITICAL REQUIREMENT

**ALWAYS use the existing Result<T> pattern from `ShadowrunGM.ApiSdk.Common.Results` namespace.**

**DO NOT create new Result classes.** The codebase has a comprehensive, production-ready Result pattern implementation that MUST be used for all error handling.

#### Required Using Statements
```csharp
using ShadowrunGM.ApiSdk.Common.Results;
```

#### Success Cases
```csharp
// Non-generic success
public Result ProcessCommand() =>
    Result.Success();

public Result ProcessWithWarning() =>
    Result.Success(ResultType.Warning);

// Generic success  
public Result<Character> GetCharacter() =>
    Result.Success(character);

public Result<Character> GetCharacterWithInfo() =>
    Result.Success(character, ResultType.Information);
```

#### Failure Cases
```csharp
// Simple error
public Result<Character> ValidateCharacter() =>
    Result.Failure<Character>("Character validation failed");

// Validation errors (use the validation framework)
public Result<Character> CreateCharacter(CreateCharacterRequest request)
{
    ValidationBuilder<Character> builder = new();
    return builder
        .RuleFor(x => x.Name, request.Name)
            .NotEmpty()
            .MaximumLength(50)
        .RuleFor(x => x.Age, request.Age)
            .GreaterThan(0)
            .LessThan(120)
        .Build(() => new Character(request.Name, request.Age));
}

// Security errors
public Result<SecureData> GetSecureData() =>
    Result.Failure<SecureData>(new SecurityException("Insufficient permissions"));

// Multiple validation errors
public Result ProcessMultipleFields()
{
    Dictionary<string, string[]> errors = new()
    {
        ["Name"] = ["Name is required", "Name must be less than 50 characters"],
        ["Email"] = ["Invalid email format"]
    };
    return Result.Failure(errors);
}
```

#### Pattern Matching
```csharp
// Simple pattern matching
return characterResult.Match(
    onSuccess: character => ProcessCharacter(character),
    onFailure: error => HandleError(error));

// Full pattern matching
return characterResult.Match(
    onSuccess: character => ProcessSuccess(character),
    onError: error => HandleError(error),
    onSecurityException: error => HandleSecurity(error), 
    onValidationException: errors => HandleValidation(errors),
    onOperationCanceledException: error => HandleCancellation(error));
```

#### HTTP Response Integration
```csharp
// Automatic HTTP response conversion
HttpResponseMessage response = await httpClient.GetAsync("/api/characters/1");
Result<Character> result = await response.ToResultFromJsonAsync<Character>();

// With custom JSON options
Result<Character> result = await response.ToResultFromJsonAsync<Character>(jsonOptions);

// Non-generic result
Result result = await response.ToResultAsync();
```

#### Available Validation Rules
The framework includes extensive validation rules:

**String validation**: `NotEmpty()`, `MinimumLength()`, `MaximumLength()`, `ExactLength()`, `EmailAddress()`, `Matches(regex)`

**Numeric validation**: `GreaterThan()`, `LessThan()`, `GreaterThanOrEqualTo()`, `LessThanOrEqualTo()`, `InclusiveBetween()`, `ExclusiveBetween()`, `PrecisionScale()`

**Collection validation**: `MinCount()`, `MaxCount()`, `Count()`, `Unique()`

**General validation**: `NotNull()`, `Null()`, `Equal()`, `NotEqual()`, `Must()`, `Empty()`

**Conditional validation**: `When()`, `Unless()`

**Custom messages**: `WithMessage("Custom error message")`

### Documentation Standards
- **XML documentation on ALL public members**: Methods, properties, classes, interfaces
  ```csharp
  /// <summary>
  /// Calculates the total dice pool for a skill test.
  /// </summary>
  /// <param name="attribute">The primary attribute for the test.</param>
  /// <param name="skill">The skill rating being used.</param>
  /// <param name="modifiers">Additional situational modifiers.</param>
  /// <returns>The total number of dice to roll.</returns>
  public int CalculateDicePool(Attribute attribute, int skill, IEnumerable<Modifier> modifiers)
  {
      // Implementation
  }
  ```

### File Organization
- **One type per file**: Classes, interfaces, enums each get their own file
- **Folder structure matches namespaces**: `Domain/Character/Character.cs` → `ShadowrunGM.Domain.Character`
- **Bounded context organization**: Group by feature/context, not by technical layer
  ```
  src/
    Domain/
      Character/
        Character.cs
        CharacterId.cs
        Augmentation.cs
        Events/
          CharacterCreated.cs
          AugmentationInstalled.cs
      Mission/
        Mission.cs
        Scene.cs
        Events/
          MissionStarted.cs
  ```

### Domain Model Standards
- **Aggregate roots**: Protect invariants, coordinate changes
- **Value objects**: Immutable, self-validating, equality by value
- **Domain events**: Past tense, contain all relevant data
- **Factory methods**: For complex object creation
- **Private setters**: Encapsulate state changes through methods

### CQRS Standards
- **Commands**: Imperative mood, return Result<T>
- **Queries**: Question form, return read models
- **Handlers**: Single responsibility, thin orchestration
- **Vertical slices**: Feature-complete including validation

### Testing Standards
- **Arrange-Act-Assert**: Clear test structure
- **One assertion per test**: Focused test scenarios
- **Test naming**: `MethodName_StateUnderTest_ExpectedBehavior`
- **Builder pattern**: For complex test data setup

### Additional Guidelines (To Be Added Later)
- Performance optimization patterns
- Async/await best practices
- Exception handling strategies
- Logging and observability
- Security considerations

## Common Development Commands

### Platform Notes
- **Development Environment**: Windows 11
- **Use PowerShell commands**: Not bash/Unix commands
- **No chmod or Unix file permissions**: Windows handles permissions differently
- **Use dotnet CLI**: For all .NET operations (build, run, test, migrations)

### Build and Run
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/API/API.csproj
dotnet build src/UI/UI.csproj

# Run API (development server)
cd src/API && dotnet run

# Run UI via Docker (recommended for PWA testing)
docker build -t shadowrun-ui:dev -f src/UI/Dockerfile .
docker run --rm -it -p 8080:80 --name shadowrun-ui shadowrun-ui:dev

# Clean rebuild (if needed)
docker build --no-cache -t shadowrun-ui:dev -f src/UI/Dockerfile .
```

### Database Commands
```bash
# Add new migration (from solution root)
dotnet ef migrations add <MigrationName> --project src/API --output-dir Infrastructure/Migrations

# Update database
dotnet ef database update --project src/API

# Drop database (dev only)
dotnet ef database drop --project src/API
```

### Testing
No test projects exist yet - when implemented, use `dotnet test`.

## Architecture Overview

### Solution Structure
- **src/API/** - ASP.NET Core backend with CQRS architecture, Domain-Driven Design
- **src/UI/** - Blazor WebAssembly frontend using MudBlazor components  
- **src/ApiSdk/** - Shared SDK for API communication with Result<T> pattern
- **src/SourceGen/** - Source generators for CQRS pattern automation

### Key Architectural Patterns
- **CQRS with MediatR** - Commands/queries in `src/API/Application/`
- **Domain-Driven Design** - Domain entities in `src/API/Domain/`
- **Import Pipeline** - PDF parsing and structured data import in `src/API/Importing/`
- **Semantic Kernel Integration** - AI plugins for GM assistance in `src/API/SemanticKernel/`
- **Result Pattern** - Error handling via `Result<T>` throughout the codebase

### Import Pipeline Architecture
Located in `src/API/Importing/` with structure-aware document processing:
- **Parser** → **Chunker** → **Classifier** → **Embedder/Indexer** → **Structured Persister**
- Supports PDF → structured domain tables for Shadowrun rulebooks
- Uses Entity Framework migrations for import staging and final schema

### Data Layer
- **Entity Framework Core** with PostgreSQL
- Migrations in `src/API/Infrastructure/Migrations/`
- Domain entities configured in `src/API/Infrastructure/Configurations/`

### UI Architecture
- **Blazor WebAssembly** with Progressive Web App capabilities
- **MudBlazor** component library for responsive design
- Mobile-first, chat-centric interface for GM assistance
- Docker deployment with nginx for static hosting

## Development Notes

### Import System
The import system is module-internal to the API project for MVP simplicity. All import-related code lives under `src/API/Importing/` and uses the same Entity Framework context and migrations as the main application.

### AI Integration
Semantic Kernel plugins expose domain functionality consistently for both UI and AI assistants. Future local model integration planned via Ollama for offline play.

### PWA Features  
The UI includes service worker, manifests, and offline capabilities. Use Docker setup for proper PWA testing across devices.