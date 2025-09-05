using HtmlAgilityPack;
using ShadowrunGM.API.Importing.Contracts;
using System.Text;

namespace ShadowrunGM.API.Importing.Chunking;

public static class HtmlTableSplitter
{
    #region Public Methods

    public static IReadOnlyList<Chunk> SplitTables(IReadOnlyList<Chunk> input)
    {
        if (input.Count == 0) return input;

        (List<Chunk> output, Dictionary<int, int> map) = ReindexOriginals(input);

        AppendChildrenForOriginals(output);

        return output;
    }

    #endregion Public Methods

    #region Private Methods

    private static (HtmlNode? HeaderThead, int ColumnCount, string? EmbeddedGroupName) AnalyzeHeaderThead(HtmlNode table, HtmlNodeCollection theads)
    {
        // Pick first THEAD that has a columns row (TR with >1 TH).
        foreach (HtmlNode thd in theads)
        {
            HtmlNodeCollection rows = thd.SelectNodes("./tr") ?? new HtmlNodeCollection(null);
            HtmlNode? columnsRow = rows.FirstOrDefault(r => CountCells(r, thOnly: true) > 1);
            if (columnsRow == null) continue;

            int colCount = CountCells(columnsRow, thOnly: true);
            string? embedded = FindEmbeddedHeaderBeforeColumns(rows, columnsRow, colCount);

            return (thd, colCount, embedded);
        }

        // Fallback if no proper THEAD
        return (null, GuessColCount(table), null);
    }

    private static void AppendChildrenForOriginals(List<Chunk> output)
    {
        int originals = output.Count; // important: capture BEFORE appending children
        for (int parentIdx = 0; parentIdx < originals; parentIdx++)
        {
            Chunk parent = output[parentIdx];
            if (LooksLikeSplitChild(parent)) continue;
            if (!ContainsHtmlTable(parent.Text)) continue;

            foreach (Chunk child in ExtractGroupChildren(parent))
            {
                int childIdx = output.Count;
                output.Add(child with { Index = childIdx, ParentChunkIndex = parentIdx });
            }
        }
    }

    private static string AppendLeaf(string breadcrumb, string leaf) =>
        string.IsNullOrWhiteSpace(breadcrumb) ? leaf : $"{breadcrumb} > {leaf}";

    private static Chunk BuildChildChunk(Chunk parent, HtmlNode? headerThead, TableGroup g)
    {
        StringBuilder sb = new StringBuilder(2048)
            .AppendLine($"### {g.Name}")
            .AppendLine("<table>");

        if (headerThead != null)
        {
            sb.AppendLine("<thead>")
              .AppendLine(headerThead.InnerHtml)
              .AppendLine("</thead>");
        }

        sb.AppendLine("<tbody>");
        foreach (HtmlNode tr in g.Rows) sb.AppendLine(tr.OuterHtml);
        sb.AppendLine("</tbody>")
          .AppendLine("</table>");

        return new Chunk(
            Index: -1,
            Text: sb.ToString().TrimEnd(),
            HeadingBreadcrumb: AppendLeaf(parent.HeadingBreadcrumb, g.Name),
            TopLevelSection: parent.TopLevelSection,
            PageNumber: parent.PageNumber,
            HeadingLevel: Math.Max(parent.HeadingLevel + 1, 1),
            ParentChunkIndex: parent.Index
        );
    }

    private static bool ContainsHtmlTable(string html) =>
        html.IndexOf("<table", StringComparison.OrdinalIgnoreCase) >= 0;

    private static int CountCells(HtmlNode tr, bool thOnly) =>
        tr.SelectNodes(thOnly ? "./th" : "./th|./td")?.Count ?? 0;

    private static IEnumerable<Chunk> ExtractGroupChildren(Chunk parent)
    {
        HtmlDocument doc = new();
        doc.LoadHtml(parent.Text);

        HtmlNodeCollection? tables = doc.DocumentNode.SelectNodes("//table");
        if (tables == null) yield break;

        foreach (HtmlNode table in tables)
        {
            HtmlNodeCollection theads = table.SelectNodes("./thead") ?? new HtmlNodeCollection(null);
            (HtmlNode? headerThead, int columnCount, string? embeddedGroupName) = AnalyzeHeaderThead(table, theads);

            List<TableGroup> groups = ParseGroups(table, headerThead, columnCount);

            if (groups.Count == 0 && !string.IsNullOrWhiteSpace(embeddedGroupName))
            {
                HtmlNodeCollection tbodyRows = table.SelectNodes(".//tbody/tr") ?? new HtmlNodeCollection(null);
                if (tbodyRows.Count > 0)
                {
                    TableGroup group = new(embeddedGroupName);
                    group.Rows.AddRange(tbodyRows.Cast<HtmlNode>());
                    groups.Add(group);
                }
            }

            foreach (TableGroup g in groups.Where(g => g.Rows.Count > 0))
                yield return BuildChildChunk(parent, headerThead, g);
        }
    }

