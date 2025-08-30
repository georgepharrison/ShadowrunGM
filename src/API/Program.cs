using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using ShadowrunGM.API.Endpoints;
using ShadowrunGM.API.Importing.Abstractions;
using ShadowrunGM.API.Importing.Builder;
using ShadowrunGM.API.Importing.Classification;
using ShadowrunGM.API.Importing.Embedding;
using ShadowrunGM.API.Importing.Hosted;
using ShadowrunGM.API.Importing.Jobs;
using ShadowrunGM.API.Importing.Persistence;
using ShadowrunGM.API.Importing.Storage;
using ShadowrunGM.API.Infrastructure;
using ShadowrunGM.API.Infrastructure.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddSingleton<IImportQueue>(sp => new ChannelImportQueue(capacity: 300));
builder.Services.AddHostedService<ImportAgent>();

builder.Services.AddScoped<IImportDirector, ImportDirector>();
builder.Services.AddScoped<IImportBuilderFactory, ImportBuilderFactory>();

builder.Services.AddScoped<MarkdownImportBuilder>();

builder.Services.AddSingleton<IImportJobRepository, InMemoryImportJobRepository>();

builder.Services.AddSingleton<IBlobStorage, LocalFileBlobStorage>();
builder.Services.AddScoped<IParser, MarkdownParser>();
builder.Services.AddScoped<IChunker, MarkdownChunker>();
builder.Services.AddScoped<IClassifier, HeuristicClassifier>();
builder.Services.AddScoped<IStructuredPersister, EfStructuredPersister>();
builder.Services.AddScoped<IEmbedderIndexer, EfEmbedderIndexer>();

builder.Services.AddSingleton<IGameItemExtractor, WeaponExtractor>();
builder.Services.AddSingleton<IMagicAbilityExtractor, SpellExtractor>();
// No-ops can also be added as needed

builder.Services.AddScoped<IGameItemPersister, EfGameItemPersister>();
builder.Services.AddScoped<IMagicAbilityPersister, EfMagicAbilityPersister>();

builder.Services.AddDbContext<ShadowrunContext>((sp, options) =>
{
    string connectionString = sp.GetRequiredService<IConfiguration>()
        .GetConnectionString("DefaultConnection")
        ?? throw new KeyNotFoundException("Connection string 'DefaultConnection' not found.");

    options.UseNpgsql(connectionString, o => o.UseVector())
        .UseSnakeCaseNamingConvention();

    options.EnableSensitiveDataLogging();
    options.LogTo(Console.WriteLine, LogLevel.Warning);
});

builder.Services.AddKeyedSingleton("import", (sp, _) =>
{
    IConfiguration config = sp.GetRequiredService<IConfiguration>();
    Uri baseUrl = new(config["OpenAI:BaseUrl"] ?? throw new KeyNotFoundException("OpenAI:BaseUrl not found in configuration."));
    string model = config["OpenAI:EmbeddingModel"] ?? throw new KeyNotFoundException("OpenAI:EmbeddingModel not found in configuration.");

    return Kernel.CreateBuilder()
        .AddOllamaEmbeddingGenerator(model, baseUrl)
        .Build();
});

builder.Services.AddSingleton<ITextEmbeddingProvider, OllamaEmbeddingProvider>();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapImportEndpoints();

// Ensure database is created and seeded
await app.EnsureDatabaseCreatedAndSeededAsync();

app.Run();