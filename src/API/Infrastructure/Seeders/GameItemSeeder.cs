using Microsoft.EntityFrameworkCore;
using ShadowrunGM.API.Infrastructure.Entities.Catalog;
using ShadowrunGM.API.Infrastructure.Entities.Import;

namespace ShadowrunGM.API.Infrastructure.Seeders;

/// <summary>
/// Provides seed data for the game_items table with basic Shadowrun equipment.
/// </summary>
public static class GameItemSeeder
{
    /// <summary>
    /// Seeds basic game items for Shadowrun 6th Edition.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public static async Task SeedGameItemsAsync(ShadowrunContext context, CancellationToken cancellationToken = default)
    {
        // Check if sourcebooks exist, create core sourcebook if not
        Sourcebook? coreSourcebook = await context.Sourcebooks
            .FirstOrDefaultAsync(s => s.Code == "CRB", cancellationToken);

        if (coreSourcebook == null)
        {
            coreSourcebook = new Sourcebook
            {
                Code = "CRB",
                Title = "Shadowrun Sixth World Core Rulebook",
                Edition = "6e",
                FileName = "seed-data",
                FileHash = "seed-data-core",
                Year = 2019,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                ImportedAt = DateTimeOffset.UtcNow
            };

            context.Sourcebooks.Add(coreSourcebook);
            await context.SaveChangesAsync(cancellationToken);
        }

        // Check if game items already seeded
        bool hasGameItems = await context.GameItems.AnyAsync(cancellationToken);
        if (hasGameItems)
            return;

        DateTimeOffset now = DateTimeOffset.UtcNow;

        GameItem[] seedItems = 
        [
            // Basic Weapons - Light Pistols
            new()
            {
                Name = "Ares Light Fire 70",
                Slug = "ares-light-fire-70",
                ItemType = "Weapon",
                Category = "Light Pistol",
                Cost = 200m,
                Availability = "2R",
                StatsJson = """{"accuracy": 7, "damage": "2P", "ap": 0, "modes": "SS", "rc": 0, "ammo": "6(c)", "type": "Taser"}""",
                SourcebookId = coreSourcebook.Id,
                Page = 268,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "Fichetti Security 600",
                Slug = "fichetti-security-600",
                ItemType = "Weapon", 
                Category = "Light Pistol",
                Cost = 350m,
                Availability = "4R",
                StatsJson = """{"accuracy": 6, "damage": "3P", "ap": 0, "modes": "SS", "rc": 0, "ammo": "30(c)"}""",
                SourcebookId = coreSourcebook.Id,
                Page = 268,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Heavy Pistols
            new()
            {
                Name = "Ares Predator VI",
                Slug = "ares-predator-vi",
                ItemType = "Weapon",
                Category = "Heavy Pistol", 
                Cost = 725m,
                Availability = "5R",
                StatsJson = """{"accuracy": 5, "damage": "4P", "ap": "-1", "modes": "SS", "rc": 0, "ammo": "15(c)"}""",
                SourcebookId = coreSourcebook.Id,
                Page = 268,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "Colt Government 2066",
                Slug = "colt-government-2066", 
                ItemType = "Weapon",
                Category = "Heavy Pistol",
                Cost = 350m,
                Availability = "4R",
                StatsJson = """{"accuracy": 5, "damage": "3P", "ap": "-1", "modes": "SS", "rc": 0, "ammo": "14(c)"}""",
                SourcebookId = coreSourcebook.Id,
                Page = 268,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Submachine Guns
            new()
            {
                Name = "Heckler & Koch MP-2013",
                Slug = "hk-mp-2013",
                ItemType = "Weapon",
                Category = "Submachine Gun",
                Cost = 1400m,
                Availability = "8R",
                StatsJson = """{"accuracy": 4, "damage": "3P", "ap": 0, "modes": "SA/BF/FA", "rc": "2", "ammo": "20/25/30(c)"}""",
                SourcebookId = coreSourcebook.Id,
                Page = 269,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Basic Armor
            new()
            {
                Name = "Armor Jacket",
                Slug = "armor-jacket",
                ItemType = "Armor", 
                Category = "Light Armor",
                Cost = 1000m,
                Availability = "2",
                StatsJson = """{"defense": 12, "capacity": 8}""",
                SourcebookId = coreSourcebook.Id,
                Page = 266,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "Lined Coat",
                Slug = "lined-coat",
                ItemType = "Armor",
                Category = "Light Armor", 
                Cost = 900m,
                Availability = "4",
                StatsJson = """{"defense": 9, "capacity": 6}""",
                SourcebookId = coreSourcebook.Id,
                Page = 266,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Electronics
            new()
            {
                Name = "Commlink",
                Slug = "commlink-basic",
                ItemType = "Electronics",
                Category = "Communication",
                Cost = 100m,
                Availability = "1",
                StatsJson = """{"device_rating": 2, "programs": 2, "data_processing": 2, "firewall": 2}""",
                SourcebookId = coreSourcebook.Id,
                Page = 258,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "Cyberdeck",
                Slug = "cyberdeck-basic",
                ItemType = "Electronics", 
                Category = "Decking",
                Cost = 5000m,
                Availability = "6R",
                StatsJson = """{"device_rating": 3, "programs": 3, "attack": 2, "sleaze": 3, "data_processing": 3, "firewall": 3}""",
                SourcebookId = coreSourcebook.Id,
                Page = 258,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Basic Gear
            new()
            {
                Name = "Medkit Rating 3",
                Slug = "medkit-rating-3",
                ItemType = "Gear",
                Category = "Medical",
                Cost = 500m,
                Availability = "3",
                StatsJson = """{"rating": 3, "supplies": 3}""",
                SourcebookId = coreSourcebook.Id,
                Page = 262,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "Climbing Gear",
                Slug = "climbing-gear",
                ItemType = "Gear",
                Category = "Outdoor",
                Cost = 200m,
                Availability = "2",
                StatsJson = """{"bonus": "+2 Climbing"}""",
                SourcebookId = coreSourcebook.Id,
                Page = 262,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Cyberware - Basic
            new()
            {
                Name = "Datajack",
                Slug = "datajack",
                ItemType = "Augmentation",
                Category = "Cyberware - Headware",
                Cost = 1000m,
                Availability = "2",
                StatsJson = """{"essence": 0.1, "capacity": 0}""",
                SourcebookId = coreSourcebook.Id,
                Page = 280,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "Cybereyes Rating 1",
                Slug = "cybereyes-rating-1",
                ItemType = "Augmentation",
                Category = "Cyberware - Headware", 
                Cost = 4000m,
                Availability = "4",
                StatsJson = """{"essence": 0.2, "capacity": 2}""",
                SourcebookId = coreSourcebook.Id,
                Page = 280,
                CreatedAt = now,
                UpdatedAt = now
            },

            // Vehicles - Basic
            new()
            {
                Name = "Yamaha Growler",
                Slug = "yamaha-growler",
                ItemType = "Vehicle",
                Category = "Bike",
                Cost = 5500m,
                Availability = "3",
                StatsJson = """{"handling": "4/3", "speed": 4, "acceleration": 2, "body": 8, "armor": 6, "pilot": 1, "sensor": 2, "seats": 1}""",
                SourcebookId = coreSourcebook.Id,
                Page = 282,
                CreatedAt = now,
                UpdatedAt = now
            },
            new()
            {
                Name = "Honda Spirit",
                Slug = "honda-spirit",
                ItemType = "Vehicle",
                Category = "Car",
                Cost = 12000m,
                Availability = "2", 
                StatsJson = """{"handling": "4/3", "speed": 3, "acceleration": 2, "body": 11, "armor": 6, "pilot": 1, "sensor": 2, "seats": 4}""",
                SourcebookId = coreSourcebook.Id,
                Page = 282,
                CreatedAt = now,
                UpdatedAt = now
            }
        ];

        context.GameItems.AddRange(seedItems);
        await context.SaveChangesAsync(cancellationToken);
    }
}