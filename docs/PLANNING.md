# Technical Planning Document: ShadowrunGM (DDD Architecture)

*Version 3.0 - Proper Architecture with Rapid MVP Delivery*  
*Last Updated: August 30, 2025*

## Architecture Overview

### Core Architectural Principles

1. **Domain-Driven Design**: Bounded contexts with rich domain models
2. **CQRS Pattern**: Source generator implementation with Result pattern
3. **Mobile-First Responsive**: Google Pixel 8 as primary target
4. **AI Orchestration**: Specialized models for different concerns
5. **Incremental Complexity**: Start with core aggregates, expand later

### System Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                 Blazor WASM (Mobile-First)                  │
│     MudBlazor Components → ViewModels → API Client          │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│              Application Layer (CQRS)                        │
│  Commands → Handlers → Domain Services → Queries            │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│           Domain Layer (Bounded Contexts)                    │
│  Character → Mission → Campaign → Rules                     │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│              Infrastructure Layer                            │
│  EF Core → PostgreSQL → pgvector → Ollama → Semantic Kernel │
└──────────────────────────────────────────────────────────────┘
```

---

## Domain-Driven Design Implementation

### Bounded Contexts (MVP Scope)

#### 1. Character Context
**Purpose**: Manage character lifecycle and state

**Aggregate Root**: Character
```csharp
public sealed class Character : AggregateRoot
{
    private readonly List<Skill> _skills = [];
    private readonly List<DomainEvent> _domainEvents = [];
    
    public CharacterId Id { get; private init; }
    public string Name { get; private init; }
    public AttributeSet Attributes { get; private set; }
    public Edge Edge { get; private set; }
    public ConditionMonitor Health { get; private set; }
    
    private Character() { } // EF Core
    
    public static Result<Character> Create(
        string name,
        AttributeSet attributes,
        int startingEdge)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Character>.Failure("Character name required");
            
        Character character = new()
        {
            Id = CharacterId.New(),
            Name = name,
            Attributes = attributes,
            Edge = Edge.Create(startingEdge),
            Health = ConditionMonitor.ForAttributes(attributes)
        };
        
        character.RaiseDomainEvent(new CharacterCreated(character.Id, name));
        return Result<Character>.Success(character);
    }
    
    public Result<EdgeSpent> SpendEdge(int amount, string purpose)
    {
        Result<Edge> edgeResult = Edge.Spend(amount);
        if (!edgeResult.IsSuccess)
            return Result<EdgeSpent>.Failure(edgeResult.Error);
            
        Edge = edgeResult.Value;
        EdgeSpent domainEvent = new(Id, amount, purpose);
        RaiseDomainEvent(domainEvent);
        
        return Result<EdgeSpent>.Success(domainEvent);
    }
}
```

**Value Objects**:
- `CharacterId` - Strongly typed ID
- `AttributeSet` - All attributes with validation
- `Edge` - Current/max with business rules
- `ConditionMonitor` - Physical/Stun damage tracking
- `Skill` - Name and rating

#### 2. Mission Context
**Purpose**: Handle gameplay sessions and dice resolution

**Aggregate Root**: GameSession (Simplified from Mission)
```csharp
public sealed class GameSession : AggregateRoot
{
    private readonly List<ChatMessage> _messages = [];
    private readonly List<DiceRoll> _rolls = [];
    
    public SessionId Id { get; private init; }
    public CharacterId CharacterId { get; private init; }
    public SessionState State { get; private set; }
    public DateTime StartedAt { get; private init; }
    
    public static Result<GameSession> Start(CharacterId characterId)
    {
        GameSession session = new()
        {
            Id = SessionId.New(),
            CharacterId = characterId,
            State = SessionState.Active,
            StartedAt = DateTime.UtcNow
        };
        
        session.RaiseDomainEvent(new SessionStarted(session.Id, characterId));
        return Result<GameSession>.Success(session);
    }
    
    public Result<DiceOutcome> ResolveDiceRoll(
        DicePool pool,
        IDiceService diceService)
    {
        DiceOutcome outcome = diceService.Roll(pool);
        _rolls.Add(new DiceRoll(pool, outcome, DateTime.UtcNow));
        
        RaiseDomainEvent(new DiceRolled(Id, pool, outcome));
        return Result<DiceOutcome>.Success(outcome);
    }
}
```

**Value Objects**:
- `SessionId` - Strongly typed ID
- `DicePool` - Attribute + Skill + Modifiers
- `DiceOutcome` - Hits, glitches, narrative result
- `ChatMessage` - Player/GM messages with metadata

**Domain Services**:
- `IDiceService` - Calculate and roll dice
- `IEdgeCalculator` - Determine Edge gain opportunities

#### 3. Campaign Context (Minimal MVP)
**Purpose**: Track persistent world state

**Aggregate Root**: Campaign
```csharp
public sealed class Campaign : AggregateRoot
{
    private readonly List<Contact> _contacts = [];
    
