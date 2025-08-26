namespace ShadowrunGM.API.Importing.Contracts;

public sealed record HeadingHit(int Level, string Text, int LineIndex);

public sealed record PageMark(int PageNumber, int LineIndex);

public sealed record ParsedDoc(
    string BlobUri,
    string Text,
    string ContentType,
    string[] Lines,
    IReadOnlyList<HeadingHit> Headings,
    IReadOnlyList<PageMark> PageMarks);