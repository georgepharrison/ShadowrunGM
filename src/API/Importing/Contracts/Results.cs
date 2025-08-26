namespace ShadowrunGM.API.Importing.Contracts;

public sealed record EmbeddingResult(int VectorsIndexed);

public sealed record ImportOutcome(
    Guid JobId,
    ParsedDoc Parsed,
    IReadOnlyList<Chunk> Chunks,
    IReadOnlyList<LabeledChunk> Labeled,
    PersistResult Persisted,
    EmbeddingResult Embeddings);

public sealed record PersistResult(int RecordsSaved, long SourcebookId);