# Architecture Overview

This document outlines the planned architecture for the **ShadowrunGM** project. It complements `Remaining Tasks.md` and is split into sub-documents (`solution_structure.md`, `semantic_kernel_plugins.md`, and `ai_models.md`).

For quick navigation from the root project, see the links below:

* [Solution Structure](solution_structure.md)
* [Semantic Kernel Plugins](semantic_kernel_plugins.md)
* [AI Models](ai_models.md)

---

## Solution & Project Structure

```
ShadowrunGM/
├── ShadowrunGM.UI/             # Blazor WebAssembly (MudBlazor UI)
│   ├── Pages/                  # Razor pages (Index, Campaigns, Catalog, Rules, Settings)
│   ├── Components/             # Shared UI components (ChatPanel, Drawers, etc.)
│   ├── Shared/                 # Layouts, NavMenu, MainLayout
│   └── wwwroot/                # Static assets (JS interop, CSS, icons, manifests)
│
├── ShadowrunGM.API/            # ASP.NET Core Web API backend
│   ├── Controllers/            # REST endpoints
│   ├── Application/            # CQRS handlers, FlowRight.Cqrs.Http
│   ├── Domain/                 # Entities, Value Objects, Aggregates (DDD)
│   ├── Infrastructure/         # EF Core, Repositories, Persistence
│   └── SemanticKernel/         # Plugins, connectors, AI orchestration
│
├── ShadowrunGM.Tests/          # Unit + Integration tests
└── docs/                       # Project documentation
```

---

## Semantic Kernel Plugin Architecture

Plugins expose domain functionality in a **consistent schema** so both UI and AI assistants can consume them.

* **CatalogPlugin** → query powers, spells, gear, augments
* **CampaignsPlugin** → manage campaigns, characters, linked catalog items
* **RulesPlugin** → query rulebook sections, glossary, dice mechanics
* **SettingsPlugin** → manage user profiles, preferences, content rating

---

## AI Model Strategy

See [ai\_models.md](ai_models.md) for details.

### Summary

* **Local models** (Ollama, GPU-hosted): low-latency inference, offline play, cost control.
* **Cloud models** (OpenAI, Anthropic, Azure/AWS): advanced reasoning, long context, compliance.
* **Hybrid orchestration**: local for RAG + quick lookups, cloud for deep reasoning + storytelling.

---

## Future Expansion

* **Benchmarking Harness** → Compare local vs. cloud latency, quality, and cost.
* **Hybrid Model Mix** → Use best model per task (rules lookup vs narrative creation).
* **Content Rating Integration** → Enforce age/maturity filters in responses.
* **AI GM Assistant** → Long-term goal: orchestrated AI that automates GM tasks, rolls dice, generates NPCs, and enforces rules.

---

## Next Steps

* Define initial **Catalog schema** in `Domain/`.
* Connect schema to **Import pipeline** for PDF parsing.
* Stand up **local model testing** (Ollama on RTX 3090).
* Expand plugins as domain knowledge grows.
