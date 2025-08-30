# Data Seeding Strategy

This document outlines the data seeding approach for the ShadowrunGM project, focusing on the `GameItemSeeder` implementation and the broader seeding architecture.

## Overview

The seeding system provides baseline Shadowrun 6th Edition content for immediate development and testing use. It follows Domain-Driven Design principles while ensuring data consistency and idempotent operations.

## Architecture

### Core Components

1. **GameItemSeeder** - Static class providing Shadowrun equipment data
2. **DatabaseExtensions** - Startup integration and orchestration
3. **Sourcebook Dependencies** - Proper relational data setup
4. **Idempotent Operations** - Safe for multiple executions

### Integration Points

```csharp
// Program.cs startup integration
await app.EnsureDatabaseCreatedAndSeededAsync();
```

The seeding runs automatically during application startup, ensuring a consistent baseline for all environments.

## GameItemSeeder Implementation

### Class Structure

```csharp
/// <summary>
/// Provides seed data for the game_items table with basic Shadowrun equipment.
/// </summary>
public static class GameItemSeeder
{
    /// <summary>
    /// Seeds basic game items for Shadowrun 6th Edition.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static async Task SeedGameItemsAsync(ShadowrunContext context, CancellationToken cancellationToken = default)
```

**Design Principles:**
- **Static class** - No state, pure data provisioning
- **Async operations** - Non-blocking database interactions
- **Cancellation support** - Responsive to shutdown requests
- **Dependency injection** - Receives context from calling code

### Dependency Management

#### Sourcebook Creation

```csharp
// Check if sourcebooks exist, create core sourcebook if not
Sourcebook? coreSourcebook = await context.Sourcebooks
    .FirstOrDefaultAsync(s => s.Code == "CRB", cancellationToken);

if (coreSourcebook == null)
{
    coreSourcebook = new Sourcebook
    {
        Code = "CRB",
        Title = "Shadowrun Sixth World Core Rulebook",
        Edition = "6e",
        FileName = "seed-data",
        FileHash = "seed-data-core",
        Year = 2019,
        CreatedUtc = DateTime.UtcNow,
        UpdatedUtc = DateTime.UtcNow,
        ImportedAt = DateTimeOffset.UtcNow
    };

    context.Sourcebooks.Add(coreSourcebook);
    await context.SaveChangesAsync(cancellationToken);
}
```

**Key Features:**
- **Dependency resolution** - Creates required sourcebook if missing
- **Proper timestamps** - All audit fields populated
- **Immediate persistence** - SaveChanges before using foreign key
- **Standard metadata** - Consistent with import pipeline structure

#### Idempotent Operations

```csharp
// Check if game items already seeded
bool hasGameItems = await context.GameItems.AnyAsync(cancellationToken);
if (hasGameItems)
    return;
```

**Benefits:**
- **Safe re-execution** - Won't duplicate data if run multiple times
- **Fast early exit** - Minimal database impact when data exists
- **Development workflow** - Supports database reset scenarios

### Data Categories

#### Weapons

```csharp
// Basic Weapons - Light Pistols
new()
{
    Name = "Ares Light Fire 70",
    Slug = "ares-light-fire-70",
    ItemType = "Weapon",
    Category = "Light Pistol",
    Cost = 200m,
    Availability = "2R",
    StatsJson = """{"accuracy": 7, "damage": "2P", "ap": 0, "modes": "SS", "rc": 0, "ammo": "6(c)", "type": "Taser"}""",
    SourcebookId = coreSourcebook.Id,
    Page = 268,
    CreatedAt = now,
    UpdatedAt = now
},
```

**Categories Included:**
- **Light Pistols** - Ares Light Fire 70, Fichetti Security 600
- **Heavy Pistols** - Ares Predator VI, Colt Government 2066  
- **Submachine Guns** - Heckler & Koch MP-2013

#### Armor

```csharp
new()
{
    Name = "Armor Jacket",
    Slug = "armor-jacket",
    ItemType = "Armor", 
    Category = "Light Armor",
    Cost = 1000m,
    Availability = "2",
    StatsJson = """{"defense": 12, "capacity": 8}""",
    SourcebookId = coreSourcebook.Id,
    Page = 266,
    CreatedAt = now,
    UpdatedAt = now
},
```

