using Microsoft.EntityFrameworkCore;
using ShadowrunGM.API.Common.Util;
using ShadowrunGM.API.Importing.Abstractions;
using ShadowrunGM.API.Importing.Contracts;
using ShadowrunGM.API.Infrastructure;
using ShadowrunGM.API.Infrastructure.Entities.Import;
using System.Text.RegularExpressions;

namespace ShadowrunGM.API.Importing.Embedding;

public sealed class EfEmbedderIndexer(ShadowrunContext db, ILogger<EfEmbedderIndexer> log, ITextEmbeddingProvider emb) : IEmbedderIndexer
{
    #region Private Members

    private readonly ShadowrunContext _db = db;
    private readonly ITextEmbeddingProvider _emb = emb;
    private readonly ILogger<EfEmbedderIndexer> _log = log;

    #endregion Private Members

    #region Public Methods

    public async Task<EmbeddingResult> IndexAsync(Guid jobId, PersistResult persisted, IReadOnlyList<LabeledChunk> labeled, CancellationToken cancellationToken = default)
    {
        // We’ll scope updates to the sourcebook we just persisted
        long sourcebookId = persisted.SourcebookId;

        // Map labeled chunks -> SourceHash for join
        string[] hashes = labeled.Select(c => Hashing.MD5Hex(c.Text)).Distinct().ToArray();

        // Pull the rows we just saved for that sourcebook
        var rows = await _db.RuleContents
            .AsNoTracking()
            .Where(rc => rc.SourcebookId == sourcebookId
                && Enumerable.Contains(hashes, rc.SourceHash)
                && (rc.Embedding == null
                    || rc.EmbeddingModel != _emb.ModelName
                    || rc.EmbeddingVersion != _emb.VersionTag))
            .Select(rc => new { rc.Id, rc.Heading, rc.Content })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return new EmbeddingResult(0);
        }

        // Normalize text
        List<(long Id, string Text)> work = [.. rows
            .Select(r => (r.Id, Text: NormalizeForEmbedding(r.Heading, r.Content)))
            .Where(x => !string.IsNullOrWhiteSpace(x.Text))];

        if (work.Count == 0)
        {
            return new EmbeddingResult(0);
        }

        const int batchSize = 64;
        int updated = 0;

        for (int i = 0; i < work.Count; i += batchSize)
        {
            cancellationToken.ThrowIfCancellationRequested();
            List<(long Id, string Text)> batch = [.. work.Skip(i).Take(batchSize)];

            float[][] vectors;
            try
            {
                vectors = await _emb.EmbedAsync(batch.Select(b => b.Text), cancellationToken);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Embedding failed for ids [{Ids}]", string.Join(",", batch.Select(b => b.Id)));
                continue;
            }

            if (vectors.Length != batch.Count)
            {
                _log.LogWarning("Embedding count mismatch: expected {Exp} got {Got} (ids [{Ids}])",
                    batch.Count, vectors.Length, string.Join(",", batch.Select(b => b.Id)));
                continue;
            }

            // Reload for update
            long[] ids = batch.Select(b => b.Id)
                .ToArray();

            List<RuleContent> tracked = await _db.RuleContents
                .Where(rc => Enumerable.Contains(ids, rc.Id))
                .ToListAsync(cancellationToken);

            for (int k = 0; k < batch.Count; k++)
            {
                long id = batch[k].Id;
                RuleContent? row = tracked.FirstOrDefault(t => t.Id == id);
                if (row is null) continue;

                row.Embedding = new Pgvector.Vector(vectors[k]);
                row.EmbeddingModel = _emb.ModelName;
                row.EmbeddingVersion = _emb.VersionTag;
            }

            await _db.SaveChangesAsync(cancellationToken);

            updated += batch.Count;
        }

        return new EmbeddingResult(updated);
    }

    #endregion Public Methods

    #region Private Methods

    private static string NormalizeForEmbedding(string? heading, string? content)
    {
        string h = string.IsNullOrWhiteSpace(heading) ? "" : $"# {heading.Trim()}\n\n";
        string c = (content ?? "").Replace("\r", "").Trim();
        c = Regex.Replace(c, @"[ \t]+", " ");
        c = Regex.Replace(c, @"\n{3,}", "\n\n");
        return h + c;
    }

    #endregion Private Methods
}