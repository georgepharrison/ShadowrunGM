using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShadowrunGM.API.Common.Util;
using ShadowrunGM.API.Importing.Abstractions;
using ShadowrunGM.API.Importing.Contracts;
using ShadowrunGM.API.Importing.Jobs;
using ShadowrunGM.API.Infrastructure;
using ShadowrunGM.API.Infrastructure.Entities.Catalog;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mime;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ShadowrunGM.API.Endpoints;

public interface IGameItemExtractor
{
    #region Public Methods

    bool CanHandle(string label);             // e.g., "weapon","gear","augmentation","armor","vehicle","drone","focus"

    bool TryExtract(LabeledChunk c, long sourcebookId, [NotNullWhen(returnValue: true)] out GameItemDraft? gameItem);

    #endregion Public Methods
}

public interface IGameItemPersister
{
    #region Public Methods

    Task<int> UpsertAsync(IEnumerable<GameItemDraft> drafts, CancellationToken ct = default);

    #endregion Public Methods
}

public interface IMagicAbilityExtractor
{
    #region Public Methods

    bool CanHandle(string label);             // e.g., "spell","adeptPower","ritual","metamagic"

    bool TryExtract(LabeledChunk c, long sourcebookId, [NotNullWhen(returnValue: true)] out MagicAbilityDraft? magicAbility);

    #endregion Public Methods
}

public interface IMagicAbilityPersister
{
    #region Public Methods

    Task<int> UpsertAsync(IEnumerable<MagicAbilityDraft> drafts, CancellationToken ct = default);

    #endregion Public Methods
}

public static class ImportEndpoints
{
    #region Public Methods

    public static IEndpointRouteBuilder MapImportEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/import");

        group.MapPost("/rulebook", ImportRulebookAsync())
            .DisableAntiforgery()
            .WithName("ImportRulebook")
            .Accepts<IFormFile>(MediaTypeNames.Multipart.FormData);

        group.MapGet("/jobs/{id:guid}", GetImportJobAsync())
            .WithName("GetImportJob");

        return app;
    }

    #endregion Public Methods

    #region Private Methods

    private static Func<Guid, IImportJobRepository, CancellationToken, Task<IResult>> GetImportJobAsync() =>
        async (id, jobRepository, cancellationToken) =>
        {
            ImportJob? job = await jobRepository.GetAsync(id, cancellationToken);
            return job is null ? Results.NotFound() : Results.Ok(new
            {
                job.Id,
                Status = job.Status.ToString(),
                Step = job.Step.ToString(),
                job.Percent,
                job.Attempts,
                job.CreatedAt,
                job.StartedAt,
                job.CompletedAt,
                job.ErrorCode,
                job.ErrorMessage
            });
        };

    private static Func<IFormFile, string, string, string, int?, IBlobStorage, IImportQueue, IImportJobRepository, HttpContext, CancellationToken, Task<Microsoft.AspNetCore.Http.IResult>> ImportRulebookAsync() =>
        async (file, [FromForm] code, [FromForm] title, [FromForm] edition, [FromForm] year, blobStorage, importQueue, jobRepository, httpContext, cancellationToken) =>
        {
            if (file is null || file.Length is 0)
            {
                return Results.BadRequest("Empty file.");
            }

            if (file.Length > 100 * 1024 * 1024)
            {
                return Results.BadRequest("File too large. Maximum size is 100MB.");
            }

            await using Stream stream = file.OpenReadStream();
            (string blobUri, string sha256) = await blobStorage.StoreAsync(stream, file.FileName, cancellationToken);

            Guid jobId = Guid.NewGuid();
            ImportJob job = ImportJob.CreateNew(jobId);
            await jobRepository.CreateRequestAsync(job, cancellationToken);

            string traceId = httpContext.TraceIdentifier;
            string userId = httpContext.User?.Identity?.Name ?? "anonymous";
            string? tennantId = httpContext.Request.Headers["X-Tenant-Id"].ToString();

            ImportWorkItem workItem = new(
                JobId: jobId,
                SourceFilename: file.FileName,
                BlobUri: blobUri,
                ContentHash: sha256,
                SubmittedByUserId: userId,
                TenantId: tennantId,
                TraceId: traceId,
                Code: code,
                Title: title,
                Edition: edition,
                Year: year);

            await importQueue.EnqueueAsync(workItem, cancellationToken);

            return Results.Accepted($"/import/jobs/{jobId}", new { jobId });
        };

    #endregion Private Methods
}