    public CampaignId Id { get; private init; }
    public CharacterId CharacterId { get; private init; }
    public Karma Karma { get; private set; }
    public Nuyen Nuyen { get; private set; }
    
    public Result<KarmaAwarded> AwardKarma(int amount, string reason)
    {
        Karma = Karma.Add(amount);
        KarmaAwarded domainEvent = new(Id, amount, reason);
        RaiseDomainEvent(domainEvent);
        return Result<KarmaAwarded>.Success(domainEvent);
    }
}
```

#### 4. Rules Context
**Purpose**: Structured content for AI lookups

**Entities** (Not aggregates - read-only reference data):
```csharp
public sealed class GameItem
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Category { get; init; }
    public Cost Cost { get; init; }
    public Availability Availability { get; init; }
    public string Description { get; init; }
    public Vector Embedding { get; init; } // For semantic search
}

public sealed class MagicAbility
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Type { get; init; } // Spell, Power, etc.
    public string Description { get; init; }
    public DrainValue Drain { get; init; }
    public Vector Embedding { get; init; }
}
```

---

## CQRS Implementation

### Command Pattern
```csharp
// Command definition
public sealed record CreateCharacterCommand(
    string Name,
    Dictionary<string, int> Attributes,
    Dictionary<string, int> Skills,
    int StartingEdge) : ICommand<CharacterId>;

// Handler with source generator support
public sealed class CreateCharacterHandler : ICommandHandler<CreateCharacterCommand, CharacterId>
{
    private readonly ICharacterRepository _repository;
    private readonly ICharacterValidationService _validator;
    
    public CreateCharacterHandler(
        ICharacterRepository repository,
        ICharacterValidationService validator)
    {
        _repository = repository;
        _validator = validator;
    }
    
    public async Task<Result<CharacterId>> HandleAsync(
        CreateCharacterCommand command,
        CancellationToken cancellationToken)
    {
        // Validate through domain service
        Result<AttributeSet> attributesResult = AttributeSet.Create(command.Attributes);
        if (!attributesResult.IsSuccess)
            return Result<CharacterId>.Failure(attributesResult.Error);
        
        // Create through factory method
        Result<Character> characterResult = Character.Create(
            command.Name,
            attributesResult.Value,
            command.StartingEdge);
            
        if (!characterResult.IsSuccess)
            return Result<CharacterId>.Failure(characterResult.Error);
        
        // Persist through repository
        await _repository.AddAsync(characterResult.Value, cancellationToken);
        
        return Result<CharacterId>.Success(characterResult.Value.Id);
    }
}
```

### Query Pattern
```csharp
public sealed record GetCharacterQuery(CharacterId Id) : IQuery<CharacterDto>;

public sealed class GetCharacterHandler : IQueryHandler<GetCharacterQuery, CharacterDto>
{
    private readonly IShadowrunDbContext _context;
    
    public async Task<Result<CharacterDto>> HandleAsync(
        GetCharacterQuery query,
        CancellationToken cancellationToken)
    {
        CharacterDto? character = await _context.Characters
            .Where(c => c.Id == query.Id)
            .Select(c => new CharacterDto
            {
                Id = c.Id,
                Name = c.Name,
                EdgeCurrent = c.EdgeCurrent,
                EdgeMax = c.EdgeMax,
                // Project to DTO
            })
            .FirstOrDefaultAsync(cancellationToken);
            
        return character is not null
            ? Result<CharacterDto>.Success(character)
            : Result<CharacterDto>.Failure("Character not found");
    }
}
```

---

## AI Orchestration Architecture

### Orchestrator Pattern
```csharp
public interface IAIOrchestrator
{
    Task<Result<string>> ProcessGameAction(GameContext context, string playerInput);
    Task<Result<EdgeDecision>> GenerateEdgeChoice(EdgeContext context);
    Task<Result<DiceOutcome>> ResolveDiceRoll(DiceContext context);
}

public sealed class SemanticKernelOrchestrator : IAIOrchestrator
{
    private readonly IKernel _kernel;
    private readonly IRuleInterpreter _ruleInterpreter;
    private readonly IEquipmentLookup _equipmentLookup;
    private readonly INarrativeGenerator _narrativeGenerator;
    private readonly IDiceResolver _diceResolver;
    
