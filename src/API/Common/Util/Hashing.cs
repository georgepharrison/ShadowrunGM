using System.Buffers;
using System.Security.Cryptography;
using System.Text;

namespace ShadowrunGM.API.Common.Util;

/// <summary>
/// Small helpers for stable, lower-case hex hashes (MD5/SHA256).
/// Stream overloads do not dispose or rewind the stream.
/// </summary>
internal static class Hashing
{
    #region Public Methods

    public static string MD5Hex(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        using var md5 = MD5.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        return ToLowerHex(md5.ComputeHash(bytes));
    }

    public static string MD5Hex(ReadOnlySpan<byte> data)
    {
        using var md5 = MD5.Create();
        Span<byte> hash = stackalloc byte[16];
        md5.TryComputeHash(data, hash, out _);
        return ToLowerHex(hash);
    }

    public static async Task<string> MD5HexAsync(Stream stream, CancellationToken ct = default) =>
        await ComputeHexAsync(MD5.Create(), stream, ct).ConfigureAwait(false);

    public static string SHA256Hex(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        using var sha = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        return ToLowerHex(sha.ComputeHash(bytes));
    }

    public static string SHA256Hex(ReadOnlySpan<byte> data)
    {
        using var sha = SHA256.Create();
        Span<byte> hash = stackalloc byte[32];
        sha.TryComputeHash(data, hash, out _);
        return ToLowerHex(hash);
    }

    public static async Task<string> SHA256HexAsync(Stream stream, CancellationToken ct = default) =>
        await ComputeHexAsync(SHA256.Create(), stream, ct).ConfigureAwait(false);

    #endregion Public Methods

    #region Private Methods

    private static async Task<string> ComputeHexAsync(HashAlgorithm algo, Stream stream, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using (algo)
        {
            // We feed the stream in chunks; we do not change Position.
            const int BufferSize = 81920;
            byte[] buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                int read;
                while ((read = await stream.ReadAsync(buffer.AsMemory(0, BufferSize), ct).ConfigureAwait(false)) > 0)
                {
                    algo.TransformBlock(buffer, 0, read, null, 0);
                }
                algo.TransformFinalBlock([], 0, 0);
                return ToLowerHex(algo.Hash!);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer, clearArray: false);
            }
        }
    }

    private static string ToLowerHex(ReadOnlySpan<byte> bytes)
    {
        // 2 chars per byte
        Span<char> chars = bytes.Length <= 128 ? stackalloc char[bytes.Length * 2] : new char[bytes.Length * 2];
        const string hex = "0123456789abcdef";
        int j = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            byte b = bytes[i];
            chars[j++] = hex[b >> 4];
            chars[j++] = hex[b & 0x0F];
        }
        return new string(chars);
    }

    #endregion Private Methods
}