public sealed class EfGameItemPersister(ShadowrunContext db) : IGameItemPersister
{
    #region Private Members

    private readonly ShadowrunContext _db = db;

    #endregion Private Members

    #region Public Methods

    public async Task<int> UpsertAsync(IEnumerable<GameItemDraft> drafts, CancellationToken ct = default)
    {
        // 1) materialize + de-dupe by slug
        List<GameItemDraft> list = [.. drafts
            .Where(d => d is not null)
            .GroupBy(d => d.Slug, StringComparer.Ordinal)
            .Select(g => g.First())];
        if (list.Count == 0) return 0;

        // 2) load existing by slug
        string[] slugs = [.. list.Select(d => d.Slug).Distinct()];
        List<GameItem> existing = await _db.Set<GameItem>()
            .Where(x => Enumerable.Contains(slugs, x.Slug))
            .ToListAsync(ct);

        // 3) map includes DB rows; we'll also add new rows as we create them
        Dictionary<string, GameItem> map = existing.ToDictionary(x => x.Slug, x => x, StringComparer.Ordinal);
        int changes = 0;

        foreach (GameItemDraft d in list)
        {
            if (!map.TryGetValue(d.Slug, out GameItem? row))
            {
                row = new GameItem
                {
                    Name = d.Name,
                    ItemType = d.ItemType,
                    Category = d.Category,
                    SourcebookId = d.SourcebookId,
                    Page = d.Page,
                    Slug = d.Slug,
                    StatsJson = JsonSerializer.Serialize(d.Stats),
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _db.Add(row);
                map[d.Slug] = row;            // <— make this visible to the loop
                changes++;
                continue;
            }

            bool dirty = false;
            if (row.Name != d.Name) { row.Name = d.Name; dirty = true; }
            if (row.ItemType != d.ItemType) { row.ItemType = d.ItemType; dirty = true; }
            if (row.Category != d.Category) { row.Category = d.Category; dirty = true; }
            if (row.SourcebookId != d.SourcebookId) { row.SourcebookId = d.SourcebookId; dirty = true; }
            if (row.Page != d.Page) { row.Page = d.Page; dirty = true; }

            string stats = JsonSerializer.Serialize(d.Stats);
            if (!string.Equals(row.StatsJson, stats, StringComparison.Ordinal))
            { row.StatsJson = stats; dirty = true; }

            if (dirty) { row.UpdatedAt = DateTimeOffset.UtcNow; changes++; }
        }

        if (changes > 0) await _db.SaveChangesAsync(ct);
        return changes;
    }

    #endregion Public Methods
}

public sealed class EfMagicAbilityPersister(ShadowrunContext db) : IMagicAbilityPersister
{
    #region Private Members

    private readonly ShadowrunContext _db = db;

    #endregion Private Members

    #region Public Methods

