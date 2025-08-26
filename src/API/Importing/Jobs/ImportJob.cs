namespace ShadowrunGM.API.Importing.Jobs;

public sealed class ImportJob
{
    #region Private Constructors

    private ImportJob(Guid id)
    {
        Id = id;
        Status = ImportJobStatus.Requested;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    #endregion Private Constructors

    #region Public Methods

    public static ImportJob CreateNew(Guid id) =>
        new(id);

    public void MarkCompleted()
    {
        Status = ImportJobStatus.Completed;
        Percent = 100;
        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void MarkFailed(string code, string message)
    {
        Status = ImportJobStatus.Failed;
        ErrorCode = code;
        ErrorMessage = message;
        Attempts++;
    }

    public void MarkInProgress(ImportStep step)
    {
        Status = ImportJobStatus.InProgress;
        Step = step;
        StartedAt ??= DateTimeOffset.UtcNow;
    }

    public void UpdateProgress(ImportStep step, int percent)
    {
        Step = step;
        Percent = Math.Clamp(percent, 0, 100);
    }

    #endregion Public Methods

    #region Public Properties

    public int Attempts { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; init; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public Guid Id { get; init; }
    public int Percent { get; private set; }
    public DateTimeOffset? StartedAt { get; private set; }
    public ImportJobStatus Status { get; private set; }
    public ImportStep Step { get; private set; } = ImportStep.Parse;

    #endregion Public Properties
}