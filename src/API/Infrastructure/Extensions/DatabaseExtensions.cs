using Microsoft.EntityFrameworkCore;
using ShadowrunGM.API.Infrastructure.Seeders;

namespace ShadowrunGM.API.Infrastructure.Extensions;

/// <summary>
/// Extension methods for database operations.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Ensures the database is created and seeded with initial data.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task EnsureDatabaseCreatedAndSeededAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        ShadowrunContext context = scope.ServiceProvider.GetRequiredService<ShadowrunContext>();

        // Apply any pending migrations
        await context.Database.MigrateAsync();

        // Seed basic game items
        await GameItemSeeder.SeedGameItemsAsync(context);
    }

    /// <summary>
    /// Seeds the database with development data.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task SeedDevelopmentDataAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        using IServiceScope scope = app.Services.CreateScope();
        ShadowrunContext context = scope.ServiceProvider.GetRequiredService<ShadowrunContext>();

        // Add development-specific seed data here if needed
        await GameItemSeeder.SeedGameItemsAsync(context);
    }
}