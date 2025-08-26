using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using ShadowrunGM.API.Importing.Abstractions;

namespace ShadowrunGM.API.Importing.Embedding;

public sealed class OllamaEmbeddingProvider : ITextEmbeddingProvider
{
    #region Private Members

    private readonly IEmbeddingGenerator<string, Embedding<float>> _generator;
    private readonly Kernel _kernel;
    private readonly string _modelName;
    private readonly int _versionTag;

    #endregion Private Members

    #region Public Constructors

    public OllamaEmbeddingProvider(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        string baseUrl = configuration["Ollama:BaseUrl"]
            ?? throw new InvalidOperationException("Missing config: Ollama:BaseUrl");

        _modelName = configuration["Ollama:EmbeddingModel"]
            ?? throw new InvalidOperationException("Missing config: Ollama:EmbeddingModel");

        // Optional: allow override in config, default 1
        _versionTag = int.TryParse(configuration["Ollama:EmbeddingVersion"], out int v) ? v : 1;

        _kernel = Kernel.CreateBuilder()
            .AddOllamaEmbeddingGenerator(_modelName, new Uri(baseUrl))
            .Build();

        _generator = _kernel.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<float[][]> EmbedAsync(IEnumerable<string> texts, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(texts);

        // materialize once to avoid multiple enumeration
        IList<string> list = texts as IList<string> ?? [.. texts];
        if (list.Count == 0) return [];

        GeneratedEmbeddings<Embedding<float>> embeddings = await _generator.GenerateAsync(list, cancellationToken: ct);

        // Convert to float[][] for Pgvector.Vector constructor
        float[][] result = new float[embeddings.Count][];
        for (int i = 0; i < embeddings.Count; i++)
            result[i] = embeddings[i].Vector.ToArray();

        return result;
    }

    #endregion Public Methods

    #region Public Properties

    public string ModelName => _modelName;
    public int VersionTag => _versionTag;

    #endregion Public Properties
}
