namespace ShadowrunGM.API.Importing.Abstractions;

public interface IBlobStorage
{
    #region Public Methods

    Task<(string blobUri, string sha256)> StoreAsync(Stream input, string fileName, CancellationToken cancellationToken = default);

    #endregion Public Methods
}