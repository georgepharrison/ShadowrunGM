# Solution Structure

This document describes the structure and responsibilities of each project in the **ShadowrunGM** solution. It provides more detail than the high-level [Architecture Overview](./architecture_overview.md).

---

## Project Layout

```
ShadowrunGM/
├── ShadowrunGM.UI/             # Blazor WebAssembly frontend
│   ├── Pages/                  # Razor pages (Index, Campaigns, Catalog, Rules, Settings)
│   ├── Components/             # Shared UI components (ChatPanel, Drawers, Overlays)
│   ├── Shared/                 # Layouts, NavMenu, MainLayout
│   └── wwwroot/                # Static assets (JS interop, CSS, service worker)
│
├── ShadowrunGM.API/            # ASP.NET Core Web API backend
│   ├── Controllers/            # REST endpoints
│   ├── Application/            # CQRS layer: Commands, Queries, MediatR handlers
│   ├── Domain/                 # DDD layer: Aggregates, Entities, Value Objects, Events
│   ├── Infrastructure/         # EF Core DbContext, persistence, repositories, integrations
│   └── SemanticKernel/         # Plugins, connectors, AI orchestration
│
├── ShadowrunGM.Import/         # Data import and parsing pipeline
│   ├── Services/               # OCR, PDF parsing, data extraction logic
│   └── Storage/                # Temporary store for parsed/validated items
│
├── ShadowrunGM.Tests/          # Testing suite
│   ├── Unit/                   # Unit tests (isolated logic)
│   └── Integration/            # Integration tests (API, database, plugins)
│
└── docs/                       # Documentation
    ├── architecture_overview.md
    ├── solution_structure.md
    ├── semantic_kernel_plugins.md
    └── ai_models.md
```

---

## Responsibilities

### ShadowrunGM.UI

* Primary user interface.
* Built with **Blazor WebAssembly** + **MudBlazor**.
* Provides campaign management, rule lookups, character builder, and chat-driven GM/AI interactions.
* Implements responsive design for desktop and mobile.

### ShadowrunGM.API

* Core backend service.
* Implements **Clean Architecture** layering (Domain, Application, Infrastructure).
* Provides REST API endpoints consumed by UI.
* Hosts Semantic Kernel plugins so both UI and AI assistants can share logic.

### ShadowrunGM.Import

* Handles importing PDF rulebooks.
* Extracts catalog items (gear, spells, augments, powers, etc.) into structured data.
* Provides a validation layer before persistence.

### ShadowrunGM.Tests

* Unit tests validate domain rules and CQRS handlers.
* Integration tests validate infrastructure (repositories, database interactions).

### docs/

* Project documentation.
* Organized into focused markdown files for architecture, plugins, AI models, and contribution guidelines.

---

## Development Notes

* **CQRS** ensures separation between write (commands) and read (queries).
* **DDD** enforces domain rules inside Aggregates and Value Objects.
* **Semantic Kernel Plugins** expose domain operations for both UI and AI.
* **Import pipeline** ensures that adding new rulebooks is streamlined and system-wide.

---

For details on plugins see [`semantic_kernel_plugins.md`](./semantic_kernel_plugins.md). For AI models see [`ai_models.md`](./ai_models.md).
