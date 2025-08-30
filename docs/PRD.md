# Product Requirements Document: ShadowrunGM (Architect Edition)

*Version 3.0 - Personal MVP with Proper Architecture*  
*Last Updated: August 30, 2025*

## Executive Summary

ShadowrunGM is a personal AI-powered gaming assistant for solo Shadowrun 6th Edition play, built with enterprise-grade architecture for long-term maintainability. The application leverages Domain-Driven Design, CQRS patterns, and AI orchestration to handle dice mechanics while preserving tactical Edge decisions. Designed for mobile-first usage on Google Pixel 8 with proper architectural foundations that support incremental feature expansion.

---

## Product Vision

### Mission Statement
To create an architecturally sound, AI-powered game master that handles Shadowrun 6e complexity while maintaining code quality and extensibility befitting a Senior Staff Engineer's personal project.

### Core Value Propositions
1. **Architectural Excellence**: DDD, CQRS, and proper patterns from day one
2. **AI Orchestration**: Multiple specialized models for different concerns
3. **Mobile-First Design**: Optimized for Google Pixel 8 daily usage
4. **Configurable Transparency**: Hidden dice by default, optional visibility
5. **Incremental Complexity**: Start simple, expand without refactoring

### Target User Profile
- **Single User**: Senior Staff Engineer/Architect with high code quality standards
- **Mobile Primary**: Google Pixel 8 for on-the-go sessions
- **Local AI Capability**: RTX 3090 for Ollama model hosting
- **Time-Conscious**: 3-4 week MVP timeline with proper patterns

---

## Functional Requirements

### 1. Character Management (Character Bounded Context)

#### 1.1 Character Creation
**Priority**: CRITICAL  
**User Story**: As a player, I want point-buy character creation with DDD-compliant architecture.

**Acceptance Criteria:**
- Point-buy system with AI validation assistance
- Character aggregate with proper invariant protection
- CQRS commands for character creation/modification
- Result pattern for all operations
- Mobile-responsive form layout for Pixel 8

**Technical Requirements:**
- Character aggregate root with factory methods
- Value objects for attributes and skills
- Domain events for character state changes
- Repository pattern with EF Core implementation

#### 1.2 Character State Management
**Priority**: CRITICAL  
**User Story**: As a player, I want character state properly encapsulated in domain model.

**Acceptance Criteria:**
- Edge tracking as value object with business rules
- Health/damage tracking with condition monitors
- Skill and attribute modifications through domain methods
- Event sourcing preparation (not required for MVP)

### 2. Mission Execution (Mission Bounded Context)

#### 2.1 Dice Resolution System
**Priority**: CRITICAL  
**User Story**: As a player, I want dice mechanics handled by AI with optional visibility.

**Acceptance Criteria:**
- Dice calculations in domain service
- Configurable visibility toggle in preferences
- AI narrates results by default
- Optional dice pool breakdown display
- Edge integration points preserved

**Technical Requirements:**
- DicePool value object with modifier aggregation
- IDiceService interface with AI/visible implementations
- Query handlers for dice resolution requests
- Result<DiceOutcome> return pattern

#### 2.2 Edge System Implementation
**Priority**: CRITICAL  
**User Story**: As a player, I want Edge as the primary tactical decision point.

**Acceptance Criteria:**
- Edge aggregate with gain/spend/burn logic
- Persistent Edge display on mobile interface
- Edge decisions presented as narrative choices
- Command handlers for Edge operations
- Domain events for Edge state changes

#### 2.3 Session Management
**Priority**: HIGH  
**User Story**: As a player, I want sessions managed through proper aggregates.

**Acceptance Criteria:**
- Session aggregate root with chat history
- Command/Query separation for session operations
- Mobile-optimized chat interface
- Session persistence with campaign context

### 3. Campaign Management (Campaign Bounded Context)

#### 3.1 Campaign State
**Priority**: MEDIUM  
**User Story**: As a player, I want campaign continuity through domain model.

