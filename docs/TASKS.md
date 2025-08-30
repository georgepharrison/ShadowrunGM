# Development Tasks: ShadowrunGM (Architecture-First MVP)

*3-4 week implementation with proper patterns*  
*Mobile-first for Google Pixel 8*

## Week 1: Foundation & Architecture

### Day 1-3: Project Structure & Domain
- [x] **TASK-001**: Create solution with bounded contexts (Character, Mission, Campaign, Rules)
- [x] **TASK-002**: Implement Character aggregate with factory method and Edge value object
- [x] **TASK-003**: Create AttributeSet and ConditionMonitor value objects with validation
- [x] **TASK-004**: Setup CQRS source generators and Result pattern library
- [x] **TASK-005**: Define repository interfaces with async operations

### Day 4-5: Database & Infrastructure
- [ ] **TASK-006**: Configure Docker Compose for PostgreSQL with pgvector
- [ ] **TASK-007**: Create initial EF Core migrations with structured tables (game_items, magic_abilities)
- [ ] **TASK-008**: Implement CharacterRepository with EF Core mapping
- [ ] **TASK-009**: Setup basic seed data for game_items table

### Day 6-7: Mobile UI Foundation
- [ ] **TASK-010**: Configure MudBlazor with mobile-first dark theme
- [ ] **TASK-011**: Create responsive layout optimized for Pixel 8 (375px width)
- [ ] **TASK-012**: Build point-buy character creation form with MudCard
- [ ] **TASK-013**: Implement character display component with Edge tracking

**Week 1 Success Criteria**: Domain models compile, DB migrations run, mobile UI renders

---

## Week 2: Core Gameplay Systems

### Day 8-10: Character & Edge Implementation
- [ ] **TASK-014**: Implement CreateCharacterCommand with handler and validation
- [ ] **TASK-015**: Build GetCharacterQuery with DTO projection
- [ ] **TASK-016**: Create SpendEdgeCommand with domain event publishing
- [ ] **TASK-017**: Wire up character creation UI to CQRS handlers

### Day 11-12: Chat Interface
- [ ] **TASK-018**: Build mobile-optimized chat UI with MudPaper bubbles
- [ ] **TASK-019**: Implement persistent Edge display in MudAppBar
- [ ] **TASK-020**: Create keyboard-aware input with proper mobile spacing
- [ ] **TASK-021**: Add chat message persistence to GameSession aggregate

### Day 13-14: Basic AI Integration
- [ ] **TASK-022**: Setup Semantic Kernel with OpenAI/Ollama configuration
- [ ] **TASK-023**: Create IGameMasterService interface with narrative generation
- [ ] **TASK-024**: Implement dice resolution service (configurable visibility)
- [ ] **TASK-025**: Build Edge decision prompt system

**Week 2 Success Criteria**: Can create character, chat with AI, track Edge, dice resolution works

---

## Week 3: AI Orchestration & Content

### Day 15-16: AI Orchestrator Pattern
- [ ] **TASK-026**: Implement IAIOrchestrator with specialized model routing
- [ ] **TASK-027**: Create IRuleInterpreter, IEquipmentLookup, INarrativeGenerator interfaces
- [ ] **TASK-028**: Setup Ollama integration for local RTX 3090 models
- [ ] **TASK-029**: Configure action classification for routing decisions

### Day 17-18: Structured Content & Plugins
- [ ] **TASK-030**: Populate game_items table with basic equipment data
- [ ] **TASK-031**: Create magic_abilities table with sample spells
- [ ] **TASK-032**: Implement EquipmentPlugin for Semantic Kernel
- [ ] **TASK-033**: Build DicePlugin with Edge integration

### Day 19-21: Session Management
- [ ] **TASK-034**: Implement GameSession aggregate with chat history
- [ ] **TASK-035**: Create StartSessionCommand and CompleteSessionCommand
- [ ] **TASK-036**: Build session save/load functionality
- [ ] **TASK-037**: Add Campaign aggregate with karma/nuyen tracking

**Week 3 Success Criteria**: AI orchestrator routes correctly, plugins work, sessions persist

---

## Week 4: Polish & Mobile Optimization

### Day 22-23: Mobile-Specific Enhancements
- [ ] **TASK-038**: Optimize touch targets (44px minimum) throughout UI
- [ ] **TASK-039**: Implement swipe gestures for navigation
- [ ] **TASK-040**: Add PWA manifest for installability
- [ ] **TASK-041**: Test and fix Pixel 8 specific issues

