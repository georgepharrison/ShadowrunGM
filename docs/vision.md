# ShadowrunGM – Product Vision & Requirements

## Executive Summary

ShadowrunGM is a personal AI-powered gaming assistant designed to enable solo Shadowrun 6th Edition gameplay without requiring human Game Masters. The application addresses context window limitations and unreliable rule integration found in existing AI tools by providing persistent campaign memory, semantic search of actual rulebooks, and streamlined UI optimized for solo play sessions.

## Primary User & Use Case

**Target User**: Solo tabletop RPG player with limited gaming time due to family commitments
**Core Problem**: Inability to participate in regular gaming groups while maintaining interest in extended Shadowrun campaigns
**Success Criteria**: Seamless solo play sessions with persistent campaign memory, accurate rule integration, and tactical decision points preserved

## Product Philosophy

### AI-Handled Complexity
- **GM AI manages all mechanical calculations** - dice pools, modifiers, success counting
- **AI presents narrative results** and tactical choice points rather than raw numbers
- **Player retains agency over tactical decisions** - particularly Edge spending choices
- **No mechanical UI complexity** - streamlined interface focused on narrative flow

### Personal Use Priority
- **Built for adaptability** - single user can adjust to tool limitations
- **Rapid iteration over polish** - prioritize playable functionality
- **Personal preferences embedded** - no need for broad user accommodation
- **Commercial viability secondary** - intellectual property concerns deferred

## Core User Journey

### Campaign Creation
1. **Global AI Overseer** helps establish campaign parameters, tone, and house rules
2. **Character creation** through streamlined point-buy system with AI guidance
3. **Campaign initialization** with world events, contacts, and initial plot hooks

### Session Play
1. **Mission Hub** serves as persistent home base between runs
2. **Chat-centric gameplay** with AI GM providing narrative and NPC interactions
3. **Edge decision points** integrated into conversation flow with persistent tracking
4. **Character sheet accessibility** via sidebar or overlay without interrupting chat flow
5. **Session conclusion** with automatic plot progression and consequence tracking

### Campaign Progression
1. **Background Plot-Bot** analyzes session outcomes and updates world state
2. **Character advancement** integrated into mission hub workflow
3. **Long-term consequence tracking** through event sourcing architecture

## UI/UX Requirements

### Essential Interface Elements

**Mission Screen (Primary Play Interface)**
- **Chat window** as main interaction channel with GM AI
- **Persistent Edge display** showing current/maximum with spending options
- **Quick-access character sheet** (sidebar or modal overlay)
- **Edge decision integration** - AI pauses for pre/post roll Edge choices
- **Narrative focus** - no dice pool calculations or mechanical detail exposure

**Mission Hub (Campaign Management)**
- **World Events Feed** - ongoing Sixth World developments
- **Contacts & NPCs** - relationship and favor tracking
- **Available Runs** - categorized missions (Plot/Faction/Filler)
- **Character advancement** - karma/nuyen spending interface
- **Mission archive** - searchable history with outcomes

**Character Creation (Streamlined)**
- **Point-buy system** - simpler than Priority method for solo use
- **AI guidance** - build suggestions and rule validation
- **Live character preview** - real-time stat calculation
- **Equipment integration** - catalog-driven gear selection

**Support Screens**
- **Catalog system** - searchable equipment, spells, augmentations
- **Preferences** - AI behavior, narrative tone, house rules
- **Campaign hub** - multiple campaign management

### Mobile-First Considerations
- **Touch-optimized Edge tracking** - large, accessible buttons
- **Swipe navigation** for character sheets and reference materials
- **Chat interface optimization** - mobile keyboard friendly
- **Offline capability** - local storage for character sheets and core rules

## Technical Architecture Integration

### AI Integration Points
- **Semantic Kernel** provides rule lookup and interpretation
- **Import pipeline** processes official rulebooks into searchable embeddings
- **Background processing** maintains world state and plot progression
- **Multiple model orchestration** - specialized AI for different tasks

### Data Persistence
- **PostgreSQL + pgvector** - campaign state and semantic search
- **Event sourcing** - character advancement and world consequence tracking
- **CQRS architecture** - clear separation of read/write operations
- **Result pattern** - robust error handling throughout

### Technical Constraints
- **Local development** - Docker Desktop and Ollama for model hosting
- **Blazor WebAssembly** - single-page application with PWA capabilities
- **MudBlazor components** - Material Design UI consistency
- **Real-time updates** - SignalR for live game state synchronization

## Development Priorities

### Phase 1: Core Playability (Immediate)
- **Basic character creation** - point-buy with AI validation
- **Chat interface** with persistent Edge tracking
- **Simple dice resolution** - AI-handled with Edge decision points
- **Character sheet overlay** - quick reference without navigation

### Phase 2: Campaign Infrastructure (Short-term)
- **Mission hub** with world events and contact management
- **Catalog system** - equipment and spell database
- **Plot progression** - background processing of campaign consequences
- **Character advancement** - karma and nuyen spending

### Phase 3: Enhanced Features (Medium-term)
- **Advanced Edge mechanics** - complex spending options and tactical guidance
- **Magic system integration** - spellcasting with Force selection and drain management
- **Augmentation system** - Essence tracking and cyberware installation
- **Matrix interfaces** - hacking actions with cyberpunk aesthetics

### Phase 4: Polish & Expansion (Long-term)
- **Visual validation** - Playwright integration for UI testing
- **Performance optimization** - caching and response time improvement
- **Advanced AI features** - personalized GM behavior and adaptive difficulty
- **Documentation** - comprehensive rule integration and citation system

## Success Metrics

### Immediate Goals
- **Complete solo session** without context loss or rule lookup failures
- **Edge decisions** feel meaningful and properly integrated
- **Character sheet accessibility** doesn't interrupt gameplay flow
- **Campaign persistence** maintains continuity across sessions

### Long-term Goals
- **Extended campaign play** (10+ sessions) with consistent world state
- **Character advancement** from creation to high-karma veteran
- **Plot complexity** with faction relationships and long-term consequences
- **Rule accuracy** matching tabletop play experience

## Risk Mitigation

### Technical Risks
- **Context window limitations** - solved through persistent storage and semantic retrieval
- **Rule accuracy** - mitigated by embedding official rulebooks and validation systems
- **Performance** - local model hosting reduces latency and cost concerns

### Product Risks
- **Intellectual property** - personal use reduces immediate legal concerns
- **Scope creep** - MVP focus with clear feature prioritization
- **Time constraints** - agent-driven development to maximize limited development time

## Future Considerations

### Commercial Viability
- **Legal consultation** required for any market release
- **Generic system adaptation** - move away from Shadowrun IP for broader appeal
- **Licensing negotiation** - explore official partnership opportunities

### Technical Evolution
- **Multi-system support** - extend to other RPG systems
- **Multiplayer capability** - shared campaign and GM functionality
- **Cloud deployment** - scalable infrastructure for broader user base
- **Advanced AI integration** - voice interaction and real-time world generation

---

*This vision document prioritizes personal use and rapid prototyping for solo Shadowrun gameplay. Commercial considerations and comprehensive feature sets are deferred in favor of achieving playable functionality quickly.*