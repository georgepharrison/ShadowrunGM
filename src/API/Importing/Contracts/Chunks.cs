namespace ShadowrunGM.API.Importing.Contracts;

public sealed record Chunk(
    int Index,
    string Text,
    string HeadingBreadcrumb,
    string? TopLevelSection,
    int? PageNumber,
    int HeadingLevel,
    int? ParentChunkIndex);

public sealed record LabeledChunk(
    int Index,
    string Text,
    string Label,
    string HeadingBreadcrumb,
    string? TopLevelSection,
    int? PageNumber,
    int HeadingLevel,
    int? ParentChunkIndex);