### Day 24-25: Integration & Testing
- [ ] **TASK-042**: Complete end-to-end gameplay test
- [ ] **TASK-043**: Tune AI prompts for better Shadowrun narrative
- [ ] **TASK-044**: Add dice visibility toggle in preferences
- [ ] **TASK-045**: Implement basic import pipeline architecture (interfaces only)

### Day 26-28: Final Polish
- [ ] **TASK-046**: Fix critical bugs from testing
- [ ] **TASK-047**: Optimize initial load time for mobile
- [ ] **TASK-048**: Add character sheet slide-out panel
- [ ] **TASK-049**: Create basic mission completion flow
- [ ] **TASK-050**: Final mobile UI polish and documentation

**Week 4 Success Criteria**: Complete mission playable on Pixel 8, all patterns implemented

---

## Technical Implementation Notes

### Domain Model Structure
```csharp
src/
  Domain/
    Character/
      Character.cs
      CharacterId.cs
      Edge.cs
      AttributeSet.cs
      Events/
        CharacterCreated.cs
        EdgeSpent.cs
    Mission/
      GameSession.cs
      DicePool.cs
      DiceOutcome.cs
    Campaign/
      Campaign.cs
      Karma.cs
      Nuyen.cs
    Rules/
      GameItem.cs
      MagicAbility.cs
```

### CQRS Handler Example
```csharp
public sealed class SpendEdgeHandler : ICommandHandler<SpendEdgeCommand, EdgeSpent>
{
    private readonly ICharacterRepository _repository;
    
    public async Task<Result<EdgeSpent>> HandleAsync(
        SpendEdgeCommand command,
        CancellationToken cancellationToken)
    {
        Result<Character> characterResult = await _repository.GetByIdAsync(command.CharacterId);
        if (!characterResult.IsSuccess)
            return Result<EdgeSpent>.Failure(characterResult.Error);
            
        Result<EdgeSpent> spendResult = characterResult.Value.SpendEdge(command.Amount, command.Purpose);
        if (!spendResult.IsSuccess)
            return spendResult;
            
        await _repository.UpdateAsync(characterResult.Value, cancellationToken);
        return spendResult;
    }
}
```

### Mobile-First Component
```razor
<MudContainer MaxWidth="MaxWidth.Small" Class="pa-0">
    <MudAppBar Fixed="true" Dense="true">
        <MudChip Color="Color.Warning">
            Edge: @Character.EdgeCurrent / @Character.EdgeMax
        </MudChip>
    </MudAppBar>
    
    <MudPaper Class="chat-container">
        @* Chat messages *@
    </MudPaper>
    
    <MudTextField Class="chat-input" 
                  @bind-Value="Message"
                  Variant="Variant.Outlined" />
</MudContainer>
```

---

## Architecture Checkpoints

### Domain-Driven Design
- [ ] All aggregates have factory methods
- [ ] Value objects are immutable
- [ ] Domain events raised for state changes
- [ ] Business logic encapsulated in domain

### CQRS Implementation
- [ ] Commands return Result<T>
- [ ] Queries use DTOs not domain models
- [ ] Handlers use dependency injection
- [ ] Source generators reduce boilerplate

### Mobile-First Design
- [ ] All touch targets ≥ 44px
- [ ] Responsive breakpoints defined
- [ ] Keyboard-aware layouts
- [ ] PWA capabilities enabled

### AI Integration
- [ ] Orchestrator pattern implemented
- [ ] Specialized models configured
- [ ] Ollama local models working
- [ ] Semantic Kernel plugins registered

---

## Definition of Done

### Per Task
- [ ] Code follows CLAUDE.md standards
- [ ] Domain logic has unit tests
- [ ] CQRS handlers execute successfully
- [ ] Mobile UI works on Pixel 8
- [ ] No compiler warnings

### Per Week
- [ ] All tasks completed or explicitly deferred
- [ ] Integration tests passing
- [ ] Mobile experience validated
- [ ] Architecture patterns consistent

---

## Deferred Items (Post-MVP)

### Acknowledged but Not Required
- Full import pipeline implementation (interfaces only)
- Complex magic system (basic spells sufficient)
- Matrix subsystem (narrative hacking only)
- Priority character creation (point-buy sufficient)
- Advanced augmentation tracking
- Multiplayer support
- Desktop-specific optimizations

### Architectural Hooks Created
- Import pipeline interfaces defined
- Magic system entities structured
- Matrix action stubs in place
- Campaign event sourcing prepared