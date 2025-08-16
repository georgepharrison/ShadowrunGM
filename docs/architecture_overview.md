# Architecture Overview

This document outlines the planned architecture for the Shadowrun GM project. It complements the `Remaining Tasks.md` file and will eventually be split into separate docs (e.g., solution layout, Semantic Kernel plugins, AI models).

---

## Solution & Project Structure

```
ShadowrunGM/
├── ShadowrunGM.UI/             # Blazor WebAssembly (MudBlazor UI)
│   ├── Pages/                  # Razor pages (Index, Campaigns, Catalog, Rules, Settings)
│   ├── Components/             # Shared UI components (ChatPanel, Drawers, etc.)
│   ├── Shared/                 # Layouts, NavMenu, MainLayout
│   └── wwwroot/                # Static assets (JS interop, CSS)
│
├── ShadowrunGM.API/            # ASP.NET Core Web API backend
│   ├── Controllers/            # REST endpoints
│   ├── Application/            # CQRS handlers, MediatR
│   ├── Domain/                 # Entities, Value Objects, Aggregates (DDD)
│   ├── Infrastructure/         # EF Core, Repositories, Persistence
│   └── SemanticKernel/         # Plugins, connectors, AI orchestration
│
├── ShadowrunGM.Import/         # Data import pipeline (PDF parsing, validation UI)
│   ├── Services/               # OCR, parsing, validation
│   └── Storage/                # Imported items before validation
│
├── ShadowrunGM.Tests/          # Unit + Integration tests
└── docs/                       # Project documentation
```

---

## Semantic Kernel Plugin Architecture

Plugins will wrap domain functionality so both UI and AI assistants can call them.

* **CatalogPlugin** → query powers, spells, gear, augments (with filters, descriptions)
* **CampaignsPlugin** → manage campaigns, characters, linked catalog items
* **RulesPlugin** → query rulebook sections, glossary, dice mechanics
* **SettingsPlugin** → manage user profile, content rating, preferences
* **ImportPlugin** → trigger import workflows (parse PDF, validate)

Each plugin exposes functions in a **consistent schema** so both LLMs and custom UI can consume them.

---

## AI Model Strategy

### Local (Ollama, GPU-hosted)

* **Text generation**

  * llama3.1 (8B, 70B when VRAM allows)
  * mistral 7B
  * qwen2 7B
  * phi3-mini (fast, lightweight)
* **Embeddings**

  * nomic-embed-text
  * snowflake-arctic-embed
* **RAG workflow**

  * Store embeddings in vector DB (LiteDB/SQLite locally, PostgreSQL/PGVector for server)
  * Serve context to local or cloud LLMs

### Cloud (fallback / advanced reasoning)

* **OpenAI**: GPT-4o, GPT-5 (for reasoning, creative output)
* **Anthropic**: Claude 3.5 (structured reasoning, long context)
* **Azure / AWS**: enterprise hosting options

---

## Future Expansion

* **Benchmarking Harness** → compare latency, quality, and token cost for local vs. cloud models.
* **Hybrid Strategy** → local models for fast RAG lookups; cloud for advanced narrative/dialogue.
* **Content Rating Integration** → enforce filters in AI responses based on profile setting (teen, adult, mature, NC-17).
* **AI GM Assistant** → eventual goal: orchestrated tool-using AI that can automate GM tasks, roll dice, generate NPCs, and enforce rules.

---

## Next Steps

* Split this doc into `solution_structure.md`, `semantic_kernel_plugins.md`, and `ai_models.md` once it grows further.
* Define initial Catalog data schema in `Domain/` and map to import pipeline.
* Start with **local model testing (Ollama on 3090)** once your PSU + Fractal Terra arrive.