    public async Task<Result<string>> ProcessGameAction(
        GameContext context,
        string playerInput)
    {
        // Determine action type
        ActionType actionType = await ClassifyAction(playerInput);
        
        // Route to specialized model
        return actionType switch
        {
            ActionType.Combat => await _diceResolver.ResolveCombat(context, playerInput),
            ActionType.Equipment => await _equipmentLookup.FindEquipment(context, playerInput),
            ActionType.Rule => await _ruleInterpreter.InterpretRule(context, playerInput),
            _ => await _narrativeGenerator.GenerateNarrative(context, playerInput)
        };
    }
}
```

### Semantic Kernel Plugins
```csharp
public sealed class EquipmentPlugin
{
    private readonly IGameItemRepository _repository;
    
    [KernelFunction("find_equipment")]
    [Description("Find equipment by name or description")]
    public async Task<IEnumerable<GameItem>> FindEquipment(
        [Description("Equipment search term")] string searchTerm)
    {
        // Use pgvector for semantic search
        return await _repository.SemanticSearchAsync(searchTerm, limit: 5);
    }
}

public sealed class DicePlugin
{
    private readonly IDiceService _diceService;
    
    [KernelFunction("roll_dice")]
    [Description("Roll Shadowrun dice pool")]
    public DiceOutcome RollDice(
        [Description("Number of dice")] int poolSize,
        [Description("Edge bonus dice")] int edgeBonus = 0)
    {
        DicePool pool = new(poolSize + edgeBonus);
        return _diceService.Roll(pool);
    }
}
```

---

## Database Schema

### Character Context Tables
```sql
-- Characters aggregate
CREATE TABLE characters (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    attributes JSONB NOT NULL,
    edge_current INT NOT NULL,
    edge_max INT NOT NULL,
    physical_damage INT DEFAULT 0,
    stun_damage INT DEFAULT 0,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
);

-- Skills as separate table for querying
CREATE TABLE character_skills (
    id UUID PRIMARY KEY,
    character_id UUID REFERENCES characters(id),
    skill_name VARCHAR(100) NOT NULL,
    rating INT NOT NULL,
    specialization VARCHAR(100),
    UNIQUE(character_id, skill_name)
);
```

### Mission Context Tables
```sql
-- Game sessions
CREATE TABLE game_sessions (
    id UUID PRIMARY KEY,
    character_id UUID REFERENCES characters(id),
    state VARCHAR(50) NOT NULL,
    started_at TIMESTAMPTZ DEFAULT NOW(),
    completed_at TIMESTAMPTZ,
    chat_messages JSONB DEFAULT '[]'
);

-- Dice rolls for audit
CREATE TABLE dice_rolls (
    id UUID PRIMARY KEY,
    session_id UUID REFERENCES game_sessions(id),
    dice_pool JSONB NOT NULL,
    outcome JSONB NOT NULL,
    rolled_at TIMESTAMPTZ DEFAULT NOW()
);
```

### Rules Context Tables (Structured for AI)
```sql
-- Game items for equipment lookup
CREATE TABLE game_items (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    category VARCHAR(100) NOT NULL,
    subcategory VARCHAR(100),
    cost INT,
    availability VARCHAR(50),
    legality VARCHAR(10),
    description TEXT,
    stats JSONB,
    embedding vector(768),
    source_book VARCHAR(100),
    page_number INT
);

-- Magic abilities for spell/power lookup
CREATE TABLE magic_abilities (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    type VARCHAR(50) NOT NULL, -- Spell, Adept Power, etc.
    category VARCHAR(100),
    force_min INT,
    drain_value VARCHAR(50),
    description TEXT,
    effects JSONB,
    embedding vector(768),
    source_book VARCHAR(100),
    page_number INT
);

-- Indexes for semantic search
CREATE INDEX idx_game_items_embedding ON game_items 
USING ivfflat (embedding vector_cosine_ops);

CREATE INDEX idx_magic_abilities_embedding ON magic_abilities 
USING ivfflat (embedding vector_cosine_ops);
```

---

## Mobile-First UI Architecture

### Responsive Breakpoints (Pixel 8 Primary)
```csharp
public static class Breakpoints
{
    public const string Mobile = "@media (max-width: 600px)"; // Pixel 8 portrait
    public const string Tablet = "@media (min-width: 601px) and (max-width: 1024px)";
    public const string Desktop = "@media (min-width: 1025px)";
}
```

### Component Structure
```razor
@* ChatInterface.razor - Mobile-first design *@
<MudContainer MaxWidth="MaxWidth.Small" Class="pa-0">
    @* Persistent Edge display *@
    <MudAppBar Fixed="true" Dense="true">
        <MudText>@Character.Name</MudText>
        <MudSpacer />
        <MudChip Color="Color.Warning">
            Edge: @Character.EdgeCurrent / @Character.EdgeMax
        </MudChip>
    </MudAppBar>
    
    @* Chat messages *@
    <MudPaper Class="chat-container">
        @foreach (var message in Messages)
        {
            <ChatMessage Message="@message" />
        }
    </MudPaper>
    
    @* Input area - keyboard aware *@
    <MudPaper Class="chat-input" Elevation="4">
        <MudTextField @bind-Value="CurrentMessage"
                      Variant="Variant.Outlined"
                      Adornment="Adornment.End"
                      AdornmentIcon="@Icons.Material.Filled.Send"
                      OnAdornmentClick="SendMessage" />
    </MudPaper>
