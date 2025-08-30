# Database Seeders

This directory contains seed data for initializing the database with basic game content.

## GameItemSeeder

Seeds the `game_items` table with basic Shadowrun 6th Edition equipment including:

### Weapons
- Light pistols (Ares Light Fire 70, Fichetti Security 600)
- Heavy pistols (Ares Predator VI, Colt Government 2066)
- Submachine guns (Heckler & Koch MP-2013)

### Armor
- Light armor (Armor Jacket, Lined Coat)

### Electronics
- Communication gear (basic Commlink)
- Decking equipment (basic Cyberdeck)

### Gear
- Medical supplies (Medkit Rating 3)
- Outdoor gear (Climbing Gear)

### Augmentations
- Basic cyberware (Datajack, Cybereyes Rating 1)

### Vehicles
- Personal transportation (Yamaha Growler bike, Honda Spirit car)

## Usage

The seeder is automatically run during application startup via `DatabaseExtensions.EnsureDatabaseCreatedAndSeededAsync()`.

Seeds are only applied if the tables are empty, preventing duplicate data on subsequent runs.

## Data Structure

All items follow the unified `GameItem` schema with:
- Core identification fields (Name, Slug, ItemType, Category)
- Economic data (Cost, Availability)
- Game statistics stored as JSON in `StatsJson` field
- Source reference (SourcebookId, Page)
- Audit timestamps

Items are linked to a core sourcebook (`CRB` - Core Rulebook) which is also seeded automatically.