**Acceptance Criteria:**
- Campaign aggregate with world state
- Contact and faction value objects
- Consequence tracking through events
- Query handlers for campaign data

### 4. Rules & Content (Rules Bounded Context)

#### 4.1 Structured Content Tables
**Priority**: HIGH  
**User Story**: As a system, I need structured tables for AI plugin lookups.

**Acceptance Criteria:**
- game_items table with normalized schema
- magic_abilities table with semantic data
- Equipment entity with domain validation
- Spell entity with Force/drain rules

**Technical Requirements:**
- Import pipeline infrastructure (basic MVP)
- Semantic search via pgvector
- Query handlers for content retrieval
- Integration with Semantic Kernel plugins

#### 4.2 Import Pipeline
**Priority**: DEFER (Architecture only)  
**User Story**: As a system, I need import capability for rulebook accuracy.

**Acceptance Criteria:**
- Pipeline architecture defined
- Interfaces created for future implementation
- Basic markdown import for testing
- Full implementation post-MVP

### 5. AI Integration System

#### 5.1 AI Orchestrator
**Priority**: CRITICAL  
**User Story**: As a system, I need orchestration of specialized AI models.

**Acceptance Criteria:**
- Orchestrator pattern implementation
- Specialized models for different concerns:
  - Rule interpretation model
  - Equipment lookup model
  - Narrative generation model
  - Dice resolution model
- Ollama integration for local RTX 3090
- Semantic Kernel plugin architecture

**Technical Requirements:**
- IAIOrchestrator interface
- Model-specific service implementations
- Plugin registration for Semantic Kernel
- Result pattern for AI operations

### 6. Mobile-First UI/UX

#### 6.1 Responsive Design
**Priority**: CRITICAL  
**User Story**: As a Pixel 8 user, I want optimized mobile experience.

**Acceptance Criteria:**
- MudBlazor responsive components
- Touch-optimized controls (44px minimum)
- Swipe gestures for navigation
- Portrait orientation primary
- Landscape support for character sheet

#### 6.2 Chat Interface
**Priority**: CRITICAL  
**User Story**: As a player, I want mobile-optimized chat with persistent Edge display.

**Acceptance Criteria:**
- Chat bubbles with proper mobile spacing
- Persistent Edge tracker in header/footer
- Quick action buttons for common responses
- Character sheet slide-out panel
- Keyboard-aware layout adjustments

---

## Non-Functional Requirements

### Architecture Standards
- **Domain-Driven Design**: Bounded contexts, aggregates, value objects
- **CQRS Pattern**: Source generator implementation, vertical slices
- **Result Pattern**: No exceptions for control flow
- **Repository Pattern**: Abstract persistence concerns
- **Mobile-First**: All UI decisions prioritize Pixel 8 experience

### Performance Requirements
- **Initial Load**: < 3 seconds on 5G mobile
- **Chat Response**: < 2 seconds for AI narration
- **Local Model Response**: < 1 second via Ollama
- **Dice Resolution**: < 500ms including AI narration

### Technical Constraints
- **.NET 9**: Latest features and performance
- **Blazor WASM**: PWA capabilities for offline
- **PostgreSQL + pgvector**: Semantic search support
- **MudBlazor**: Material Design consistency
- **Ollama**: Local model hosting on RTX 3090
- **Docker**: Development environment consistency

---

## Success Metrics

### Week 1 Deliverables
- Domain model foundation (Character, Edge, DicePool)
- CQRS infrastructure with source generators
- Basic character creation with repository
- Mobile-responsive layout shell

### Week 2 Deliverables
- Chat interface with AI integration
- Edge system with domain logic
- Dice resolution with visibility toggle
- Session aggregate implementation

### Week 3 Deliverables
- AI orchestrator with specialized models
- Structured content tables (game_items, magic_abilities)
- Campaign context basics
- Semantic Kernel plugin integration

### Week 4 Deliverables
- Complete gameplay loop
- Character persistence and retrieval
- Session save/load functionality
- Mobile UI polish for Pixel 8

