# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Standard Development Workflow

**All tasks must follow this workflow automatically:**

1. **Analyze First**: Use `@codebase-analyzer` to understand existing patterns before implementing new features
2. **Test-Driven Development**: Use `tdd-implementation-specialist` and `tdd-test-writer` agents for tests-first approach
3. **Mark Tasks Complete**: Update `docs/TASKS.md` to mark completed tasks with `[x]`
4. **Auto-Documentation**: Run `@documentation-agent` after task completion to update docs and commit changes

**Example implementation flow:**
```bash
# Step 1: Understand existing patterns (automatic)
@codebase-analyzer I need to implement [feature]. Please analyze existing patterns for [relevant area].

# Step 2: TDD Implementation (agents coordinate automatically)
# Tests written first, then implementation follows

# Step 3: Mark tasks complete in TASKS.md (CRITICAL STEP)
# Update [ ] to [x] for completed tasks in docs/TASKS.md

# Step 4: Documentation and commit (automatic)
@documentation-agent Please analyze the changes I just made and update any necessary documentation, then commit all changes with conventional commits following the project's existing commit patterns.
```

**CRITICAL: Always update docs/TASKS.md task checkboxes when completing tasks. This is required for proper project tracking.**

**This workflow applies to all tasks unless explicitly overridden.**

## Coding Standards

### Core Principles
- **SOLID Principles**: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion
- **GoF Patterns**: Use when appropriate (Factory, Strategy, Observer) but avoid over-engineering
- **Domain-Driven Design**: Rich domain models, value objects, aggregates, domain events

### C# Language Standards

**NOTE: Core language standards are enforced by `.editorconfig` as build errors. See that file for complete formatting and style rules.**

**Key architectural patterns to follow:**
- **Sealed classes**: Mark as sealed when not designed for inheritance
- **Record types**: Use for DTOs, events, and simple data structures
- **Primary constructors**: Use for simple services and records
- **Private fields over constructor parameters**: For complex aggregates/entities
- **Nullable reference types**: Use nullable annotations consistently
- **Strategic accessibility modifiers**: Use internal/private constructors appropriately

### Result Pattern - CRITICAL REQUIREMENT

**ALWAYS use the FlowRight libraries for Result<T> pattern and validation.**

**DO NOT create new Result classes.** The codebase uses FlowRight libraries which provide a comprehensive, production-ready Result pattern implementation and validation framework that MUST be used for all error handling.

#### Required Using Statements
```csharp
using FlowRight.Core.Results;
using FlowRight.Validation.Builders; // For ValidationBuilder
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
public static Result<Character> CreateCharacter(string name, int age) =>
    new ValidationBuilder<Character>()
        .RuleFor(x => x.Name, name)
            .NotEmpty()
            .MaxLength(50)
            .WithMessage("Character name must be between 1 and 50 characters")
        .RuleFor(x => x.Age, age)
            .GreaterThan(0)
            .LessThan(120)
            .WithMessage("Character age must be between 1 and 119")
        .Build(() => new Character(name, age));

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

#### Result<T> Composition with ValidationBuilder
```csharp
// Use Result<T> composition for nested validation
public static Result<Character> Create(string name, AttributeSet attributes, int startingEdge) =>
    new ValidationBuilder<Character>()
        .RuleFor(x => x.Name, name)
            .NotEmpty()
            .MaxLength(100)
            .WithMessage("Character name must be between 1 and 100 characters")
        .RuleFor(x => x.Attributes, AttributeSet.Create(attributes), out AttributeSet? validatedAttributes)
        .RuleFor(x => x.Edge, Edge.Create(startingEdge), out Edge? validatedEdge)
        .Build(() => new Character(name, validatedAttributes!, validatedEdge!));

// Real example from the codebase - AttributeSet validation
public static Result<AttributeSet> Create(int body, int agility, int reaction, int strength, 
    int willpower, int logic, int intuition, int charisma) =>
    new ValidationBuilder<AttributeSet>()
        .RuleFor(x => x.Body, body)
            .InclusiveBetween(1, 10)
            .WithMessage("Body attribute must be between 1 and 10")
        .RuleFor(x => x.Agility, agility) 
            .InclusiveBetween(1, 10)
            .WithMessage("Agility attribute must be between 1 and 10")
        // ... other attributes
        .Build(() => new AttributeSet(body, agility, reaction, strength, willpower, logic, intuition, charisma));
```

#### FlowRight ValidationBuilder Features
- **Result<T> Integration**: `RuleFor(x => x.Property, someResult, out value)` automatically extracts failures
- **Fluent Validation**: Chain validation rules with method chaining
- **Custom Messages**: Use `WithMessage()` for user-friendly error messages
- **Out Parameter Pattern**: Access successful values from Result<T> for object construction
- **Nested Validation**: Compose validation across value object creation methods
- **Type-Safe Building**: Build() method only executes factory when all validations pass

#### ValidationBuilder Best Practices
- **Parse, Don't Validate Pattern**: Always normalize/sanitize inputs BEFORE validation:
  ```csharp
  public static Result<Skill> Create(string name, int rating)
  {
      // Normalize inputs first
      string trimmedName = name?.Trim() ?? string.Empty;
      
      // Then validate normalized values
      return new ValidationBuilder<Skill>()
          .RuleFor(x => x.Name, trimmedName)
              .NotEmpty()
              .WithMessage("Skill name is required")
          .Build(() => new Skill(trimmedName, rating));
  }
  ```
- **Consistent Error Messages**: Include the entity name in validation messages (e.g., "Character name is required" not just "Name is required")
- **Whitespace Handling**: Trim string inputs before validation to properly reject whitespace-only values

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

#### FlowRight Validation Rules
The FlowRight validation framework includes extensive validation rules:

**String validation**: `NotEmpty()`, `MinLength()`, `MaxLength()`, `ExactLength()`, `EmailAddress()`, `Matches(regex)`

**Numeric validation**: `GreaterThan()`, `LessThan()`, `GreaterThanOrEqualTo()`, `LessThanOrEqualTo()`, `InclusiveBetween()`, `ExclusiveBetween()`

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