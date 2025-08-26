using ShadowrunGM.API.Importing.Abstractions;
using ShadowrunGM.API.Importing.Contracts;
using System.Text;
using System.Text.RegularExpressions;

namespace ShadowrunGM.API.Endpoints;

public sealed partial class MarkdownParser : IParser
{
    #region Private Members

    private static readonly Regex _headingRx = Heading();
    private static readonly Regex _pageMarkerRx = PageMarker();

    #endregion Private Members

    #region Public Methods

    public async Task<ParsedDoc> ParseAsync(string blobUri, CancellationToken ct)
    {
        string path = new Uri(blobUri).LocalPath;
        byte[] bytes = await File.ReadAllBytesAsync(path, ct);

        // quick guards (you already added stronger ones in builder; safe to re-check)
        if (bytes.Length >= 5 &&
            bytes[0] == (byte)'%' && bytes[1] == (byte)'P' && bytes[2] == (byte)'D' &&
            bytes[3] == (byte)'F' && bytes[4] == (byte)'-')
            throw new InvalidOperationException("Blob is PDF, not Markdown.");

        string text = Encoding.UTF8.GetString(bytes);
        // normalize newlines early
        string normalized = text.Replace("\r\n", "\n");
        string[] lines = normalized.Split('\n');

        List<HeadingHit> headings = new(capacity: 256);
        List<PageMark> marks = new(capacity: 256);

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            Match pm = _pageMarkerRx.Match(line);
            if (pm.Success && int.TryParse(pm.Groups[1].Value, out int pg))
            {
                marks.Add(new PageMark(pg, i));
            }

            Match hm = _headingRx.Match(line);
            if (hm.Success)
            {
                headings.Add(new HeadingHit(hm.Groups[1].Value.Length, hm.Groups[2].Value.Trim(), i));
            }
        }

        return new ParsedDoc(
            BlobUri: blobUri,
            Text: normalized,
            ContentType: "text/markdown",
            Lines: lines,
            Headings: headings,
            PageMarks: marks);
    }

    #endregion Public Methods

    #region Private Methods

    [GeneratedRegex(@"^(#+)\s+(.+)$", RegexOptions.Compiled)]
    private static partial Regex Heading();

    [GeneratedRegex(@"^---$", RegexOptions.Compiled)]
    private static partial Regex PageMarker();

    #endregion Private Methods
}