using ShadowrunGM.API.Endpoints;
using ShadowrunGM.API.Importing.Contracts;

namespace ShadowrunGM.API.Importing.Abstractions;

public interface IChunker
{
    #region Public Methods

    Task<IReadOnlyList<Chunk>> ChunkAsync(ParsedDoc doc, CancellationToken cancellationToken = default);

    #endregion Public Methods
}
