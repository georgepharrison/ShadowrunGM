using ShadowrunGM.API.Importing.Abstractions;
using System.Security.Cryptography;

namespace ShadowrunGM.API.Importing.Storage;

public sealed class LocalFileBlobStorage : IBlobStorage
{
    #region Private Members

    private readonly string _root;

    #endregion Private Members

    #region Public Constructors

    public LocalFileBlobStorage()
    {
        _root = Path.Combine(Path.GetTempPath(), "shadowrunGM_uploads");
        Directory.CreateDirectory(_root);
    }

    #endregion Public Constructors

    #region Public Methods

    public async Task<(string blobUri, string sha256)> StoreAsync(Stream input, string fileName, CancellationToken cancellationToken = default)
    {
        Guid id = Guid.NewGuid();
        string safeName = string.Concat(fileName.Split(Path.GetInvalidFileNameChars()));
        string path = Path.Combine(_root, $"{id}_{safeName}");

        await using FileStream fileStream = new(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true);
        using SHA256 sha = SHA256.Create();
        byte[] buffer = new byte[81920];
        int read;

        while ((read = await input.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            sha.TransformBlock(buffer, 0, read, null, 0);
        }

        sha.TransformFinalBlock([], 0, 0);
        string hash = Convert.ToHexString(sha.Hash!);

        return ($"file://{path}", hash);
    }

    #endregion Public Methods
}
