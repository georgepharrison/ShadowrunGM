# Infrastructure Layer

The Infrastructure layer provides concrete implementations for data persistence, external services, and cross-cutting concerns. This layer implements interfaces defined in the Domain layer and supports the application's CQRS architecture.

## Architecture Overview

This layer follows the **Repository Pattern** with **Entity Framework Core** as the primary ORM, supporting PostgreSQL with pgvector extensions for AI-powered features.

## Database Setup

### Docker Compose PostgreSQL with pgvector

The project includes a complete Docker Compose setup for local development:

```yaml
# docker-compose.yml
services:
  postgres:
    image: pgvector/pgvector:pg16
    environment:
      POSTGRES_DB: shadowrundb
      POSTGRES_USER: shadowrun
      POSTGRES_PASSWORD: shadowrun123
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./scripts/init-db.sql:/docker-entrypoint-initdb.d/init-db.sql
```

#### Starting the Database

```bash
# Start PostgreSQL with pgvector
docker-compose up -d postgres

# Verify connection
docker exec -it shadowrun-postgres psql -U shadowrun -d shadowrundb -c "SELECT version();"
```

#### Connection Configuration

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=shadowrundb;Username=shadowrun;Password=shadowrun123;Port=5432"
  }
}
```

### Migrations

Entity Framework migrations are located in `Migrations/` and handle:

- **Initial schema**: Game items, magic abilities, sourcebooks, rule content
- **Character tables**: Full Character aggregate with value object mapping
- **pgvector support**: Vector embeddings for AI-powered search

```bash
# Apply migrations
dotnet ef database update --project src/API

# Create new migration
dotnet ef migrations add <MigrationName> --project src/API --output-dir Infrastructure/Migrations
```

## Components

### Entity Configuration (`Configurations/`)

Entity Framework configurations map domain entities to database tables:

- **Character/CharacterConfiguration.cs** - Character aggregate mapping with owned value objects
- **Import/** - Import pipeline entities (Sourcebook, RuleContent, GameItem, MagicAbility)

**Example Character Configuration:**
```csharp
public void Configure(EntityTypeBuilder<Character> builder)
{
    builder.ToTable("characters");
    builder.HasKey(x => x.Id);
    
    // Value object mappings
    builder.OwnsOne(x => x.Attributes);
    builder.OwnsOne(x => x.Edge); 
    builder.OwnsOne(x => x.Health);
}
```

### Repository Implementations (`Repositories/`)

Concrete implementations of domain repository interfaces:

#### CharacterRepository

Implements `ICharacterRepository` with comprehensive CRUD operations:

- **Async/await patterns** with proper cancellation token support
- **FlowRight Result<T> pattern** for consistent error handling
- **Entity Framework Core** with PostgreSQL optimizations
- **Comprehensive logging** with structured logging patterns

**Key Methods:**
- `GetByIdAsync(CharacterId id)` - Retrieve by unique identifier
- `GetByNameAsync(string name)` - Retrieve by character name
- `GetActiveCharactersAsync()` - Get all active characters
- `GetByUserIdAsync(string userId)` - User-scoped character queries
- `AddAsync(Character aggregate)` - Insert with duplicate detection
- `UpdateAsync(Character aggregate)` - Update with concurrency handling
- `DeleteAsync(CharacterId id)` - Soft or hard delete operations
- `ExistsAsync(CharacterId id)` - Existence checking

**Error Handling:**
- Validation errors return `Result.Failure<T>(message)`
- Database exceptions logged and return generic failure messages
- Concurrency conflicts specifically detected and handled

### Data Seeding (`Seeders/`)

#### GameItemSeeder

Provides baseline Shadowrun 6e equipment data:

**Seeded Categories:**
- **Weapons** - Light pistols, heavy pistols, submachine guns
- **Armor** - Light armor (jackets, lined coats)
- **Electronics** - Commlinks, cyberdecks
- **Gear** - Medical kits, climbing gear
- **Cyberware** - Basic datajacks, cybereyes
- **Vehicles** - Bikes and cars

**Features:**
- **Idempotent seeding** - Safe to run multiple times
- **Sourcebook relationships** - Links to Core Rulebook
- **JSON stats storage** - Flexible schema for game mechanics
- **Proper timestamps** - CreatedAt/UpdatedAt tracking

### Database Extensions (`Extensions/`)

#### DatabaseExtensions

Provides startup database initialization:

```csharp
// Program.cs integration
await app.EnsureDatabaseCreatedAndSeededAsync();
```

**Operations:**
- Apply pending migrations automatically
- Seed basic game items for immediate development use
- Environment-specific seeding (development vs production)

## Integration with Application Layer

### Dependency Injection

```csharp
// Program.cs
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
```

### CQRS Handler Integration

Repositories integrate seamlessly with FlowRight.Cqrs.Http endpoint handlers:

```csharp
public class GetCharacterByIdHandler : IRequestHandler<GetCharacterByIdQuery, Result<CharacterDto>>
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

## Testing Strategy

### Test Database Setup

Infrastructure tests use in-memory SQLite or test containers:

```csharp
public class CharacterRepositoryTests : IDisposable
{
    private readonly ShadowrunContext _context;
    
    public CharacterRepositoryTests()
    {
        DbContextOptions<ShadowrunContext> options = new DbContextOptionsBuilder<ShadowrunContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ShadowrunContext(options);
    }
}
```

### Repository Testing Patterns

- **Arrange-Act-Assert** structure
- **Builder patterns** for test data construction
- **FlowRight Result<T> assertions** using Shouldly
- **Async test patterns** with proper cancellation token testing

## Performance Considerations

### Database Optimization

- **Indexes** on frequently queried columns (Name, CreatedAt, ModifiedAt)
- **Connection pooling** via Entity Framework
- **Async queries** to prevent thread blocking
- **Bulk operations** for seeding and imports

### Memory Management

- **Scoped lifetime** for repositories prevents memory leaks
- **Proper disposal** of contexts and resources
- **Cancellation token propagation** for responsive shutdowns

## Future Enhancements

### Planned Features

- **Event Sourcing** for character history tracking
- **CQRS read models** for optimized query performance  
- **Distributed caching** with Redis for frequently accessed data
- **Audit trails** for character modifications
- **Soft delete** patterns for data retention

### AI Integration

- **Vector embeddings** for semantic search across game content
- **Document similarity** for rule lookups and content recommendations
- **Hybrid search** combining traditional and semantic search