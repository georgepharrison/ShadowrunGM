using ShadowrunGM.API.Importing.Abstractions;
using ShadowrunGM.API.Importing.Contracts;
using System.Text.RegularExpressions;

namespace ShadowrunGM.API.Importing.Classification;

public sealed class HeuristicClassifier : IClassifier
{
    #region Private Members

    private static readonly Regex RxDrain = new(@"\bDRAIN\b\s*[:=]?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RxSpell = new(@"\bTYPE\b.*\bRANGE\b.*\bDURATION\b", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RxWeapon = new(@"\b(DV|DAMAGE|AP|MODE|MODES|AR|AMMO)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    #endregion Private Members

    #region Public Methods

    public Task<IReadOnlyList<LabeledChunk>> ClassifyAsync(IReadOnlyList<Chunk> chunks, CancellationToken ct)
    {
        List<LabeledChunk> labeled = new(chunks.Count);

        foreach (Chunk c in chunks)
        {
            ct.ThrowIfCancellationRequested();

            // Fast heading hints
            string hb = c.HeadingBreadcrumb ?? string.Empty;
            string top = c.TopLevelSection ?? string.Empty;

            bool headingSuggestsSpell = hb.Contains("Spell", StringComparison.OrdinalIgnoreCase) || top.Contains("Magic", StringComparison.OrdinalIgnoreCase);
            bool headingSuggestsWeapon = hb.Contains("Weapon", StringComparison.OrdinalIgnoreCase) || hb.Contains("Ranged", StringComparison.OrdinalIgnoreCase)
                                         || top.Contains("Combat", StringComparison.OrdinalIgnoreCase);

            // Text heuristics
            string t = c.Text;

            // Spell: TYPE/RANGE/DURATION block or has DRAIN, often in spell stat blocks
            if (RxSpell.IsMatch(t) || headingSuggestsSpell && RxDrain.IsMatch(t))
            {
                labeled.Add(new LabeledChunk(c.Index, c.Text, "spell", c.HeadingBreadcrumb, c.TopLevelSection, c.PageNumber, c.HeadingLevel, c.ParentChunkIndex));
                continue;
            }

            // Weapon: typical DV/AP/MODE keywords or weapon-y headings
            if (RxWeapon.IsMatch(t) || headingSuggestsWeapon)
            {
                labeled.Add(new LabeledChunk(c.Index, c.Text, "weapon", c.HeadingBreadcrumb, c.TopLevelSection, c.PageNumber, c.HeadingLevel, c.ParentChunkIndex));
                continue;
            }

            // Fallback
            labeled.Add(new LabeledChunk(c.Index, c.Text, "Unknown", c.HeadingBreadcrumb, c.TopLevelSection, c.PageNumber, c.HeadingLevel, c.ParentChunkIndex));
        }

        return Task.FromResult<IReadOnlyList<LabeledChunk>>(labeled);
    }

    #endregion Public Methods
}
