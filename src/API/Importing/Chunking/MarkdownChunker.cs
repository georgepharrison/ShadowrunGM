using Microsoft.EntityFrameworkCore;
using ShadowrunGM.API.Importing.Abstractions;
using ShadowrunGM.API.Importing.Contracts;
using System.Text;
using System.Text.RegularExpressions;

namespace ShadowrunGM.API.Endpoints;

public sealed partial class MarkdownChunker : IChunker
{
    #region Private Members

    private const int _maxBlockSize = 1600;
    private static readonly Regex _headingRx = Heading();
    private static readonly Regex _htmlTableCloseRx = HtmlTableClose();
    private static readonly Regex _htmlTableOpenRx = HtmlTableOpen();
    private static readonly Regex _pageMarkerRx = PageMarker();

    #endregion Private Members

    #region Public Methods

    public Task<IReadOnlyList<Chunk>> ChunkAsync(ParsedDoc doc, CancellationToken cancellationToken)
    {
        State s = new(doc.Lines);

        for (int i = 0; i < s.Lines.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string line = s.Lines[i];
            string? nextLine = (i + 1 < s.Lines.Length) ? s.Lines[i + 1] : null;

            if (HandleHtmlTableLine(s, line)) continue;
            if (HandlePageMarkerLine(s, line)) continue;
            if (HandleHeadingLine(s, line)) continue;

            HandleRegularLine(s, line, nextLine);

            if (ShouldFlushLargeBlock(s)) Flush(s);
        }

        Flush(s);

        if (s.Chunks.Count == 0 && !string.IsNullOrWhiteSpace(doc.Text))
        {
            s.Chunks.Add(new Chunk(
                Index: 0,
                Text: doc.Text.TrimEnd(),
                HeadingBreadcrumb: "",
                TopLevelSection: null,
                PageNumber: null,
                HeadingLevel: 0,
                ParentChunkIndex: null
            ));
        }

        return Task.FromResult<IReadOnlyList<Chunk>>(s.Chunks);
    }

    #endregion Public Methods

    #region Line Handlers

    private static bool HandleHeadingLine(State s, string line)
    {
        Match m = _headingRx.Match(line);
        if (!m.Success) return false;

        Flush(s);

        int level = m.Groups[1].Value.Length;         // count of '#'
        string heading = m.Groups[2].Value.Trim();

        PopStacksToLevelMinusOne(s, level);
        s.HeadingStack.Push(heading);
        s.CurrentHeadingLevel = level;

        s.PendingParentIndex = s.SectionIndexStack.Count > 0 ? s.SectionIndexStack.Peek() : null;
        s.WillPushAsSection = true;

        s.Buffer.AppendLine(line);
        return true;
    }

    private static bool HandleHtmlTableLine(State s, string line)
    {
        int opens = _htmlTableOpenRx.Matches(line).Count;
        int closes = _htmlTableCloseRx.Matches(line).Count;

        if (opens == 0 && closes == 0 && s.HtmlTableDepth == 0)
            return false;

        if (s.HtmlTableDepth == 0 && opens > 0)
            Flush(s); // start of a new table: finish prior block

        s.HtmlTableDepth += opens;
        s.Buffer.AppendLine(line);
        s.HtmlTableDepth -= closes;

        if (s.HtmlTableDepth <= 0)
        {
            s.HtmlTableDepth = 0;
            Flush(s); // table finished: emit as a single chunk
        }

        return true; // handled
    }

    private static bool HandlePageMarkerLine(State s, string line)
    {
        if (!_pageMarkerRx.IsMatch(line)) return false;
        s.CurrentPage++;
        return true;
    }

    private static void HandleRegularLine(State s, string line, string? nextLine)
    {
        s.Buffer.AppendLine(line);

        bool isPipeRow = IsPipeRow(line);
        bool nextIsPipeRow = IsPipeRow(nextLine);

        if (isPipeRow) s.InPipeTable = true;

        // End of a pipe-table block?
        if (s.InPipeTable && !nextIsPipeRow)
        {
            Flush(s);
            s.InPipeTable = false;
        }
    }

    #endregion Line Handlers

    #region Flush & Helpers

    private static string BuildBreadcrumb(Stack<string> headingStack) =>
        string.Join(" > ", headingStack.Reverse());

    private static void Flush(State s)
    {
        if (s.Buffer.Length == 0) return;

        string breadcrumb = BuildBreadcrumb(s.HeadingStack);
        // NOTE: If you want the TRUE top-level section, use First(); this keeps prior behavior (leaf).
        string? section = s.HeadingStack.Count > 0 ? s.HeadingStack.Last() : null;

        int? parentIdx = s.WillPushAsSection
            ? s.PendingParentIndex
            : (s.SectionIndexStack.Count > 0 ? s.SectionIndexStack.Peek() : null);

        var chunk = new Chunk(
            Index: s.Chunks.Count,
            Text: s.Buffer.ToString().TrimEnd(),
            HeadingBreadcrumb: breadcrumb,
            TopLevelSection: section,
            PageNumber: s.CurrentPage,
            HeadingLevel: s.CurrentHeadingLevel,
            ParentChunkIndex: parentIdx
        );

        s.Chunks.Add(chunk);
        s.Buffer.Clear();

        if (s.WillPushAsSection)
        {
            s.SectionIndexStack.Push(chunk.Index);
            s.WillPushAsSection = false;
            s.PendingParentIndex = null;
        }
    }

    private static bool IsPipeRow(string? line) =>
        line is not null && line.StartsWith('|');

    private static void PopStacksToLevelMinusOne(State s, int level)
    {
        while (s.HeadingStack.Count >= level)
        {
            s.HeadingStack.Pop();
            if (s.SectionIndexStack.Count >= level) s.SectionIndexStack.Pop();
        }
    }

    private static bool ShouldFlushLargeBlock(State s) =>
        !s.InPipeTable && s.HtmlTableDepth == 0 && s.Buffer.Length > _maxBlockSize;

    #endregion Flush & Helpers

    #region Regex Generators

    [GeneratedRegex(@"^(#+)\s+(.+)$", RegexOptions.Compiled)]
    private static partial Regex Heading();

    [GeneratedRegex(@"</table\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex HtmlTableClose();

    [GeneratedRegex(@"<table\b[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex HtmlTableOpen();

    [GeneratedRegex(@"^---$", RegexOptions.Compiled)]
    private static partial Regex PageMarker();

    #endregion Regex Generators

    #region Private Types

    private sealed class State(string[] lines)
    {
        #region Public Properties

        public StringBuilder Buffer { get; } = new(2048);
        public List<Chunk> Chunks { get; } = new(capacity: 512);
        public int CurrentHeadingLevel { get; set; } = 0;
        public int CurrentPage { get; set; } = 0;
        public Stack<string> HeadingStack { get; } = new();
        public int HtmlTableDepth { get; set; } = 0;
        public bool InPipeTable { get; set; } = false;
        public string[] Lines { get; } = lines;
        public int? PendingParentIndex { get; set; } = null;
        public Stack<int> SectionIndexStack { get; } = new();
        public bool WillPushAsSection { get; set; } = false;

        #endregion Public Properties
    }

    #endregion Private Types
}