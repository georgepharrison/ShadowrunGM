using ShadowrunGM.API.Importing.Contracts;

namespace ShadowrunGM.API.Importing.Abstractions;

public interface IEmbedderIndexer
{
    #region Public Methods

    Task<EmbeddingResult> IndexAsync(Guid jobId, PersistResult persisted, IReadOnlyList<LabeledChunk> labeled, CancellationToken cancellationToken = default);

    #endregion Public Methods
}