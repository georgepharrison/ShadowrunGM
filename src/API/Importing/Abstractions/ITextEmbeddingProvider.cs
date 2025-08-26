namespace ShadowrunGM.API.Importing.Abstractions;

public interface ITextEmbeddingProvider
{
    #region Public Methods

    Task<float[][]> EmbedAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);

    #endregion Public Methods

    #region Public Properties

    string ModelName { get; }
    int VersionTag { get; }

    #endregion Public Properties
}
