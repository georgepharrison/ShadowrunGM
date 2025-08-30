# Domain Layer

This layer contains the core business logic and domain models for ShadowrunGM, following Domain-Driven Design principles.

## Aggregates

| Aggregate | Purpose | Documentation |
|-----------|---------|---------------|
| Character | Represents a player character or NPC in the Shadowrun universe with attributes, skills, Edge, and health management. | [Character Docs](../../docs/domain/character-aggregate.md) |
| GameSession | Manages game session lifecycle, real-time chat interactions, and Shadowrun dice rolling mechanics. | [Mission Docs](../../docs/domain/mission-aggregate.md) |


## Architecture Principles

- **Aggregate Roots:** Maintain consistency boundaries and coordinate changes within bounded contexts
- **Value Objects:** Immutable objects with equality based on value, encapsulating business rules and validation
- **Domain Events:** Capture important business events for eventual consistency and cross-context integration
- **Domain Services:** Encapsulate domain logic that spans multiple entities (e.g., dice rolling algorithms)
- **Result Pattern:** Explicit success/failure handling without exceptions, integrated with validation framework
- **Bounded Contexts:** Clear separation between Character and Mission domains with well-defined integration points

## Domain Concepts

### Bounded Contexts

**Character Context:**
- Character creation and management
- Attribute and skill systems  
- Edge point economy and spending
- Condition monitor (health/damage tracking)
- Augmentation and equipment management

**Mission Context:**
- Game session lifecycle (Active, Paused, Completed states)
- Real-time chat message handling (Player, GM, System, Narrative types)
- Shadowrun dice mechanics (dice pools, hits, glitches, Edge rules)
- Session history and audit trail maintenance

### Cross-Context Integration

- **Identity References:** Mission aggregate references Character via CharacterId
- **Domain Events:** Loose coupling through event-driven architecture
- **Shared Kernel:** Result<T> pattern, validation framework, common value object patterns

## Detailed Documentation

- [Character Aggregate](../../docs/domain/character-aggregate.md) - Complete character system documentation
- [Mission Aggregate](../../docs/domain/mission-aggregate.md) - Session and dice rolling mechanics
- [Shadowrun Glossary](../../docs/domain/shadowrun-glossary.md) - Comprehensive terminology reference
- [Domain Architecture](../../docs/architecture/domain-architecture.md) - Overall domain structure and patterns

## Related Documentation

- [Application Layer CQRS](../Application/README.md) - Command/Query handlers and validation
- [Result Pattern](../Application/Common/Results/) - Validation framework and error handling  
- [Architecture Overview](../../docs/architecture/README.md) - System-wide architectural decisions
