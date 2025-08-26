using ShadowrunGM.API.Importing.Contracts;

namespace ShadowrunGM.API.Importing.Abstractions;

public interface IParser
{
    #region Public Methods

    Task<ParsedDoc> ParseAsync(string blobUri, CancellationToken cancellationToken = default);

    #endregion Public Methods
}
