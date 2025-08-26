namespace ShadowrunGM.API.Importing.Jobs;

public enum ImportJobStatus
{
    Requested,
    InProgress,
    Completed,
    Failed
}

public enum ImportStep
{
    Parse,
    Chunk,
    Classify,
    Persist,
    EmbedIndex,
    Finalize
}