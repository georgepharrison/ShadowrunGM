# ShadowrunGM Documentation Hub

*Complete documentation for the ShadowrunGM project - a Blazor WebAssembly and ASP.NET Core application following Domain-Driven Design principles with CQRS architecture.*

## 📋 Project Planning & Management

### Core Planning Documents
- **[📋 TASKS.md](TASKS.md)** - Current development tasks with completion tracking (3-4 week architecture-first MVP)
- **[📝 PRD.md](PRD.md)** - Product Requirements Document with functional specs and success criteria
- **[🏗️ PLANNING.md](PLANNING.md)** - Technical Planning Document with DDD architecture and implementation details
- **[⚙️ CLAUDE.md](../CLAUDE.md)** - Development workflow, coding standards, and Claude Code integration patterns

### Development Workflow
The project follows a standardized development workflow documented in [CLAUDE.md](../CLAUDE.md):
1. **Analyze First**: Use `@codebase-analyzer` to understand existing patterns
2. **Test-Driven Development**: Use specialized agents for tests-first approach
3. **Mark Tasks Complete**: Update task checkboxes in [TASKS.md](TASKS.md)  
4. **Auto-Documentation**: Run `@documentation-agent` for docs and commits

### Agent Specialists
- **[🤖 Agent Catalog](AGENT_CATALOG.md)** - Complete guide to all specialized development agents
- **[Development Agents](.claude/agents/)** - Individual agent definitions for TDD, DDD, UI, and architecture

## 🏗️ Architecture & Engineering

### Domain-Driven Design
- **[🎯 Domain Architecture](architecture/domain-architecture.md)** - Bounded contexts and aggregate design
- **[👤 Character Aggregate](domain/character-aggregate.md)** - Character entity with DDD patterns
- **[🎲 Mission Aggregate](domain/mission-aggregate.md)** - Game session and dice resolution
- **[📚 Shadowrun Glossary](domain/shadowrun-glossary.md)** - Business terminology and domain concepts

### Technical Architecture  
- **[📊 Architecture Overview](engineering/architecture_overview.md)** - High-level system architecture
- **[🏗️ Solution Structure](engineering/solution_structure.md)** - Project organization and bounded contexts
- **[🤖 AI Models](engineering/ai_models.md)** - Semantic Kernel integration and AI orchestration
- **[🔌 Semantic Kernel Plugins](engineering/semantic_kernel_plugins.md)** - AI plugin architecture

### Implementation Guides
- **[💾 Database Setup](database-setup.md)** - PostgreSQL with pgvector configuration
- **[📊 Data Seeding Strategy](engineering/data-seeding-strategy.md)** - Structured content import for AI
- **[👤 Character Repository](engineering/character-repository-implementation.md)** - DDD repository patterns

### Layer Documentation
- **[🎯 Domain Layer README](../src/API/Domain/README.md)** - Domain model and business logic
- **[⚡ Application Layer README](../src/API/Application/README.md)** - CQRS commands and queries  
- **[🔧 Infrastructure Layer README](../src/API/Infrastructure/README.md)** - Data persistence and external services
- **[🌱 Data Seeders README](../src/API/Infrastructure/Seeders/README.md)** - Shadowrun 6e content seeding

## 🎯 Context & Vision

### Project Context
- **[🎯 Vision](vision.md)** - Product vision and long-term goals
- **[🗺️ Context Map](context-map.md)** - Bounded context relationships
- **[🎮 Campaign Play](contexts/campaign-play.md)** - Gameplay session context
- **[👤 Character Builder](contexts/character-builder.md)** - Character creation context  
- **[📥 Data Import](contexts/data-import.md)** - Content import and processing context

## 📚 Reference Documentation

### Shadowrun Rules Integration
- **[📖 SR6 Berlin Edition](rules/CAT28000B_SR6_Berlin_Edition.md)** - Core rulebook integration
- **[🎯 Firing Squad](rules/CAT28002_Firing_Squad.md)** - Combat and action rules
- **[🪄 Street Wyrd](rules/CAT28003_Street_Wyrd.md)** - Magic system rules
- **[🌍 Sixth World Companion](rules/CAT28005_Sixth_World_Companion.md)** - Expanded game content

### Documentation Analysis
- **[📋 Documentation Audit](documentation-audit.md)** - Coverage analysis and XML documentation status

## 🗃️ Research Archive

*Historical research and analysis that informed current project decisions*

### User Experience Research
- **[📱 UX Research](archive/ux-research.md)** - Mobile-first design research and user needs analysis
- **[🎨 UI Vision](archive/ui-vision.md)** - Original interface design concepts
- **[🔍 UI Vision Analysis](archive/ui-vision-analysis.md)** - Evaluation of design approaches
- **[📊 Shadowrun 6e UX Analysis](archive/shadowrun-6e-ux-analysis.md)** - Analysis of existing tools and pain points

### Technical Research
- **[⚙️ Tech Stack Analysis](archive/tech-stack-analysis.md)** - Technology selection rationale
- **[🗺️ Context Map (Archive)](archive/context-map.md)** - Early bounded context exploration
- **[📚 Glossary (Archive)](archive/glossary.md)** - Initial domain terminology research
- **[🛣️ Roadmap (Archive)](archive/roadmap.md)** - Original project roadmap and timelines

*The archive demonstrates the research and iteration that led to the current [PLANNING.md](PLANNING.md), [PRD.md](PRD.md), and [TASKS.md](TASKS.md).*

## 🚀 Getting Started

### For Developers
1. **Start with [CLAUDE.md](../CLAUDE.md)** - Essential development workflow and standards
2. **Review [TASKS.md](TASKS.md)** - Current development status and next priorities  
3. **Explore [Agent Catalog](AGENT_CATALOG.md)** - Specialized development assistants
4. **Check [Architecture Overview](engineering/architecture_overview.md)** - System design principles

### For Contributors  
1. **Read [PRD.md](PRD.md)** - Product requirements and success criteria
2. **Understand [PLANNING.md](PLANNING.md)** - Technical architecture and domain design
3. **Follow [Database Setup](database-setup.md)** - Local development environment
4. **Browse [Domain Documentation](domain/)** - Business logic and domain concepts

### For Architects
1. **Study [Domain Architecture](architecture/domain-architecture.md)** - DDD implementation patterns
2. **Review [Archive Research](archive/)** - Decision history and trade-offs
3. **Examine [Solution Structure](engineering/solution_structure.md)** - Bounded context organization
4. **Analyze [Semantic Kernel Integration](engineering/semantic_kernel_plugins.md)** - AI architecture patterns

---

## 📖 Navigation Tips

- **GitHub First**: All documentation is optimized for GitHub browsing with clear hierarchies
- **Cross-Referenced**: Related documents are linked for easy discovery  
- **Progress Tracking**: Check [TASKS.md](TASKS.md) for current development status
- **Standards Compliance**: All code follows patterns defined in [CLAUDE.md](../CLAUDE.md)
- **Agent Assistance**: Use [specialized agents](AGENT_CATALOG.md) for development tasks

*For the most current information, always refer to [TASKS.md](TASKS.md) for development status and [CLAUDE.md](../CLAUDE.md) for standards.*
