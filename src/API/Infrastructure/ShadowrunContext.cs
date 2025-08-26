using Microsoft.EntityFrameworkCore;
using ShadowrunGM.API.Infrastructure.Entities.Import;
using System.Reflection;

namespace ShadowrunGM.API.Infrastructure;

public class ShadowrunContext(DbContextOptions<ShadowrunContext> options) : DbContext(options)
{
    #region Protected Methods

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.HasPostgresExtension("pg_trgm");
        modelBuilder.HasPostgresExtension("unaccent");

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    #endregion Protected Methods

    #region Public Properties

    public DbSet<RuleContent> RuleContents => Set<RuleContent>();
    public DbSet<Sourcebook> Sourcebooks => Set<Sourcebook>();

    #endregion Public Properties
}