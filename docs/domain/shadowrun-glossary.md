# Shadowrun Domain Glossary

This comprehensive glossary defines both Shadowrun 6th Edition terminology and ShadowrunGM domain-driven design concepts, providing a unified reference for understanding the game mechanics and software architecture.

## Table of Contents

- [Core Game Mechanics](#core-game-mechanics)
- [Character System](#character-system)  
- [Mission & Session Management](#mission--session-management)
- [Dice Rolling & Tests](#dice-rolling--tests)
- [Edge Mechanics](#edge-mechanics)
- [Combat & Damage](#combat--damage)
- [Magic & Technology](#magic--technology)
- [Domain-Driven Design Terms](#domain-driven-design-terms)
- [Technical Architecture](#technical-architecture)

---

## Core Game Mechanics

### Attribute
**Domain Context:** Character Bounded Context  
**Type:** Value Object Component  
A fundamental character statistic representing natural capabilities. In Shadowrun 6th Edition, attributes range from 1-12 and include Body, Agility, Reaction, Strength, Willpower, Logic, Intuition, and Charisma. Attributes form the base dice pool for all tests.

**Technical Implementation:**
```csharp
// Part of AttributeSet value object
public sealed class AttributeSet : ValueObject
{
    public int Body { get; }
    public int Agility { get; }
    public int Reaction { get; }
    // ... other attributes
}
```

### Dice Pool
**Domain Context:** Mission Bounded Context  
**Type:** Value Object  
The total number of six-sided dice rolled for a test, calculated from Attribute + Skill + Modifiers + Edge bonuses. Represents the character's capability in a specific situation.

**Game Rule:** Minimum dice pool is 0 (no dice rolled), practical maximum is typically 20-30 dice.

### Hits
**Domain Context:** Mission Bounded Context  
**Type:** Value Object Property  
The number of successes achieved in a dice roll. Each die showing 5 or 6 counts as one hit. Hits determine the degree of success for actions.

### Limit
**Domain Context:** Mission Bounded Context  
**Type:** Value Object Property  
The maximum number of hits that can count toward success, unless Edge is used to ignore limits. Represents physical, mental, or social constraints on performance.

### Test
**Domain Context:** Mission Bounded Context  
**Business Process:** Dice rolling with success/failure determination  
An action resolution mechanism where dice are rolled against a target number or threshold. Forms the core conflict resolution system in Shadowrun.

---

## Character System

### Character
**Domain Context:** Character Bounded Context  
**Type:** Aggregate Root  
The primary entity representing a player character or NPC in the Shadowrun universe. Manages attributes, skills, health, Edge points, and progression.

**Aggregate Boundaries:**
- Maintains character identity and core statistics
- Manages Edge point economy
- Tracks condition monitor (health/damage)
- Controls skill advancement and augmentation

### Metatype
**Domain Context:** Character Bounded Context  
**Type:** Value Object  
The character's species/race in the Shadowrun world: Human, Elf, Dwarf, Ork, or Troll. Determines base attribute ranges, special abilities, and social prejudices.

**Technical Note:** Referenced as enum or value object for type safety.

### Skill
**Domain Context:** Character Bounded Context  
**Type:** Value Object  
Learned abilities that modify dice pools for specific actions. Skills range from 0-12 and are combined with attributes for tests. Examples include Firearms, Hacking, Spellcasting.

### Augmentation
**Domain Context:** Character Bounded Context  
**Type:** Entity (within Character aggregate)  
Cybernetic implants, bioware, or magical enhancements that modify character capabilities. Each augmentation has costs, benefits, and potential drawbacks.

### Condition Monitor
**Domain Context:** Character Bounded Context  
**Type:** Value Object  
Tracks physical and stun damage. Characters become increasingly impaired as damage accumulates, eventually leading to unconsciousness or death.

**Game Rule:** Physical and Stun tracks calculated from Body + Willpower attributes, typically 8-15 boxes each.

---

## Mission & Session Management

### Game Session
**Domain Context:** Mission Bounded Context  
**Type:** Aggregate Root  
Represents a bounded period of gameplay interaction between players and GM. Manages session lifecycle (Active, Paused, Completed), chat messages, and dice roll history.

**Session States:**
- **Active:** Normal gameplay, all operations allowed
- **Paused:** Temporary suspension, limited operations
- **Completed:** Terminal state, read-only historical record

### Chat Message
**Domain Context:** Mission Bounded Context  
**Type:** Value Object  
A communication record within a game session, classified by sender type (Player, GameMaster, System, Narrative) and containing content, timestamp, and metadata.

**Message Types:**
- **Player:** User input and character actions
- **GameMaster:** AI-generated responses and rulings
- **System:** Automated dice roll results and game events
- **Narrative:** Descriptive scene-setting content

### Scene
**Domain Context:** Mission Bounded Context  
**Future Implementation:** Entity within Mission aggregate  
A discrete narrative unit within a mission, such as a combat encounter, investigation, or social interaction. Manages scene-specific context and pacing.

### Run
**Domain Context:** Mission Bounded Context  
**Future Implementation:** Aggregate Root  
A complete shadowrun mission from planning through execution. Coordinates multiple scenes, tracks objectives, manages payouts, and handles consequences.

---

## Dice Rolling & Tests

### Dice Outcome
**Domain Context:** Mission Bounded Context  
**Type:** Value Object  
The resolved result of a dice pool roll, containing individual die results, hit count, glitch status, and success/failure determination.

**Components:**
- **Rolls:** Array of individual die results (1-6)
- **Hits:** Count of successes (5s and 6s)
- **Ones:** Count of 1s rolled (for glitch calculation)
- **IsGlitch:** More than half dice showed 1s
- **IsCriticalGlitch:** Glitch with zero hits (catastrophic failure)

### Threshold Test
**Domain Context:** Mission Bounded Context  
**Business Rule:** Success determined by meeting or exceeding target hit count  
A test where a specific number of hits must be achieved. Common for skill checks, spellcasting, and technical operations.

### Opposed Test
**Domain Context:** Mission Bounded Context  
**Business Rule:** Comparative success between two dice pools  
Two parties roll dice pools, winner determined by higher hit count. Used for stealth vs. perception, social influence, combat initiatives.

### Extended Test
**Domain Context:** Mission Bounded Context  
**Business Rule:** Multiple tests required to accumulate threshold hits  
A series of tests performed over time to complete complex actions. Each test adds to cumulative hit total until threshold is reached.

---

## Edge Mechanics

### Edge
**Domain Context:** Character Bounded Context  
**Type:** Value Object  
Shadowrun's luck/karma resource representing a character's ability to push beyond normal limits. Spent for various benefits, regained through good roleplay and story progression.

**Edge Economy:**
- **Current Edge:** Available points for immediate use
- **Maximum Edge:** Character's Edge attribute rating
- **Burned Edge:** Permanently spent points (rare, dramatic effects)

### Edge Actions
**Domain Context:** Mission Bounded Context  
**Business Rules:** Various Edge spending options with specific mechanical effects

- **Push the Limit:** Add Edge rating to dice pool
- **Second Chance:** Reroll failed dice
- **Seize the Initiative:** Go first in initiative order
- **Blitz:** Extra actions in combat
- **Close Call:** Avoid death or critical consequences

### Exploding Sixes
**Domain Context:** Mission Bounded Context  
**Technical Rule:** When Edge is spent, 6s generate additional dice  
Edge rule where each 6 rolled generates an additional die, which can itself explode on a 6. Creates potential for exceptional results.

---

## Combat & Damage

### Initiative
**Domain Context:** Mission Bounded Context  
**Business Rule:** Reaction + Intuition + 1d6, determines action order  
The sequence in which characters act during combat turns. Higher initiative acts first, with ties broken by Reaction + Intuition.

### Action Phase
**Domain Context:** Mission Bounded Context  
**Business Rule:** Characters act in initiative order, can take multiple actions with penalties  
A discrete moment in combat where one character can perform actions. Multiple action phases occur each combat turn.

### Physical Damage
**Domain Context:** Character Bounded Context  
**Type:** Condition Monitor Component  
Bodily harm from weapons, environmental hazards, or magical effects. Accumulated in boxes on the Physical condition monitor, causing dice pool penalties and eventual unconsciousness/death.

### Stun Damage
**Domain Context:** Character Bounded Context  
**Type:** Condition Monitor Component  
Non-lethal harm from shock weapons, drain, fatigue, or psychological stress. Fills Stun condition monitor, causing penalties and eventual unconsciousness.

### Overflow Damage
**Domain Context:** Character Bounded Context  
**Business Rule:** When Physical track is full, additional damage causes death  
Lethal damage that exceeds the Physical condition monitor capacity, resulting in character death unless immediately treated.

---

## Magic & Technology

### Spell
**Domain Context:** Character Bounded Context (future expansion)  
**Type:** Entity within Magical Character specialization  
Formulaic magical effects that manipulate reality through metaphysical principles. Each spell has drain costs, thresholds, and specific mechanics.

### Cyberware
**Domain Context:** Character Bounded Context  
**Type:** Augmentation specialization  
Electronic implants that replace or enhance natural body parts. Provides bonuses but increases Essence loss and may cause incompatibilities.

### Bioware
**Domain Context:** Character Bounded Context  
**Type:** Augmentation specialization  
Biological modifications using genetically-engineered tissues. More expensive than cyberware but causes less Essence loss.

### Essence
**Domain Context:** Character Bounded Context  
**Type:** Value Object Property  
Metaphysical measure of a character's life force and magical potential. Reduced by cybernetic and biological augmentations, affecting magical ability and healing.

### Matrix
**Domain Context:** Future Bounded Context  
**Type:** Separate domain for cyberpunk hacking mechanics  
The global computer network in Shadowrun's world. Accessed through cyberterminals or direct neural interface, governed by complex hacking rules.

---

## Domain-Driven Design Terms

### Aggregate
**Technical Context:** Domain Architecture  
A cluster of domain objects treated as a single unit for data changes. Each aggregate has a root entity that maintains consistency and controls access to aggregate members.

**ShadowrunGM Aggregates:**
- **Character:** Manages character data, attributes, skills, health
- **GameSession:** Manages session lifecycle, chat, dice rolling
- **Mission:** (Future) Manages run objectives, scenes, payouts

### Bounded Context
**Technical Context:** Domain Architecture  
A explicit boundary within which a domain model is defined and applicable. Different contexts can have different meanings for the same terms.

**ShadowrunGM Contexts:**
- **Character Context:** Character creation, advancement, equipment
- **Mission Context:** Session management, dice mechanics, narrative
- **Import Context:** PDF parsing, rule extraction, data structuring

### Value Object
**Technical Context:** Domain Implementation  
An object that represents a descriptive aspect of the domain with no conceptual identity. Value objects are immutable and compared by value equality.

**Examples:** DicePool, AttributeSet, DiceOutcome, ChatMessage

### Domain Event
**Technical Context:** Integration Pattern  
An event representing something significant that happened in the domain. Used for eventual consistency between aggregates and integration with external systems.

**Examples:** CharacterCreated, SessionStarted, DiceRolled, EdgeSpent

### Domain Service
**Technical Context:** Domain Implementation  
Encapsulates domain logic that doesn't naturally belong to any single entity or value object. Often coordinates between multiple domain objects.

**Examples:** IDiceService (dice rolling algorithms), ICharacterBuilder (complex character creation)

---

## Technical Architecture

### Result Pattern
**Technical Context:** Error Handling Architecture  
A comprehensive pattern for explicit success/failure handling throughout the application. Replaces exceptions with typed results containing either successful values or detailed error information.

**Result Types:**
- `Result` - Non-generic for operations without return values
- `Result<T>` - Generic for operations returning typed values
- Supports validation errors, security exceptions, operation cancellation

### Validation Framework
**Technical Context:** Application Layer  
Fluent validation system integrated with Result<T> pattern, providing comprehensive rule-based validation with automatic error aggregation and composition.

**Features:**
- Property-specific validators (string, numeric, enumerable)
- Result<T> composition and error merging  
- Conditional validation and custom error messages
- Type-safe expression-based property selection

### CQRS (Command Query Responsibility Segregation)
**Technical Context:** Application Architecture  
Architectural pattern separating read and write operations, enabling optimized data models and scalable query performance.

**Implementation:**
- Commands modify domain aggregates through handlers
- Queries read from optimized projections/views
- Result<T> pattern for consistent error handling
- Event-driven synchronization between write and read models

### Repository Pattern
**Technical Context:** Infrastructure Abstraction  
Abstraction layer over data persistence, providing domain-focused interface while hiding infrastructure concerns like Entity Framework or database specifics.

**Types:**
- Write repositories: Domain aggregate persistence
- Read repositories: Query-optimized data access
- Interface segregation: Separate concerns by usage patterns

---

*ShadowrunGM Domain Glossary - Comprehensive Edition 2025-08-30*