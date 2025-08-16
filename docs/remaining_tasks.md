# Remaining Tasks

This document tracks the major tasks left for our Shadowrun GM project. Each bullet includes extra context so we don’t lose track of what “done” means. For now, everything is combined here — but we may split docs later (e.g., `architecture.md`, `ai_providers.md`, etc.) to keep things organized.

---

## Navigation & Layout

* [ ] Polish navigation drawers (icons, tooltips, behavior consistency between desktop and mobile)
* [ ] Ensure responsive handling of Campaigns/Catalog/Rules sections (verify routes and groupings work in both drawers)

## Stubbed Pages

* [x] Campaign Hub stub
* [x] Campaign Hub → Create Campaign stub
* [x] Catalog (with Powers, Spells, Gear, Augments)
* [x] Rules stub
* [x] Settings stub
* [ ] Campaign Overview (planned, should summarize characters, notes, and quick links)

## Settings Page

* [x] Initial layout (avatar, display name, rating selector)
* [ ] Hook up persistence (local storage now, backend later)
* [ ] Enforce rating filters across app (teen/adult/mature/NC-17)
* [ ] Add profile editing fields (email, password, preferences if accounts enabled later)

## Catalog

* [ ] Browsing UI for each item type (cards, lists, pagination)
* [ ] Detail modal/page for individual items (stats, cost, description)
* [ ] Search & filter (by category, cost, sourcebook, etc.)
* [ ] Validation state indicators (flag unvalidated imported items)

## Data Import

* [ ] PDF import system (MVP: manual select + parse)
* [ ] Parsing and placing data into proper collections (Powers, Spells, Gear, Augments)
* [ ] Validation / correction flow (manual correction UI for OCR issues)
* [ ] Flag items as validated before appearing in catalog

## Campaigns

* [ ] Campaign overview page (summary of characters, notes, linked catalog items)
* [ ] Campaign management (add/remove characters, link catalog items like gear or spells)
* [ ] Connect campaign hub to overview/details (ensure proper routing)

## Rules

* [ ] Build rules browser (sections, table of contents style nav, search)
* [ ] Import or link to rule content (MVP could be stubs with anchors, full import later)

## AI Integration & Model Strategy

* [ ] Define Semantic Kernel plugin architecture

  * Plugin stubs for **Catalog**, **Campaigns**, **Rules**, **Settings**
  * Ensure we can inject domain-specific knowledge for RAG
* [ ] Local models (via Ollama + 3090 GPU)

  * `llama3.1:8b` (default chat model)
  * `mistral:7b` (fast dev loop)
  * `qwen2:14b` (local “extra juice”)
  * `phi3:mini` (compact baseline)
  * `all-minilm` / `e5` (embeddings)
* [ ] Cloud models (fallbacks / cross-validation)

  * OpenAI GPT-4o / GPT-5
  * Anthropic Claude
  * Azure / AWS-hosted models
* [ ] Benchmark script for comparing local vs cloud (latency, tokens/sec)
* [ ] Guardrail presets tied to Content Rating (temp, max tokens, style)

## Future Enhancements

* [ ] Authentication / user accounts (link settings profile)
* [ ] Multi-user support (GM + players, shared campaigns)
* [ ] Cloud save sync (persist across devices)
* [ ] AI GM assistant integration (rules queries, automation, dice rolling)

---

# Commit Message Conventions

We’ll follow **Conventional Commits** to keep history clean and Semantic Release friendly.

## Examples

* **feat:** add settings page with profile and rating selector
* **fix:** correct nav links for Campaign Hub
* **style:** tweak drawer tooltips
* **refactor:** restructure catalog stubs into folders
* **docs:** update `remaining_tasks.md`
* **chore:** bump dependencies

## Rules

* Keep commit subject short (≤ 72 chars)
* Use imperative mood (e.g., "add" not "added")
* Optionally include body with reasoning/details
* Use footer for breaking changes or issue refs

---
