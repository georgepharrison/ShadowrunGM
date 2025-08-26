using Microsoft.EntityFrameworkCore;
using ShadowrunGM.API.Importing.Abstractions;
using ShadowrunGM.API.Importing.Contracts;
using ShadowrunGM.API.Infrastructure;
using ShadowrunGM.API.Infrastructure.Entities.Import;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ShadowrunGM.API.Importing.Persistence;

public sealed class EfStructuredPersister(ShadowrunContext db, ILogger<EfStructuredPersister> log) : IStructuredPersister
{
    #region Private Members

    private readonly ShadowrunContext _db = db;
    private readonly ILogger<EfStructuredPersister> _log = log;

    #endregion Private Members

    #region Public Methods

    public async Task<PersistResult> SaveAsync(ImportWorkItem item, IReadOnlyList<LabeledChunk> labeled, CancellationToken cancellationToken)
    {
        long sbId = await EnsureSourcebookAsync(item, cancellationToken);

        Dictionary<int, string> hashByChunk = labeled.ToDictionary(c => c.Index, c => MD5Hex(c.Text));

        string[] allHashes = labeled
            .Select(c => MD5Hex(c.Text))
            .Distinct()
            .ToArray();

        var existingRows = await _db.Set<RuleContent>()
            .AsNoTracking()
            .Where(rc => rc.SourcebookId == sbId && Enumerable.Contains(allHashes, rc.SourceHash))
            .Select(rc => new { rc.Id, rc.SourceHash })
            .ToListAsync(cancellationToken);

        Dictionary<string, long> existingByHash = existingRows.ToDictionary(x => x.SourceHash, x => x.Id);
        HashSet<string> existingHashes = existingByHash.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        int? maxSeq = await _db.Set<RuleContent>()
            .Where(rc => rc.SourcebookId == sbId)
            .Select(rc => (int?)rc.SequenceNumber)
            .MaxAsync(cancellationToken);

        int seq = maxSeq ?? 0;
        int inserted = 0;

        Dictionary<int, long> idByChunk = [];

        foreach (LabeledChunk ch in labeled)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string chHash = hashByChunk[ch.Index];

            if (existingHashes.Contains(chHash))
            {
                if (existingByHash.TryGetValue(chHash, out long existingId))
                {
                    idByChunk[ch.Index] = existingId;
                }
                continue;
            }

            long? parentId = null;
            if (ch.ParentChunkIndex is int p)
            {
                if (idByChunk.TryGetValue(p, out long pid))
                {
                    parentId = pid;
                }
                else
                {
                    if (hashByChunk.TryGetValue(p, out string? parentHash) &&
                        existingByHash.TryGetValue(parentHash, out long existingPid))
                    {
                        parentId = existingPid;
                    }
                }
            }

            string headingTitle = ch.HeadingBreadcrumb is { Length: > 0 } s && s.Contains(" > ")
                ? s.Split(" > ").Last()
                : ch.HeadingBreadcrumb;

            RuleContent rc = new()
            {
                SourcebookId = sbId,
                Content = ch.Text,
                Heading = ch.HeadingBreadcrumb,
                HeadingTitle = headingTitle,
                HeadingLevel = ch.HeadingLevel,
                Section = ch.TopLevelSection,
                ContentType = DetectContentType(ch.Text),
                PageNumber = ch.PageNumber,
                SequenceNumber = ++seq,
                ParentContentId = parentId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.Set<RuleContent>().Add(rc);
            try
            {
                await _db.SaveChangesAsync(cancellationToken);
                inserted++;
                idByChunk[ch.Index] = rc.Id;

                existingByHash[chHash] = rc.Id;
                existingHashes.Add(chHash);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                _db.Entry(rc).State = EntityState.Detached;
                if (existingByHash.TryGetValue(chHash, out long eid))
                    idByChunk[ch.Index] = eid;
            }
        }

        _log.LogInformation("Persisted {Count} rule chunks (job {JobId}).", inserted, item.JobId);
        return new PersistResult(inserted, sbId);
    }

    #endregion Public Methods

    #region Private Methods

    private static string DetectContentType(string content)
    {
        if (content.Contains('|')) return "table";
        if (Regex.IsMatch(content, @"\bTYPE\b.*\bRANGE\b", RegexOptions.Singleline)) return "spell";
        return "text";
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        Exception? e = ex.InnerException;
        while (e is not null)
        {
            string? sqlState = e.GetType().GetProperty("SqlState")?.GetValue(e)?.ToString();
            if (sqlState == "23505") return true;
            e = e.InnerException;
        }
        return false;
    }

    private static string MD5Hex(string text)
    {
        using MD5 md5 = MD5.Create();
        byte[] data = Encoding.UTF8.GetBytes(text);
        byte[] hash = md5.ComputeHash(data);
        return ToHex(hash);
    }

    private static string ToHex(ReadOnlySpan<byte> bytes)
    {
        StringBuilder sb = new(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++) sb.Append(bytes[i].ToString("x2"));
        return sb.ToString();
    }

    private async Task<long> EnsureSourcebookAsync(ImportWorkItem item, CancellationToken ct)
    {
        DbSet<Sourcebook> books = _db.Set<Sourcebook>();
        Sourcebook? book = await books.FirstOrDefaultAsync(b => b.Code == item.Code, ct);

        if (book is null)
        {
            book = new Sourcebook
            {
                Code = item.Code,
                Title = item.Title,
                Edition = item.Edition,
                Year = item.Year,
                FileName = item.SourceFilename,
                FileHash = item.ContentHash,
                CreatedUtc = DateTime.UtcNow,
                UpdatedUtc = DateTime.UtcNow,
                ImportedAt = DateTimeOffset.UtcNow
            };
            books.Add(book);
            await _db.SaveChangesAsync(ct);
            return book.Id;
        }

        bool dirty = false;
        if (book.Title != item.Title) { book.Title = item.Title; dirty = true; }
        if (book.Edition != item.Edition) { book.Edition = item.Edition; dirty = true; }
        if (book.Year != item.Year) { book.Year = item.Year; dirty = true; }
        if (book.FileName != item.SourceFilename) { book.FileName = item.SourceFilename; dirty = true; }
        if (!string.Equals(book.FileHash, item.ContentHash, StringComparison.OrdinalIgnoreCase))
        { book.FileHash = item.ContentHash; dirty = true; }

        if (dirty)
        {
            book.UpdatedUtc = DateTime.UtcNow;
            book.ImportedAt = DateTimeOffset.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        return book.Id;
    }

    #endregion Private Methods
}