    private static string? FindEmbeddedHeaderBeforeColumns(HtmlNodeCollection rows, HtmlNode columnsRow, int colCount)
    {
        foreach (HtmlNode r in rows)
        {
            if (ReferenceEquals(r, columnsRow)) break;
            HtmlNodeCollection cells = r.SelectNodes("./th|./td") ?? new HtmlNodeCollection(null);
            if (IsGroupHeaderRow(cells, colCount))
            {
                string name = Normalize(r.InnerText);
                if (!string.IsNullOrWhiteSpace(name)) return name;
            }
        }
        return null;
    }

    private static int GuessColCount(HtmlNode table)
    {
        int max = 0;
        foreach (HtmlNode tr in table.SelectNodes(".//tbody//tr") ?? Enumerable.Empty<HtmlNode>())
            max = Math.Max(max, CountCells(tr, thOnly: false));
        if (max == 0)
            foreach (HtmlNode tr in table.SelectNodes(".//tr") ?? Enumerable.Empty<HtmlNode>())
                max = Math.Max(max, CountCells(tr, thOnly: false));
        return Math.Max(max, 1);
    }

    private static bool IsGroupHeaderRow(HtmlNodeCollection cells, int columnCount)
    {
        if (cells == null || cells.Count == 0) return false;
        if (cells.Count == 1) return true;
        foreach (HtmlNode c in cells)
        {
            int cs = int.TryParse(c.GetAttributeValue("colspan", "1"), out int v) ? v : 1;
            if (cs >= columnCount) return true;
        }
        return false;
    }

    private static bool LooksLikeSplitChild(Chunk c) =>
        c.Text.Length >= 3 && c.Text.AsSpan(0, 3).SequenceEqual("###");

    private static int? MapParent(int? oldParent, Dictionary<int, int> map) =>
        oldParent is int p && map.TryGetValue(p, out int np) ? np : (int?)null;

    private static string Normalize(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        string t = HtmlEntity.DeEntitize(s);
        StringBuilder sb = new(t.Length);
        bool ws = false;
        foreach (char ch in t)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (!ws) sb.Append(' ');
                ws = true;
            }
            else
            {
                sb.Append(ch);
                ws = false;
            }
        }
        return sb.ToString().Trim();
    }

    private static List<TableGroup> ParseGroups(HtmlNode table, HtmlNode? headerThead, int columnCount)
    {
        List<TableGroup> result = [];
        bool headerSeen = headerThead == null;
        TableGroup? cur = null;

        foreach (HtmlNode node in table.ChildNodes)
        {
            if (node.Name.Equals("thead", StringComparison.OrdinalIgnoreCase))
            {
                if (!headerSeen && node == headerThead) { headerSeen = true; continue; }
                if (!headerSeen) continue; // ignore pre-header theads

                string name = Normalize(node.InnerText);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    cur = new TableGroup(name);
                    result.Add(cur);
                }
            }
            else if (node.Name.Equals("tbody", StringComparison.OrdinalIgnoreCase))
            {
                foreach (HtmlNode tr in node.SelectNodes("./tr") ?? new HtmlNodeCollection(null))
                {
                    HtmlNodeCollection cells = tr.SelectNodes("./th|./td") ?? new HtmlNodeCollection(null);

                    if (IsGroupHeaderRow(cells, columnCount))
                    {
                        string name = Normalize(tr.InnerText);
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            cur = new TableGroup(name);
                            result.Add(cur);
                        }
                        continue;
                    }

                    if (cur != null) cur.Rows.Add(tr);
                }
            }
        }
        return result;
    }

    private static (List<Chunk> Output, Dictionary<int, int> Map) ReindexOriginals(IReadOnlyList<Chunk> input)
    {
        List<Chunk> output = new(input.Count + 64);
        Dictionary<int, int> map = new(input.Count);

        for (int i = 0; i < input.Count; i++)
        {
            Chunk c = input[i];
            int newIdx = output.Count;
            map[c.Index] = newIdx;

            output.Add(c with
            {
                Index = newIdx,
                ParentChunkIndex = MapParent(c.ParentChunkIndex, map)
            });
        }
        return (output, map);
    }

    #endregion Private Methods

    #region Private Classes

    private sealed class TableGroup(string name)
    {
        #region Public Properties

        public string Name { get; } = name;
        public List<HtmlNode> Rows { get; } = [];

        #endregion Public Properties
    }

    #endregion Private Classes
}