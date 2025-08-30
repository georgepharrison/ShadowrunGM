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

## 🏗 Infrastructure

### Database Setup

The project uses **PostgreSQL with pgvector** for AI-powered features:

```bash
# Start PostgreSQL with Docker Compose
docker-compose up -d postgres

# Apply Entity Framework migrations
cd src/API && dotnet ef database update

# Run the API (includes automatic data seeding)
dotnet run
```

See [Database Setup Guide](docs/database-setup.md) for detailed configuration.

### Key Infrastructure Components

* **Docker Compose** - PostgreSQL + pgvector for local development
* **Entity Framework Core** - Domain entity persistence with PostgreSQL
* **Repository Pattern** - Data access with comprehensive Result<T> error handling
* **Data Seeding** - Automatic Shadowrun 6e equipment and content population
* **TDD Implementation** - Full test coverage with xUnit and Shouldly

For implementation details, see:
* [Infrastructure Layer README](src/API/Infrastructure/README.md)
* [CharacterRepository Implementation](docs/engineering/character-repository-implementation.md)  
* [Data Seeding Strategy](docs/engineering/data-seeding-strategy.md)

---

## 🛠 Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for local setup, branching strategy, and coding standards.

---

## 📖 Documentation

All documentation lives in the [docs/](docs/) folder. Key entry point: [docs/README.md](docs/README.md)

---

## 🚀 Current Status

**Completed Infrastructure (Days 4-5):**
* ✅ Docker Compose PostgreSQL with pgvector setup
* ✅ Entity Framework migrations with Character aggregate support
* ✅ CharacterRepository implementation with comprehensive TDD
* ✅ Game items seed data with baseline Shadowrun 6e equipment
* ✅ Result<T> pattern integration throughout data layer

**Next Steps:**
* Hook up Semantic Kernel plugins for AI orchestration
* Begin local Ollama testing on RTX 3090  
* Extend Overseer AI for player/GM interaction
* Implement CQRS command/query handlers for Character operations