---

## Development Priorities

### Phase 1: Architectural Foundation (Week 1)
1. **Bounded Contexts**: Define and implement boundaries
2. **Domain Models**: Character, Edge, DicePool with proper patterns
3. **CQRS Setup**: Source generators and handlers
4. **Mobile Shell**: Responsive layout for Pixel 8

### Phase 2: Core Gameplay (Week 2)
1. **Chat System**: Mobile-optimized conversation UI
2. **AI Integration**: Basic Semantic Kernel setup
3. **Edge Mechanics**: Full domain implementation
4. **Dice System**: Configurable visibility with AI narration

### Phase 3: AI & Content (Week 3)
1. **AI Orchestrator**: Multiple model coordination
2. **Content Tables**: game_items and magic_abilities
3. **Ollama Integration**: Local model hosting
4. **Plugin Architecture**: Semantic Kernel plugins

### Phase 4: Polish & Persistence (Week 4)
1. **Campaign Context**: Basic world state tracking
2. **Import Pipeline**: Architecture and interfaces
3. **Mobile Optimization**: Pixel 8 specific enhancements
4. **End-to-end Testing**: Complete gameplay validation

---

## Architectural Decisions

### Domain-Driven Design Rationale
- **Long-term Maintainability**: Proper boundaries prevent coupling
- **Business Logic Encapsulation**: Rules live in domain, not UI
- **Testability**: Domain logic testable without infrastructure
- **Incremental Complexity**: Can add aggregates without refactoring

### CQRS Implementation
- **Source Generators**: Compile-time validation and boilerplate reduction
- **Result Pattern**: Explicit error handling without exceptions
- **Vertical Slices**: Features complete from UI to database
- **Query Optimization**: Read models separate from domain

### Mobile-First Justification
- **Primary Usage Pattern**: Playing on-the-go via Pixel 8
- **Touch Optimization**: Designed for thumb navigation
- **Responsive Priority**: Mobile breakpoints before desktop
- **PWA Capabilities**: Offline play potential

### AI Orchestration Benefits
- **Specialized Models**: Better accuracy for specific tasks
- **Local/Cloud Hybrid**: Ollama for speed, cloud for complexity
- **Plugin Extensibility**: Easy to add new capabilities
- **Cost Optimization**: Local models reduce API costs

---

## Deferred Features (Post-MVP)

### Acknowledged but Deferred
1. **Priority Character Creation**: Point-buy sufficient for personal use
2. **Complex Magic System**: Basic spellcasting only initially
3. **Matrix Subsystem**: Narrative hacking for MVP
4. **Detailed Augmentations**: Simple Essence tracking only
5. **Full Import Pipeline**: Basic markdown import sufficient
6. **Multiplayer Support**: Single user focus
7. **Advanced Combat**: Streamlined resolution initially

### Architectural Preparation
- Interfaces defined for future features
- Domain boundaries support expansion
- Database schema allows migration
- Plugin architecture enables additions

---

## Risk Mitigation

### Technical Risks
- **DDD Complexity**: Mitigated by incremental aggregate implementation
- **Mobile Performance**: Addressed by PWA and lazy loading
- **AI Latency**: Solved by local Ollama models
- **Pattern Overhead**: Justified by long-term maintainability

### Timeline Risks
- **3-4 Week Timeline**: Achievable with focused scope
- **Architecture Time**: Invest early for later velocity
- **Feature Creep**: Strict MVP boundaries with architectural hooks

---

## Constraints & Assumptions

### Technical Constraints
- RTX 3090 available for Ollama hosting
- Google Pixel 8 as primary device
- Docker Desktop for development
- PostgreSQL with pgvector support

### Architectural Constraints
- Domain-Driven Design is mandatory
- CQRS with source generators required
- Result pattern for error handling
- Repository pattern for persistence

### Assumptions
- Single user removes auth complexity
- Personal use allows "good enough" AI accuracy
- Local models provide sufficient performance
- Mobile-first doesn't sacrifice desktop usability