    public async Task<int> UpsertAsync(IEnumerable<MagicAbilityDraft> drafts, CancellationToken ct = default)
    {
        List<MagicAbilityDraft> list = [.. drafts
            .Where(d => d is not null)
            .GroupBy(d => d.Slug, StringComparer.Ordinal)
            .Select(g => g.First())];
        if (list.Count == 0) return 0;

        string[] slugs = [.. list.Select(d => d.Slug).Distinct()];
        List<MagicAbility> existing = await _db.Set<MagicAbility>()
            .Where(x => Enumerable.Contains(slugs, x.Slug))
            .ToListAsync(ct);

        Dictionary<string, MagicAbility> map = existing.ToDictionary(x => x.Slug, x => x, StringComparer.Ordinal);
        int changes = 0;

        foreach (MagicAbilityDraft d in list)
        {
            if (!map.TryGetValue(d.Slug, out MagicAbility? row))
            {
                row = new MagicAbility
                {
                    Slug = d.Slug,
                    Name = d.Name,
                    AbilityType = d.AbilityType,
                    Category = d.Category,
                    Range = d.Range,
                    Duration = d.Duration,
                    Type = d.Type,
                    DrainValue = d.DrainValue,
                    PowerPointCost = d.PowerPointCost,
                    SourcebookId = d.SourcebookId,
                    Page = d.Page,
                    ExtraJson = JsonSerializer.Serialize(d.Extra),
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _db.Add(row);
                map[d.Slug] = row;            // <— add to map so dupes don’t insert again
                changes++;
                continue;
            }

            bool dirty = false;
            void Upd<T>(T current, T incoming, Action apply)
            {
                if (!EqualityComparer<T>.Default.Equals(current, incoming)) { apply(); dirty = true; }
            }

            Upd(row.Name, d.Name, () => row.Name = d.Name);
            Upd(row.AbilityType, d.AbilityType, () => row.AbilityType = d.AbilityType);
            Upd(row.Category, d.Category, () => row.Category = d.Category);
            Upd(row.Range, d.Range, () => row.Range = d.Range);
            Upd(row.Duration, d.Duration, () => row.Duration = d.Duration);
            Upd(row.Type, d.Type, () => row.Type = d.Type);
            Upd(row.DrainValue, d.DrainValue, () => row.DrainValue = d.DrainValue);
            Upd(row.PowerPointCost, d.PowerPointCost, () => row.PowerPointCost = d.PowerPointCost);
            Upd(row.SourcebookId, d.SourcebookId, () => row.SourcebookId = d.SourcebookId);
            Upd(row.Page, d.Page, () => row.Page = d.Page);

            string extra = JsonSerializer.Serialize(d.Extra);
            Upd(row.ExtraJson, extra, () => row.ExtraJson = extra);

            if (dirty) { row.UpdatedAt = DateTimeOffset.UtcNow; changes++; }
        }

        if (changes > 0) await _db.SaveChangesAsync(ct);
        return changes;
    }

    #endregion Public Methods
}

public sealed class SpellExtractor : IMagicAbilityExtractor
{
    #region Public Methods

    public bool CanHandle(string label) => label.Equals("spell", StringComparison.OrdinalIgnoreCase);

    public bool TryExtract(LabeledChunk c, long sourcebookId, [NotNullWhen(true)] out MagicAbilityDraft? magicAbility)
    {
        magicAbility = null;

        string name = ExtractorHelpers.FirstNonEmpty(ExtractorHelpers.FromBreadcrumb(c.HeadingBreadcrumb), ExtractorHelpers.FirstLine(c.Text)) ?? "";
        if (string.IsNullOrWhiteSpace(name)) return false;

        Dictionary<string, object?> extra = new(StringComparer.OrdinalIgnoreCase);

        string? type = CanonType(ExtractorHelpers.PickAfter(c.Text, "TYPE"));
        string? range = Token(ExtractorHelpers.PickAfter(c.Text, "RANGE"), 50);
        string? duration = Token(ExtractorHelpers.PickAfter(c.Text, "DURATION"), 50);
        int? drain = ParseIntSafe(ExtractorHelpers.PickAfter(c.Text, "DRAIN"));

        magicAbility = new(
            Name: name.Trim(),
            AbilityType: "Spell",
            Category: InferSpellCategory(c.Text),
            Range: range,
            Duration: duration,
            Type: type,
            DrainValue: drain,
            PowerPointCost: null,
            SourcebookId: sourcebookId,
            Page: c.PageNumber,
            Slug: ExtractorHelpers.Slugify("spell", name),
            Extra: extra,
            SourceHash: Hashing.MD5Hex(c.Text)
        );
        return true;
    }

    #endregion Public Methods

    #region Private Methods

    private static string? CanonType(string? s)
    {
        string? t = Token(s, 10);
        if (string.IsNullOrEmpty(t)) return null;
        // normalize common spell type forms -> M/P
        if (t.StartsWith("M", StringComparison.OrdinalIgnoreCase) || t.StartsWith("Mana", StringComparison.OrdinalIgnoreCase)) return "M";
        if (t.StartsWith("P", StringComparison.OrdinalIgnoreCase) || t.StartsWith("Phys", StringComparison.OrdinalIgnoreCase)) return "P";
        return t.Length > 10 ? t[..10] : t;
    }

