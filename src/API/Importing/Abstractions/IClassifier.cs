using ShadowrunGM.API.Importing.Contracts;

namespace ShadowrunGM.API.Importing.Abstractions;

public interface IClassifier
{
    #region Public Methods

    Task<IReadOnlyList<LabeledChunk>> ClassifyAsync(IReadOnlyList<Chunk> chunks, CancellationToken cancellationToken = default);

    #endregion Public Methods
}