**Categories Included:**
- **Light Armor** - Armor Jacket, Lined Coat

#### Electronics

```csharp
new()
{
    Name = "Commlink",
    Slug = "commlink-basic",
    ItemType = "Electronics",
    Category = "Communication", 
    Cost = 100m,
    Availability = "1",
    StatsJson = """{"device_rating": 2, "programs": 2, "data_processing": 2, "firewall": 2}""",
    SourcebookId = coreSourcebook.Id,
    Page = 258,
    CreatedAt = now,
    UpdatedAt = now
},
```

**Categories Included:**
- **Communication** - Basic Commlink
- **Decking** - Basic Cyberdeck

#### Additional Categories

- **Medical Gear** - Medkit Rating 3
- **Outdoor Gear** - Climbing Gear  
- **Cyberware** - Datajack, Cybereyes Rating 1
- **Vehicles** - Yamaha Growler (bike), Honda Spirit (car)

### Data Structure Standards

#### JSON Stats Schema

Each item includes a `StatsJson` field with category-specific game mechanics:

**Weapons:**
```json
{
  "accuracy": 7,
  "damage": "2P", 
  "ap": 0,
  "modes": "SS",
  "rc": 0,
  "ammo": "6(c)",
  "type": "Taser"
}
```

**Armor:**
```json
{
  "defense": 12,
  "capacity": 8
}
```

**Electronics:**
```json
{
  "device_rating": 2,
  "programs": 2,
  "data_processing": 2,
  "firewall": 2
}
```

**Cyberware:**
```json
{
  "essence": 0.1,
  "capacity": 0
}
```

#### Metadata Standards

All seeded items include:
- **Name** - Human-readable item name
- **Slug** - URL/API friendly identifier
- **ItemType** - High-level category (Weapon, Armor, etc.)
- **Category** - Specific sub-category
- **Cost** - Nuyen price as decimal
- **Availability** - Shadowrun availability rating
- **SourcebookId** - Foreign key to source material
- **Page** - Reference page in sourcebook
- **Timestamps** - CreatedAt/UpdatedAt for audit trails

## DatabaseExtensions Integration

### Startup Integration

```csharp
/// <summary>
/// Ensures the database is created and seeded with initial data.
/// </summary>
/// <param name="app">The web application.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public static async Task EnsureDatabaseCreatedAndSeededAsync(this WebApplication app)
{
    using IServiceScope scope = app.Services.CreateScope();
    ShadowrunContext context = scope.ServiceProvider.GetRequiredService<ShadowrunContext>();

    // Apply any pending migrations
    await context.Database.MigrateAsync();

    // Seed basic game items
    await GameItemSeeder.SeedGameItemsAsync(context);
}
```

**Features:**
- **Scoped services** - Proper dependency injection lifetime
- **Migration integration** - Ensures schema is current before seeding
- **Error handling** - Exceptions bubble up to application startup
- **Resource cleanup** - Using statement ensures proper disposal

### Development-Specific Seeding

```csharp
/// <summary>
/// Seeds the database with development data.
/// </summary>
/// <param name="app">The web application.</param>
/// <returns>A task representing the asynchronous operation.</returns>
public static async Task SeedDevelopmentDataAsync(this WebApplication app)
{
    if (!app.Environment.IsDevelopment())
        return;

    using IServiceScope scope = app.Services.CreateScope();
    ShadowrunContext context = scope.ServiceProvider.GetRequiredService<ShadowrunContext>();

    // Add development-specific seed data here if needed
    await GameItemSeeder.SeedGameItemsAsync(context);
}
```

**Purpose:**
- **Environment-specific data** - Only runs in development
- **Extended test data** - Could include additional items for testing
- **Developer convenience** - Rich data set for UI development

## Testing Strategy

### Unit Tests for Seeders

