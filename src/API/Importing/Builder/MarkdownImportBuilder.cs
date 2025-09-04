using ShadowrunGM.API.Endpoints;
using ShadowrunGM.API.Importing.Abstractions;
using ShadowrunGM.API.Importing.Chunking;
using ShadowrunGM.API.Importing.Contracts;
using System.Text;
using FlowRight.Core.Results;

namespace ShadowrunGM.API.Importing.Builder;

public sealed class MarkdownImportBuilder(
    IParser parser,
    IChunker chunker,
    IClassifier classifier,
    IStructuredPersister persister,
    IEmbedderIndexer embedder,
    IEnumerable<IGameItemExtractor> itemExtractors,
    IGameItemPersister itemPersister,
    IEnumerable<IMagicAbilityExtractor> magicExtractors,
    IMagicAbilityPersister magicPersister) : IImportBuilder
{
    #region Private Members

    private readonly IChunker _chunker = chunker;
    private readonly IClassifier _classifier = classifier;
    private readonly IEmbedderIndexer _embedder = embedder;
    private readonly IEnumerable<IGameItemExtractor> _itemExtractors = itemExtractors;
    private readonly IGameItemPersister _itemPersister = itemPersister;
    private readonly IEnumerable<IMagicAbilityExtractor> _magicExtractors = magicExtractors;
    private readonly IMagicAbilityPersister _magicPersister = magicPersister;
    private readonly IParser _parser = parser;
    private readonly IStructuredPersister _persister = persister;
    private IReadOnlyList<Chunk>? _chunks;
    private ParsedDoc? _doc;
    private EmbeddingResult? _embeddings;
    private List<GameItemDraft>? _itemDrafts;
    private IReadOnlyList<LabeledChunk>? _labeled;
    private List<MagicAbilityDraft>? _magicDrafts;
    private PersistResult? _persisted;

    #endregion Private Members

    #region Public Methods

    public Result<ImportOutcome> Build()
    {
        if (_doc is null || _chunks is null || _labeled is null || _persisted is null || _embeddings is null)
            return Result.Failure<ImportOutcome>($"All steps must be completed before {nameof(Build)}().");

        return Result.Success(new ImportOutcome(
            JobId: Guid.Empty,
            Parsed: _doc,
            Chunks: _chunks,
            Labeled: _labeled,
            Persisted: _persisted,
            Embeddings: _embeddings));
    }

    public async Task<Result<IImportBuilder>> ChunkAsync(CancellationToken cancellationToken = default)
    {
        if (_doc is null) return Result.Failure<IImportBuilder>($"{nameof(ParseAsync)} must run first.");
        IReadOnlyList<Chunk> chunks = await _chunker.ChunkAsync(_doc, cancellationToken);
        _chunks = HtmlTableSplitter.SplitTables(chunks);
        return this;
    }

    public async Task<Result<IImportBuilder>> ClassifyAsync(CancellationToken cancellationToken = default)
    {
        if (_chunks is null) return Result.Failure<IImportBuilder>($"{nameof(ChunkAsync)} must run first.");
        _labeled = await _classifier.ClassifyAsync(_chunks, cancellationToken);
        return this;
    }

    public async Task<Result<IImportBuilder>> EmbedAndIndexAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        if (_labeled is null || _persisted is null) return Result.Failure<IImportBuilder>($"{nameof(ClassifyAsync)} must run first.");
        _embeddings = await _embedder.IndexAsync(jobId, _persisted, _labeled, cancellationToken);
        return this;
    }

    public async Task<Result<IImportBuilder>> ExtractAsync(CancellationToken cancellationToken = default)
    {
        if (_labeled is null) return Result.Failure<IImportBuilder>($"{nameof(ClassifyAsync)} must run first.");
        if (_persisted is null) return Result.Failure<IImportBuilder>($"{nameof(PersistAsync)} must run first.");

        List<GameItemDraft> items = [];
        List<MagicAbilityDraft> abilities = [];

        foreach (LabeledChunk c in _labeled)
        {
            IGameItemExtractor? ie = _itemExtractors.FirstOrDefault(x => x.CanHandle(c.Label));
            if (ie is not null)
            {
                if (ie.TryExtract(c, _persisted.SourcebookId, out GameItemDraft? gameItem))
                {
                    items.Add(gameItem);
                }
                continue;
            }

            IMagicAbilityExtractor? me = _magicExtractors.FirstOrDefault(x => x.CanHandle(c.Label));
            if (me is not null)
            {
                if (me.TryExtract(c, _persisted.SourcebookId, out MagicAbilityDraft? magicAbility))
                {
                    abilities.Add(magicAbility);
                }
                continue;
            }
        }

        _itemDrafts = items;
        _magicDrafts = abilities;
        return this;
    }

    public async Task<Result<IImportBuilder>> ParseAsync(string blobUri, CancellationToken cancellationToken = default)
    {
        Result<IImportBuilder> result = await ValidateLooksLikeMarkdownAsync(blobUri, cancellationToken);

        if (result.IsSuccess)
        {
            _doc = await _parser.ParseAsync(blobUri, cancellationToken);
        }

        return result;
    }

    public async Task<Result<IImportBuilder>> PersistAsync(ImportWorkItem item, CancellationToken cancellationToken = default)
    {
        if (_labeled is null) return Result.Failure<IImportBuilder>($"{nameof(ClassifyAsync)} must run first.");
        _persisted = await _persister.SaveAsync(item, _labeled, cancellationToken);
        return this;
    }

    public async Task<Result<IImportBuilder>> PersistStructuredAsync(CancellationToken ct = default)
    {
        int changed = 0;
        if (_itemDrafts is not null && _itemDrafts.Count > 0)
            changed += await _itemPersister.UpsertAsync(_itemDrafts, ct);
        if (_magicDrafts is not null && _magicDrafts.Count > 0)
            changed += await _magicPersister.UpsertAsync(_magicDrafts, ct);

        // optional: store count in _persisted or a side record for telemetry
        return this;
    }

    #endregion Public Methods

    #region Private Methods

    private async Task<Result<IImportBuilder>> ValidateLooksLikeMarkdownAsync(string blobUri, CancellationToken ct)
    {
        string path = new Uri(blobUri).LocalPath;

        byte[] bytes;
        try { bytes = await File.ReadAllBytesAsync(path, ct); }
        catch (Exception ex)
        {
            return Result.Failure<IImportBuilder>($"Unable to read uploaded file: {ex.Message}");
        }

        // Guard: obvious PDF magic bytes
        if (bytes.Length >= 5 &&
            bytes[0] == (byte)'%' && bytes[1] == (byte)'P' && bytes[2] == (byte)'D' && bytes[3] == (byte)'F' && bytes[4] == (byte)'-')
            return Result.Failure<IImportBuilder>("File content appears to be a PDF, not Markdown.");

        // Guard: looks binary (NUL bytes)
        if (bytes.AsSpan().IndexOf((byte)0) >= 0)
            return Result.Failure<IImportBuilder>("File contains NUL bytes and is likely binary.");

        // UTF-8 strict check
        string text = Encoding.UTF8.GetString(bytes);
        Encoding strict = Encoding.GetEncoding("UTF-8", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
        try { _ = strict.GetBytes(text); }
        catch { return Result.Failure<IImportBuilder>("File is not valid UTF-8 text."); }

        // Quick markdown heuristics on a sample
        string sample = text.Length > 8192 ? text[..8192] : text;
        int newlineCount = sample.Count(c => c == '\n');
        int nonPrintable = sample.Count(c => char.IsControl(c) && c is not '\r' and not '\n' and not '\t');

        if (newlineCount < 1 || nonPrintable > sample.Length * 0.02)
            return Result.Failure<IImportBuilder>("File does not look like plain text/markdown.");

        int mdTokens =
            Count(sample, "# ") +
            Count(sample, "## ") +
            Count(sample, "- ") +
            Count(sample, "* ") +
            Count(sample, "`") +
            Count(sample, "](") +
            Count(sample, "[") +
            Count(sample, "---");

        if (mdTokens < 3)
            return Result.Failure<IImportBuilder>("Content lacks common Markdown structure.");

        return Result.Success(this as IImportBuilder);

        static int Count(string s, string needle)
        {
            int i = 0, c = 0;
            while ((i = s.IndexOf(needle, i, StringComparison.Ordinal)) >= 0) { c++; i += needle.Length; }
            return c;
        }
    }

    #endregion Private Methods
}
