# ShadowrunGM

A modern Game Master assistant for Shadowrun, built with **Blazor WebAssembly**, **ASP.NET Core**, and **AI integration via Semantic Kernel**.

---

## ✨ Features (MVP)

* Campaign management with AI-powered GM assistant
* Catalog browsing for powers, spells, gear, and augments
* Character builder with Point-Buy system (100CP)
* PDF rulebook import → structured domain tables
* Light/dark theming with MudBlazor UI
* Local + cloud AI model orchestration (for rules, narrative, and assistance)

---

## 📂 Project Structure

* **ShadowrunGM.UI/** → Blazor WebAssembly frontend
* **ShadowrunGM.API/** → ASP.NET Core backend (CQRS, DDD, Semantic Kernel)
* **ShadowrunGM.Tests/** → Unit + integration tests
* **docs/** → Documentation (architecture, design, contributing)

For detailed breakdowns, see:

* [Architecture Overview](docs/engineering/architecture_overview.md)
* [Solution Structure](docs/engineering/solution_structure.md)
* [Semantic Kernel Plugins](docs/engineering/semantic_kernel_plugins.md)
* [AI Models](docs/engineering/ai_models.md)

---

## 🛠 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for local setup, branching strategy, and coding standards.

---

## 📖 Documentation

All documentation lives in the [docs/](docs/) folder. Key entry point: [docs/README.md](docs/README.md)

---

## 🚀 Next Steps

* Implement catalog schema in Domain + Import pipeline
* Hook up Semantic Kernel plugins for AI orchestration
* Begin local Ollama testing on RTX 3090
* Extend Overseer AI for player/GM interaction
