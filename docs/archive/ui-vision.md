# ShadowrunGM – UI/UX Product Requirements (For LLM Agents)

This document describes the planned **user interface and experience** for the ShadowrunGM application.  
The target audience is LLM agents (e.g., Claude Code, ChatGPT, other code-assistants) that will implement features in parallel.  
Each section includes elaboration on purpose, components, and desired UX.

---

## Home Page
- **Purpose**: Landing screen and primary navigation point.  
- **Features**:
  - Hero section with app branding and tagline.  
  - Quick-action button: **Start New Campaign**.  
  - List of active campaigns (cards or tiles) with **Continue** button.  
  - Access to Preferences and Catalog.  
- **UX Design**:
  - Clean, inviting hero display.  
  - One-click resume of ongoing campaigns.  
  - Global nav bar to Preferences, Catalog, Campaign Hub.

---

## Character Creation Screen
- **Approach**: MVP only, **point-based creation system** (no multi-step wizard).  
- **User Flow**:
  - Choose **metatype** (human, elf, ork, troll, dwarf).  
  - Assign **attribute points** (strength, body, agility, logic, etc.).  
  - Select **skills** from available pool.  
  - Pick **qualities** (positive/negative).  
  - Add **equipment, spells, or powers**.  
- **AI Overseer Integration**:
  - Suggests optimized builds.  
  - Warns if rules are broken (overspending points, exceeding limits).  
  - Offers tips (e.g., “Street Samurai typically invest more in agility and reflexes”).  
- **UI Goals**:
  - Minimal clutter, tabbed or collapsible sections.  
  - Inline validation of point totals.  
  - Character sheet preview pane updates live.

---

## Preferences Screen
- **Purpose**: Global user configuration.  
- **Features**:
  - Rating sliders (violence, grit, noir vs. heroic tone).  
  - Toggle for **AI Overseer activity level** (passive suggestions vs. active coaching).  
  - Session defaults (metric vs. imperial, ¥ vs. credits).  
  - AI GM behavior defaults: narrative density, decision strictness, risk level.  
- **UX Considerations**:
  - Group settings by theme: Narrative, Mechanics, Accessibility.  
  - Clear descriptions + example tooltips (“Low grit = pulp adventure; High grit = lethal realism”).  

---

## Catalog Pages
- **Scope**: Central library of **equipment, spells, powers, augmentations, qualities, skills, etc.** available in the Sixth World.  
- **Features**:
  - Filterable + searchable by category (weapons, armor, magic, cyberware).  
  - Detailed pages: stats, description, cost, availability, sourcebook reference.  
  - Optional **AI recommendations** (e.g., “This item synergizes with your chosen skillset”).  
- **UX Design**:
  - Grid/list view switcher.  
  - Hover or expand for detail.  
  - Breadcrumb navigation back to category.  

---

## Campaign Hub
- **Purpose**: High-level overview of active and potential campaigns.  
- **Features**:
  - Create new campaign.  
  - Join or resume existing campaign.  
  - View campaign metadata (name, description, house rules).  
  - Set per-campaign GM AI preferences (overrides global defaults).  
- **UI Notes**:
  - Dashboard layout: campaigns as cards.  
  - Quick “Continue” button for active campaign.  

---

## Mission Hub
- **Purpose**: Player staging ground after selecting a campaign.  
- **Content**:
  - **World Events Feed**: ongoing Sixth World events that shape gameplay.  
  - **GMPCs (Game Master PCs)**: key NPCs controlled by the GM AI.  
  - **Contacts**: allies, fixers, Mr. Johnsons.  
  - **Runs Offered**: categorized as Plot, Faction, or Filler.  
  - **Character Management**: view/advance character sheet, spend karma/nuyen.  
  - **Past Missions**: archive of previous runs with outcomes.  
- **UX Flow**:
  - Mission hub is **persistent home base**.  
  - Clicking a run launches Mission Screen.  
  - On return: debrief screen shows rewards + outcomes.

---

## Mission Screen
- **Primary Element**: **Chat window** as main interaction channel.  
- **GM AI Role**:
  - Provides narrative + NPC dialog.  
  - Offers structured **options/choices** (if enabled in settings).  
  - Responds to freeform input when players improvise.  
- **UI Elements**:
  - Chat log with timestamped GM/Player lines.  
  - Quick-action buttons for common actions (attack, negotiate, hack, spellcasting).  
  - Sidebar:
    - Character sheet (always accessible).  
    - Current objectives.  
    - Active conditions/status effects.  
- **End of Mission**:
  - Automatic debrief popup:
    - Karma gained.  
    - Currency earned.  
    - Loot acquired.  
    - Consequential events (grudges, alliances, faction standing).  
  - Player is returned to **Mission Hub**.  
  - Plot-Bot in background updates future events (non-UI).  

---

## Past Missions Screen
- **Purpose**: Historical record of completed missions.  
- **Content**:
  - Mission title, date, participants.  
  - GM summary of events.  
  - Player choices that mattered.  
  - Rewards gained.  
  - Any **Event Sourcing entries** (faction changes, new grudges).  
- **UI Design**:
  - Table/list of missions with expand for details.  
  - Search + filter (by character, campaign, faction).  

---

# Notes for LLM Implementation
- Each section can be built as a **Blazor component** in the ShadowrunGM UI.  
- Data flows should follow **CQRS vertical slice pattern**.  
- All user actions (create character, select mission, update preferences) should map to a **Command** or **Query** in the backend API.  
- UI must remain **MVP-focused** but extensible. Agents should code for modularity.  
