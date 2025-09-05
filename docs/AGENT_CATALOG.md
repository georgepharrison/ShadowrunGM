# Agent Catalog

This document provides a comprehensive overview of all specialized agents available in the ShadowrunGM project. Each agent is a specialist focused on specific development tasks, architectural patterns, or technical domains.

## Table of Contents

- [Development Workflow Agents](#development-workflow-agents)
- [Testing Specialists](#testing-specialists)
- [Architecture and Design Agents](#architecture-and-design-agents)
- [Domain-Driven Design Specialists](#domain-driven-design-specialists)
- [UI and Frontend Specialists](#ui-and-frontend-specialists)
- [AI and Semantic Kernel Specialists](#ai-and-semantic-kernel-specialists)
- [Infrastructure and Database Specialists](#infrastructure-and-database-specialists)
- [Quality Assurance Agents](#quality-assurance-agents)

---

## Development Workflow Agents

### 🔍 Codebase Analyzer
**File**: [`.claude/agents/codebase-analyzer.md`](.claude/agents/codebase-analyzer.md)

**Purpose**: Analyzes existing codebase patterns, architectural decisions, and conventions before implementing new features.

**Key Capabilities**:
- Examines solution structure and project organization
- Identifies architectural patterns (CQRS, DDD, layered architecture)
- Maps dependency flows and project references
- Documents existing domain models, entities, and value objects
- Analyzes data access patterns and persistence strategies
- Provides implementation guidance aligned with existing patterns

**When to Use**:
- Before implementing any new feature to understand existing patterns
- When extending domain models or CQRS handlers
- When you need to understand how existing code is structured
- Before making significant architectural changes

**Example Usage**:
```
I need to implement equipment management for characters. Can you help me understand how to structure this?
→ Use @codebase-analyzer to examine existing patterns and architecture first
```

### 📝 Documentation Agent
**File**: [`.claude/agents/documentation-agent.md`](.claude/agents/documentation-agent.md)

**Purpose**: Maintains comprehensive documentation including XML docs, README files, and architectural diagrams.

**Key Capabilities**:
- Scans C# files for missing XML documentation
- Generates domain-driven design documentation with Mermaid diagrams
- Creates and maintains README.md files for directories
- Provides documentation coverage auditing
- Generates C4-style architectural diagrams
- Maintains domain glossaries with Shadowrun terminology

**When to Use**:
- After creating or modifying C# classes
- When implementing new domain aggregates or value objects
- Before committing code to ensure documentation standards
- When documentation coverage falls below standards
- For onboarding new team members

**Commands**:
- `doc:audit` - XML documentation coverage analysis
- `doc:update` - Complete documentation refresh
- `doc:generate-aggregates` - DDD aggregate documentation

---

## Testing Specialists

### 🔴 TDD Test Writer
**File**: [`.claude/agents/tdd-test-writer.md`](.claude/agents/tdd-test-writer.md)

**Purpose**: Creates comprehensive failing unit tests following strict Test-Driven Development practices.

**Key Capabilities**:
- Writes failing tests that define expected behavior
- Uses xUnit and Shouldly for expressive assertions
- Creates Builder patterns for complex test data
- Follows Arrange-Act-Assert structure with single assertions
- Implements Mother Object patterns for common scenarios
- Tests domain invariants and business rules

**When to Use**:
- Before implementing any new functionality (Red phase of TDD)
- When refactoring existing code
- When adding validation rules or business logic
- Creating new domain aggregates or value objects

**Example Usage**:
```
I need to add a validation rule that ensures character names are between 3 and 50 characters
→ Use @tdd-test-writer to create failing tests that define this validation behavior first
```

### 🟢 TDD Implementation Specialist
**File**: [`.claude/agents/tdd-implementation-specialist.md`](.claude/agents/tdd-implementation-specialist.md)

**Purpose**: Implements minimal code to make failing tests pass while following established project patterns.

**Key Capabilities**:
- Makes failing tests pass with minimal implementation
- Follows Red-Green-Refactor cycle
- Uses Result<T> pattern from FlowRight.Core.Results
- Implements ValidationBuilder patterns for complex validation
- Adheres to all project coding standards (no `var`, explicit types)
- Applies SOLID principles incrementally

**When to Use**:
- After tests are written and failing (Green phase of TDD)
- When implementing features with established test requirements
- When refactoring code while maintaining test coverage

**Example Usage**:
```
I've written tests for the CharacterService.CreateCharacter method but they're all failing
→ Use @tdd-implementation-specialist to create the minimal implementation
```

### 🟡 Integration Test Specialist
**File**: [`.claude/agents/integration-test-specialist.md`](.claude/agents/integration-test-specialist.md)

**Purpose**: Creates comprehensive integration, component, and end-to-end tests across all application layers.

**Key Capabilities**:
- API integration tests with WebApplicationFactory and real databases
- Blazor component tests with bUnit framework
- Playwright end-to-end tests for full user workflows
- Mobile-first testing with 375px viewport verification
- Touch target validation (minimum 44px)
- Cross-browser compatibility testing

**When to Use**:
- After implementing features that span multiple layers
- When creating mobile-responsive UI components
- For validating complete user workflows
- When testing API endpoints with database interactions

**Example Usage**:
```
I've finished implementing the character creation feature. Can you help me create comprehensive tests?
→ Use @integration-test-specialist for full test suite coverage
```

---

## Architecture and Design Agents

### 🔵 Refactoring Specialist
**File**: [`.claude/agents/refactoring-specialist.md`](.claude/agents/refactoring-specialist.md)

**Purpose**: Transforms working code into well-designed implementations following SOLID principles and design patterns.

**Key Capabilities**:
- Identifies code smells and SOLID principle violations
- Applies appropriate design patterns (Strategy, Factory, Observer)
- Extracts repeated logic into reusable components
- Implements Shadowrun-specific domain patterns
- Maintains green test state during all refactoring
- Improves cyclomatic complexity and maintainability

**When to Use**:
- After implementing features with all tests passing
- When code has duplicated validation logic
- When complex conditionals could benefit from polymorphism
- Before code review to improve design quality

**Example Usage**:
```
I've got character creation working but I'm repeating validation patterns in multiple places
→ Use @refactoring-specialist to extract reusable validation components
```

### 🟠 PR Review Agent
**File**: [`.claude/agents/pr-review-agent.md`](.claude/agents/pr-review-agent.md)

**Purpose**: Reviews code changes for compliance with architectural patterns, SOLID principles, and project standards.

**Key Capabilities**:
- Validates Result<T> pattern usage and FlowRight integration
- Enforces SOLID principles and DDD patterns
- Checks bounded context separation and aggregate design
- Reviews mobile-first UI standards and accessibility
- Provides structured feedback with severity levels
- Ensures cyclomatic complexity and method size standards

**When to Use**:
- Before submitting pull requests
- After completing feature implementation
- When making changes to domain models or CQRS handlers
- For ensuring code quality and architectural compliance

**Review Format**:
- ✅ Good Patterns - Exemplary code following standards
- ⚠️ Suggestions - Improvements for maintainability  
- 🔴 Must Fix - Critical violations requiring correction
- 💡 Refactoring Opportunities - Strategic improvements

---

## Domain-Driven Design Specialists

### 🟢 Aggregate Builder
**File**: [`.claude/agents/aggregate-builder.md`](.claude/agents/aggregate-builder.md)

**Purpose**: Designs and implements DDD aggregates with proper encapsulation and business rule protection.

**Key Capabilities**:
- Creates always-valid aggregates with protected invariants
- Implements private constructors with static factory methods
- Uses ValidationBuilder for complex multi-field validation
- Manages child entities through aggregate root methods
- Raises domain events for significant state changes
- Integrates value objects with proper composition

**When to Use**:
- Creating new domain entities with business rule enforcement
- Refactoring anemic domain models into rich aggregates
- Implementing complex business logic with multiple invariants
- Designing aggregates for Shadowrun game mechanics

**Example Usage**:
```
I need to create a Mission aggregate that tracks objectives and ensures they can only be completed in order
→ Use @aggregate-builder to design proper invariant protection and sequential completion rules
```

### 🩷 Domain Boundary Guardian
**File**: [`.claude/agents/domain-boundary-guardian.md`](.claude/agents/domain-boundary-guardian.md)

**Purpose**: Enforces proper Domain-Driven Design principles and bounded context integrity.

**Key Capabilities**:
- Validates bounded context separation and prevents cross-context violations
- Reviews aggregate design for proper encapsulation
- Ensures communication through domain events instead of direct calls
- Identifies shared entities that violate DDD principles
- Validates value object immutability and self-validation
- Provides specific refactoring guidance with code examples

**When to Use**:
- When implementing features that interact across contexts
- Before creating shared entities or services
- When reviewing aggregate designs and relationships
- During architectural reviews of domain model changes

**ShadowrunGM Contexts**:
- **Character Context**: Character lifecycle and augmentations
- **Mission Context**: Gameplay sessions and dice rolls
- **Campaign Context**: Persistent world state and reputation
- **Rules Context**: Read-only reference data

---

## UI and Frontend Specialists

### 🟣 Blazor UI Specialist
**File**: [`.claude/agents/blazor-ui-specialist.md`](.claude/agents/blazor-ui-specialist.md)

**Purpose**: Creates mobile-first, responsive Blazor WebAssembly components using MudBlazor with OLED-friendly design.

**Key Capabilities**:
- Mobile-first design for 375px viewport (Google Pixel 8)
- OLED-optimized color scheme with concrete hex values
- Touch target optimization (minimum 44px)
- MudBlazor component implementation with proper sizing
- Responsive navigation with bottom-positioned controls
- PWA integration with offline capabilities

**Design Standards**:
- Background: #0a0a0a (OLED black)
- Primary: #00ff41 (Matrix green)  
- Secondary: #ff6b00 (Edge orange)
- Surface: #1a1a1a (elevated cards)

**When to Use**:
- Creating character sheets and game interface components
- Implementing mobile-responsive navigation
- Building chat interfaces and real-time interactions
- Optimizing existing UI components for mobile devices

**Example Usage**:
```
I need to create a character sheet component that displays attributes in a mobile-friendly layout
→ Use @blazor-ui-specialist for mobile-first responsive design with proper touch targets
```

---

## AI and Semantic Kernel Specialists

### 🟡 Semantic Kernel Specialist
**File**: [`.claude/agents/semantic-kernel-specialist.md`](.claude/agents/semantic-kernel-specialist.md)

**Purpose**: Integrates AI orchestration capabilities, develops Semantic Kernel plugins, and implements semantic search.

**Key Capabilities**:
- Designs Semantic Kernel plugins for game mechanics
- Configures OpenAI and local Ollama models for RTX 3090
- Implements pgvector-based semantic search
- Creates AI orchestrators for game master assistance
- Optimizes embedding strategies and similarity search
- Integrates with existing CQRS and Result<T> patterns

**AI Model Integration**:
- OpenAI API configuration and key management
- Local Ollama model deployment and optimization
- Model switching and fallback strategies
- Performance optimization for real-time gameplay

**When to Use**:
- Implementing AI-powered dice rolling suggestions
- Creating semantic search for equipment and rules
- Building game master orchestration features
- Integrating local AI models for offline play

**Example Usage**:
```
I need to create a plugin that suggests when players should use Edge based on dice pools
→ Use @semantic-kernel-specialist to design an Edge suggestion plugin
```

---

## Infrastructure and Database Specialists

### 🔴 Migration Planner
**File**: [`.claude/agents/migration-planner.md`](.claude/agents/migration-planner.md)

**Purpose**: Plans, creates, and validates Entity Framework Core migrations for PostgreSQL with pgvector support.

**Key Capabilities**:
- Safe migration planning with rollback strategies
- PostgreSQL-specific features (snake_case, pgvector, JSONB)
- Vector index management and optimization
- Data preservation during schema changes
- Migration testing and validation procedures
- Proper constraint definitions and foreign keys

**PostgreSQL Expertise**:
- pgvector extension management
- GIN/GiST index optimization for JSONB
- Snake_case naming conventions
- Vector similarity operations (cosine, L2)

**When to Use**:
- Adding new tables with vector embedding support
- Modifying existing schema with data preservation
- Creating indexes for performance optimization
- Planning complex schema migrations

**Example Usage**:
```
I need to add a table for storing Shadowrun equipment with vector embeddings for semantic search
→ Use @migration-planner to design proper table structure with pgvector support
```

---

## Quality Assurance Agents

### 🔵 CQRS Minimal API Specialist
**File**: [`.claude/agents/cqrs-minimal-api-specialist.md`](.claude/agents/cqrs-minimal-api-specialist.md)

**Purpose**: Creates CQRS commands, queries, and minimal API endpoints following vertical slice architecture.

**Key Capabilities**:
- Designs commands and queries tailored to UI needs
- Implements minimal API endpoints without MediatR
- Creates response DTOs that eliminate over/under-fetching
- Integrates FluentValidation with proper error handling
- Uses direct EF Core projections for performance
- Follows vertical slice pattern with feature-complete implementations

**Architectural Patterns**:
- Commands express business intent (not CRUD)
- Queries return specific Response DTOs
- Endpoint methods serve as handlers
- Single file per bounded context feature

**When to Use**:
- Creating new API endpoints for domain features
- Implementing command/query handlers
- Designing response DTOs for specific UI components
- Refactoring controllers to minimal API patterns

**Example Usage**:
```
I need to add equipment management endpoints - create, update, and list operations
→ Use @cqrs-minimal-api-specialist for proper CQRS patterns and vertical slicing
```

---

## How to Use This Catalog

### Choosing the Right Agent

1. **Start with Analysis**: Use `@codebase-analyzer` before implementing new features
2. **Follow TDD Workflow**: `@tdd-test-writer` → `@tdd-implementation-specialist` → `@refactoring-specialist`
3. **Domain Focus**: Use `@aggregate-builder` and `@domain-boundary-guardian` for domain logic
4. **UI Development**: Use `@blazor-ui-specialist` for mobile-first component development
5. **Quality Assurance**: Use `@pr-review-agent` before submitting code

### Integration with Standard Workflow

The standard development workflow from [CLAUDE.md](CLAUDE.md) automatically coordinates these agents:

```bash
# Step 1: Understand existing patterns (automatic)
@codebase-analyzer I need to implement [feature]. Please analyze existing patterns.

# Step 2: TDD Implementation (agents coordinate automatically)
@tdd-test-writer → @tdd-implementation-specialist

# Step 3: Mark tasks complete in TASKS.md (CRITICAL STEP)
# Update [ ] to [x] for completed tasks

# Step 4: Documentation and commit (automatic)  
@documentation-agent Please analyze changes and update documentation
```

### Agent Coordination

Agents are designed to work together seamlessly:
- **Analysis → Design → Implementation → Testing → Documentation**
- **Quality gates at each stage with proper agent handoffs**
- **Consistent Result<T> pattern and architectural standards across all agents**

For the most current agent definitions and capabilities, always refer to the individual agent files in [`.claude/agents/`](.claude/agents/).