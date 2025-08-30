using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ShadowrunGM.API.Infrastructure;
using ShadowrunGM.API.Infrastructure.Repositories;

namespace ShadowrunGM.Infrastructure.Tests.TestHelpers;

/// <summary>
/// Builder pattern for creating CharacterRepository instances for testing.
/// </summary>
public sealed class CharacterRepositoryBuilder
{
    private ShadowrunContext? _context;
    private ILogger<CharacterRepository>? _logger;
    private bool _useInMemoryDatabase = true;
    private bool _useFailingDatabase = false;
    private string? _databaseName;

    /// <summary>
    /// Uses a specific ShadowrunContext instance.
    /// </summary>
    /// <param name="context">The context to use.</param>
    /// <returns>The builder instance.</returns>
    public CharacterRepositoryBuilder WithContext(ShadowrunContext context)
    {
        _context = context;
        _useInMemoryDatabase = false;
        return this;
    }

    /// <summary>
    /// Uses an in-memory database with a specific name.
    /// </summary>
    /// <param name="databaseName">The database name. If null, a random GUID will be used.</param>
    /// <returns>The builder instance.</returns>
    public CharacterRepositoryBuilder WithInMemoryDatabase(string? databaseName = null)
    {
        _databaseName = databaseName ?? Guid.NewGuid().ToString();
        _useInMemoryDatabase = true;
        _useFailingDatabase = false;
        return this;
    }

    /// <summary>
    /// Uses a failing database context for testing error scenarios.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public CharacterRepositoryBuilder WithFailingDatabase()
    {
        _useFailingDatabase = true;
        _useInMemoryDatabase = false;
        return this;
    }

    /// <summary>
    /// Uses a specific logger instance.
    /// </summary>
    /// <param name="logger">The logger to use.</param>
    /// <returns>The builder instance.</returns>
    public CharacterRepositoryBuilder WithLogger(ILogger<CharacterRepository> logger)
    {
        _logger = logger;
        return this;
    }

    /// <summary>
    /// Uses a substitute logger for testing.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public CharacterRepositoryBuilder WithMockLogger()
    {
        _logger = Substitute.For<ILogger<CharacterRepository>>();
        return this;
    }

    /// <summary>
    /// Builds the CharacterRepository instance.
    /// </summary>
    /// <returns>A configured CharacterRepository.</returns>
    public CharacterRepository Build()
    {
        ShadowrunContext context = _context ?? CreateContext();
        ILogger<CharacterRepository> logger = _logger ?? Substitute.For<ILogger<CharacterRepository>>();

        return new CharacterRepository(context, logger);
    }

    /// <summary>
    /// Builds the CharacterRepository with its context for disposal management.
    /// </summary>
    /// <returns>A tuple containing the repository and its context.</returns>
    public (CharacterRepository Repository, ShadowrunContext Context) BuildWithContext()
    {
        ShadowrunContext context = _context ?? CreateContext();
        ILogger<CharacterRepository> logger = _logger ?? Substitute.For<ILogger<CharacterRepository>>();

        CharacterRepository repository = new(context, logger);
        return (repository, context);
    }

    /// <summary>
    /// Creates a context based on the configured settings.
    /// </summary>
    private ShadowrunContext CreateContext()
    {
        if (_useFailingDatabase)
        {
            return CreateFailingContext();
        }

        if (_useInMemoryDatabase)
        {
            return CreateInMemoryContext();
        }

        return CreateInMemoryContext(); // Default fallback
    }

    /// <summary>
    /// Creates an in-memory Entity Framework context for testing.
    /// </summary>
    private ShadowrunContext CreateInMemoryContext()
    {
        string databaseName = _databaseName ?? Guid.NewGuid().ToString();
        
        DbContextOptions<ShadowrunContext> options = new DbContextOptionsBuilder<ShadowrunContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new ShadowrunContext(options);
    }

    /// <summary>
    /// Creates a context that will fail on operations for testing error scenarios.
    /// </summary>
    private static ShadowrunContext CreateFailingContext()
    {
        // This will fail because it tries to connect to a non-existent database
        DbContextOptions<ShadowrunContext> options = new DbContextOptionsBuilder<ShadowrunContext>()
            .UseNpgsql("Host=nonexistent;Database=nonexistent;Username=nonexistent;Password=nonexistent")
            .Options;

        return new ShadowrunContext(options);
    }
}