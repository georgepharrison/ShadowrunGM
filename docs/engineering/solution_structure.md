# Solution Structure

This document describes the structure and responsibilities of each project in the **ShadowrunGM** solution. It reflects the decision to keep the **Import pipeline inside the API project** (no separate `ShadowrunGM.Import` project for MVP).

---

## Project Layout (MVP)

```
ShadowrunGM/
├── ShadowrunGM.UI/                 # Blazor WebAssembly frontend (MudBlazor)
│   ├── Components/                 # Shared UI components (ChatPanel, Drawers, Overlays)
│   ├── Layout/                     # Layouts, NavMenu, MainLayout
│   ├── Models/                     # Shared models (DTOs, ViewModels)
│   ├── Pages/                      # Razor pages (Index, Campaigns, Catalog, Rules, Settings)
│   └── wwwroot/                    # Static assets (JS interop, CSS, service worker)
│
├── ShadowrunGM.API/                # ASP.NET Core Web API backend
│   ├── Application/                # CQRS layer: Commands, Queries, FlowRight.Cqrs.Http handlers
│   │   └── Import/                 # Import commands/queries/DTOs (PdfPig pipeline)
│   ├── Domain/                     # DDD layer: Aggregates, Entities, Value Objects, Events
│   ├── Endpoints/                  # REST endpoints (incl. ImportController)
│   ├── Infrastructure/             # EF Core DbContext, persistence, repositories, integrations
│   │   ├── Configurations/         # EF Core entity configurations
│   │   ├── Import/                 # Staging schemas, services, storage (Import infra)
│   │   └── Migrations/             # EF Core migrations (Postgres), incl. import tables
│   ├── SemanticKernel/             # Plugins, connectors, AI orchestration
│   └── Workers/                    # BackgroundService(s) (e.g., Import, World Evolution)
│
├── ShadowrunGM.Tests/              # Unit + Integration tests
│   ├── Unit/                       # Unit tests (domain, application)
│   └── Integration/                # Integration tests (API, DB, plugins)
│
└── docs/                           # Documentation
    ├── engineering/
    │   ├── architecture_overview.md
    │   ├── solution_structure.md
    │   ├── semantic_kernel_plugins.md
    │   └── ai_models.md
    └── ...
```

---

## Responsibilities

### ShadowrunGM.UI

* **Purpose:** Primary user interface (mobile-first, chat-centric).
* **Key Areas:**

  * Campaign hub and GM chat (in-campaign persona only)
  * Character Builder via Overseer chat (outside campaigns)
  * PDF Import pages: **Upload → Review → Commit** (admin-audited)
* **Notes:** Thin UI—calls API endpoints; uses SSE for progress in import/evolution.

### ShadowrunGM.API

* **Purpose:** Application core (CQRS, DDD, persistence, AI plugins).
* **Key Areas:**

  * `Application/` → Commands/Queries/Handlers (incl. `Application/Import/*`)
  * `Domain/` → Aggregates, Entities, Value Objects, domain events
  * `Endpoints/` → REST endpoints (including `ImportController`)
  * `Infrastructure/` → EF Core DbContext, repositories, Postgres migrations

    * `Infrastructure/Import/` → import staging models & services
    * `Infrastructure/Migrations/` → **all migrations live here**, including import tables
  * `SemanticKernel/` → plugin surfaces for UI/AI (Catalog, Campaigns, Rules, Settings, Import)
  * `Workers/` → `BackgroundService` implementations (e.g., import parsing, world evolution)
* **Notes:** Import is **module-internal** to API for MVP; can be extracted later if needed.

### ShadowrunGM.Tests

* **Purpose:** Quality gates for domain and infrastructure.
* **Key Areas:**

  * `Unit/` → deterministic tests (builders, validation, rules)
  * `Integration/` → API + DB + plugin round-trips (including import staging → commit)

### docs/

* **Purpose:** Living documentation (domain, engineering, AI).
* **Key Entry Points:** `engineering/architecture_overview.md`, this file, plugins, models.

---

## Import Module Inside API (Rationale)

* **Simplicity for MVP:** one deployable, one DI container, fewer csproj files.
* **UI Integration:** upload & review pages live in UI, calling API Import endpoints.
* **Migrations in one place:** Postgres tables for both gameplay and import staging are versioned under `Infrastructure/Migrations/`.
* **Escape Hatch Later:** if OCR/throughput grows heavy, the `Application/Import` and `Infrastructure/Import` folders can be promoted into a separate worker service without breaking UI contracts.

---

## Development Notes

* **CQRS:** commands for write paths, queries for read models.
* **DDD:** domain invariants enforced in aggregates; builder allows temporary invalid states until finalize.
* **EF Core:** use Postgres; keep migrations under `Infrastructure/Migrations/`.
* **SSE:** server-sent events for import progress and world evolution notifications.
* **Auth:** gate import endpoints & pages behind an Admin policy.

---

## Common Commands (EF Core)

```bash
# From solution root (ensure API is startup project in VS/CLI)

# Add a migration (e.g., initial import staging)
dotnet ef migrations add InitImportSchema \
  --project ShadowrunGM.API \
  --output-dir Infrastructure/Migrations

# Apply migrations to Postgres
dotnet ef database update --project ShadowrunGM.API
```

---

## Future Split (Optional)

If/when needed, extract:

* `Application/Import` → new project `ShadowrunGM.Import` (Worker)
* `Infrastructure/Import` + related migrations → move to worker (or keep shared migrations in API if sharing DB)
* Replace in-process calls with queue/HTTP between UI/API and the worker.