</MudContainer>

<style>
    .chat-container {
        height: calc(100vh - 120px); /* Account for app bar and input */
        overflow-y: auto;
        padding: 8px;
    }
    
    .chat-input {
        position: fixed;
        bottom: 0;
        width: 100%;
        padding: 8px;
    }
    
    @@media (min-width: 601px) {
        .chat-container {
            max-width: 600px;
            margin: 0 auto;
        }
    }
</style>
```

---

## Development Timeline (3-4 Weeks)

### Week 1: Foundation & Architecture
**Day 1-3: Project Structure**
- Solution setup with bounded contexts
- CQRS source generator configuration
- Domain model foundations (Character, Edge, DicePool)
- Repository interfaces and EF Core setup

**Day 4-5: Database & Infrastructure**
- PostgreSQL with Docker Compose
- Initial migrations with structured tables
- Basic repository implementations
- Result pattern integration

**Day 6-7: Mobile UI Shell**
- MudBlazor configuration with mobile theme
- Responsive layout for Pixel 8
- Character creation form (point-buy)
- Basic navigation structure

### Week 2: Core Gameplay Implementation
**Day 8-10: Character & Edge Systems**
- Character aggregate implementation
- Edge value object with business rules
- CQRS handlers for character operations
- Character display components

**Day 11-12: Chat Interface**
- Mobile-optimized chat UI
- Message persistence
- Keyboard-aware layout
- Edge display integration

**Day 13-14: AI Integration**
- Semantic Kernel setup
- Basic narrative generation
- Dice resolution service
- Edge decision prompts

### Week 3: AI Orchestration & Content
**Day 15-16: AI Orchestrator**
- Orchestrator pattern implementation
- Specialized model routing
- Ollama integration for local models
- Plugin architecture

**Day 17-18: Structured Content**
- game_items table population
- magic_abilities table setup
- Semantic search implementation
- Equipment lookup plugin

**Day 19-21: Session Management**
- GameSession aggregate
- Save/load functionality
- Dice roll history
- Campaign state basics

### Week 4: Polish & Testing
**Day 22-23: Mobile Optimization**
- Pixel 8 specific testing
- Touch target optimization
- Swipe gesture support
- PWA manifest

**Day 24-25: Integration Testing**
- End-to-end gameplay testing
- AI response tuning
- Edge case handling
- Performance optimization

**Day 26-28: Final Polish**
- Bug fixes
- UI refinements
- Documentation
- Deployment preparation

---

## Risk Mitigation Strategies

### Architecture Complexity
- **Risk**: Over-engineering for personal project
- **Mitigation**: Start with minimal aggregates, add complexity incrementally
- **Benefit**: Clean boundaries enable future expansion

### Mobile Performance
- **Risk**: Blazor WASM size on mobile
- **Mitigation**: Lazy loading, PWA caching, minimal initial bundle
- **Benefit**: Offline capability once cached

### AI Response Time
- **Risk**: Latency with cloud models
- **Mitigation**: Ollama for common operations, cloud for complex
- **Benefit**: RTX 3090 provides excellent local performance

### Timeline Pressure
- **Risk**: 3-4 week timeline with proper architecture
- **Mitigation**: MVP scope strictly defined, defer complex features
- **Benefit**: Solid foundation prevents technical debt

---

## Success Criteria

### Week 1 Checkpoint
- [ ] Domain models compile and pass tests
- [ ] CQRS handlers execute successfully
- [ ] Database migrations run
- [ ] Mobile UI renders on Pixel 8

### Week 2 Checkpoint
- [ ] Character creation works end-to-end
- [ ] Chat interface functional
- [ ] Edge tracking operational
- [ ] AI responds coherently

### Week 3 Checkpoint
- [ ] AI orchestrator routes correctly
- [ ] Structured tables queryable
- [ ] Sessions persist and load
- [ ] Plugins integrate with Semantic Kernel

### Week 4 Checkpoint
- [ ] Complete mission playable
- [ ] Mobile experience polished
- [ ] All tests passing
- [ ] Ready for personal use