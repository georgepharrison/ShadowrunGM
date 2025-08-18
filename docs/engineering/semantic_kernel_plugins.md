# Semantic Kernel Plugins

This document describes the Semantic Kernel plugin architecture used in **ShadowrunGM**. Plugins expose domain functionality in a consistent schema, making them usable by both the UI and AI assistants.

---

## Overview

* Each plugin corresponds to a bounded context in the domain.
* Plugins wrap domain services, CQRS handlers, or repositories.
* Exposed functions follow a consistent contract (name, description, parameters, return type).
* Enables **chat-driven AI orchestration**: both GM AI and Overseer AI can call the same functions.

---

## Core Plugins

### CatalogPlugin

* **Purpose**: Query available rulebook content.
* **Functions**:

  * GetGearByCategory
  * GetSpells (filters by tradition, drain, type)
  * GetAugments (cyberware, bioware, restrictions)
  * GetPowers (adept, awakened)
  * SearchCatalogItems

### CampaignsPlugin

* **Purpose**: Manage campaigns and characters.
* **Functions**:

  * CreateCampaign
  * AddCharacterToCampaign
  * AssignGearToCharacter
  * RecordJobOutcome (success, failure, consequences)
  * UpdateCampaignLog

### RulesPlugin

* **Purpose**: Provide rule lookups and mechanics.
* **Functions**:

  * GetRuleSection (by keyword or reference)
  * GetDicePool (based on attributes/skills)
  * RollDice (simulate dice mechanics)
  * GlossaryLookup

### SettingsPlugin

* **Purpose**: Handle user preferences and system config.
* **Functions**:

  * GetUserProfile
  * UpdateUserProfile
  * SetContentRating (teen, adult, mature, NC-17)
  * ManageDisplaySettings

### ImportPlugin

* **Purpose**: Trigger import workflows.
* **Functions**:

  * ParsePDF (OCR + extraction)
  * ValidateParsedData
  * ApproveImport (save into catalog)
  * ReprocessFailedImports

---

## Design Principles

* **Consistency**: Every function returns structured results (JSON with metadata, errors, or validation).
* **Traceability**: Errors from domain validation are passed directly back to AI/UX.
* **Flexibility**: Overseer AI and GM AI use the same plugins, but prompt context determines intent.
* **Extensibility**: Future plugins (e.g., MarketplacePlugin, NPCPlugin) can be added without breaking existing integrations.

---

## Next Steps

* Define JSON schemas for plugin inputs/outputs.
* Map plugin functions to existing CQRS handlers.
* Test with Overseer AI chat panel integration.
* Expand coverage to include CharacterBuilderPlugin (post-MVP).