```csharp
[Fact]
public async Task SeedGameItemsAsync_WithEmptyDatabase_SeedsAllCategories()
{
    // Arrange
    using ShadowrunContext context = CreateInMemoryContext();
    
    // Act
    await GameItemSeeder.SeedGameItemsAsync(context);
    
    // Assert
    int totalItems = await context.GameItems.CountAsync();
    totalItems.Should().Be(15); // Expected number of seeded items
    
    // Verify categories are represented
    var categories = await context.GameItems
        .Select(i => i.Category)
        .Distinct()
        .ToListAsync();
    
    categories.Should().Contain("Light Pistol");
    categories.Should().Contain("Heavy Pistol");
    categories.Should().Contain("Light Armor");
    // ... etc
}

[Fact]
public async Task SeedGameItemsAsync_WithExistingItems_DoesNotDuplicate()
{
    // Arrange
    using ShadowrunContext context = CreateInMemoryContext();
    await GameItemSeeder.SeedGameItemsAsync(context); // First seeding
    
    int initialCount = await context.GameItems.CountAsync();
    
    // Act
    await GameItemSeeder.SeedGameItemsAsync(context); // Second seeding
    
    // Assert
    int finalCount = await context.GameItems.CountAsync();
    finalCount.Should().Be(initialCount); // No duplicates
}
```

### Integration Tests

```csharp
[Fact]
public async Task EnsureDatabaseCreatedAndSeededAsync_CreatesSchemaAndData()
{
    // Arrange
    WebApplication app = CreateTestApplication();
    
    // Act
    await app.EnsureDatabaseCreatedAndSeededAsync();
    
    // Assert
    using IServiceScope scope = app.Services.CreateScope();
    ShadowrunContext context = scope.ServiceProvider.GetRequiredService<ShadowrunContext>();
    
    // Verify schema exists
    bool canConnect = await context.Database.CanConnectAsync();
    canConnect.Should().BeTrue();
    
    // Verify data exists
    bool hasItems = await context.GameItems.AnyAsync();
    hasItems.Should().BeTrue();
}
```

## Performance Considerations

### Bulk Operations

```csharp
GameItem[] seedItems = 
[
    // All items defined in array
];

context.GameItems.AddRange(seedItems); // Bulk insert
await context.SaveChangesAsync(cancellationToken); // Single transaction
```

**Benefits:**
- **Single transaction** - All items committed together
- **Reduced round trips** - One SaveChanges call
- **Memory efficiency** - Array allocation over individual adds

### Database Efficiency

- **Existence check first** - Avoids unnecessary processing
- **Foreign key resolution** - Sourcebook created before items
- **Proper indexing** - Database indexes support seeding queries
- **Async operations** - Non-blocking execution

## Production Considerations

### Environment Safety

```csharp
public static async Task SeedProductionDataAsync(this WebApplication app)
{
    if (!app.Environment.IsProduction())
        return;
        
    // Production-specific seeding logic
    // More conservative, audit-logged operations
}
```

### Data Versioning

Future enhancements could include:
- **Seed data versioning** - Track which seed data has been applied
- **Migration integration** - Coordinate with EF migrations
- **Rollback support** - Ability to remove or update seed data
- **Audit logging** - Track seeding operations

### Configuration-Driven Seeding

```csharp
public class SeedingOptions
{
    public bool EnableGameItemSeeding { get; set; } = true;
    public bool EnableDevelopmentData { get; set; } = false;
    public string[] EnabledCategories { get; set; } = ["Weapon", "Armor"];
}
```

## Future Enhancements

### Planned Features

1. **Magic Abilities Seeder** - Spells, adept powers, spirit abilities
2. **Character Templates** - Pre-built character examples
3. **Campaign Scenarios** - Sample missions and encounters
4. **Localization Data** - Multi-language content support

### Advanced Seeding Patterns

1. **Conditional Seeding** - Based on feature flags or configuration
2. **Incremental Updates** - Apply only changes since last seed
3. **External Data Sources** - Import from JSON, CSV, or APIs
4. **Validation Integration** - Ensure seeded data meets domain rules

### Data Management Tools

1. **Seed Data Generator** - Tool to create new seed data from rulebooks
2. **Data Export** - Extract current database state as seed files  
3. **Diff Tools** - Compare seed data versions
4. **Validation Reports** - Ensure data integrity across environments