    private static string? InferSpellCategory(string text) =>
            null;

    private static int? ParseIntSafe(string? s) =>
        int.TryParse(new string([.. (s ?? "").Where(char.IsDigit)]), out int n) ? n : null;

    private static string? Token(string? s, int max = 50)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        // take first token before comma/semicolon/pipe or double-space, then trim
        string t = Regex.Split(s, @"[,;|]|\s{2,}")[0].Trim();
        // also cut off anything after first space for true "token" columns
        string? first = t.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(first)) first = t;
        return first.Length > max ? first[..max] : first;
    }

    #endregion Private Methods
}

public sealed class WeaponExtractor : IGameItemExtractor
{
    #region Public Methods

    public bool CanHandle(string label) => label.Equals("weapon", StringComparison.OrdinalIgnoreCase);

    public bool TryExtract(LabeledChunk c, long sourcebookId, [NotNullWhen(returnValue: true)] out GameItemDraft? gameItem)
    {
        gameItem = null;

        string name = ExtractorHelpers.FirstNonEmpty(ExtractorHelpers.FromBreadcrumb(c.HeadingBreadcrumb), ExtractorHelpers.FirstLine(c.Text)) ?? "";
        if (string.IsNullOrWhiteSpace(name)) return false;

        Dictionary<string, object?> stats = new(StringComparer.OrdinalIgnoreCase)
        {
            // very light heuristics for common fields (refine later)
            ["damage"] = ExtractorHelpers.PickAfter(c.Text, "DV") ?? ExtractorHelpers.PickAfter(c.Text, "DAMAGE"),
            ["ap"] = ExtractorHelpers.PickAfter(c.Text, "AP"),
            ["modes"] = ExtractorHelpers.PickAfter(c.Text, "MODE") ?? ExtractorHelpers.PickAfter(c.Text, "MODES")
        };

        gameItem = new(
            Name: name.Trim(),
            ItemType: "Weapon",
            Category: InferWeaponCategory(c.HeadingBreadcrumb, c.Text),
            SourcebookId: sourcebookId,
            Page: c.PageNumber,
            Slug: ExtractorHelpers.Slugify("weapon", name),
            Stats: stats,
            SourceHash: Hashing.MD5Hex(c.Text)
        );

        return true;
    }

    #endregion Public Methods

    #region Private Methods

    private static string? InferWeaponCategory(string breadcrumb, string text) =>
        breadcrumb.Contains("Pistol", StringComparison.OrdinalIgnoreCase) ? "Pistol" : null;

    #endregion Private Methods
}

internal static class ExtractorHelpers
{
    #region Public Methods

    public static string? FirstLine(string text) =>
        text.Split('\n').FirstOrDefault()?.Trim();

    public static string? FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

    public static string? FromBreadcrumb(string bc) =>
        string.IsNullOrWhiteSpace(bc) ? null : bc.Split('>').Last().Trim();

    public static int? ParseIntSafe(string? s) =>
        int.TryParse(new string([.. (s ?? "").Where(char.IsDigit)]), out int n) ? n : null;

    public static string? PickAfter(string text, string label)
    {
        int idx = text.IndexOf(label, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;
        string rest = text[(idx + label.Length)..];
        int colon = rest.IndexOf(':'); if (colon >= 0) rest = rest[(colon + 1)..];
        string? line = rest.Split('\n', '\r').FirstOrDefault()?.Trim();
        return string.IsNullOrWhiteSpace(line) ? null : line;
    }

    public static string Slugify(string kind, string name) =>
        $"{kind}:{name}".ToLowerInvariant().Replace(' ', '-');

    #endregion Public Methods
}

public sealed record GameItemDraft(
    string Name,
    string ItemType,
    string? Category,
    long SourcebookId,
    int? Page,
    string Slug,
    IReadOnlyDictionary<string, object?> Stats, // becomes StatsJson
    string SourceHash);

public sealed record MagicAbilityDraft(
    string Name,
    string AbilityType,
    string? Category,
    string? Range,
    string? Duration,
    string? Type,
    int? DrainValue,
    decimal? PowerPointCost,
    long SourcebookId,
    int? Page,
    string Slug,
    IReadOnlyDictionary<string, object?> Extra,
    string SourceHash);