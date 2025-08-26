namespace ShadowrunGM.API.Importing.Jobs;

public sealed class ImportStepException(ImportStep step, string message) : Exception($"Error during import step '{step}': {message}")
{
    #region Public Properties

    public string Step { get; init; } = step.ToString();

    #endregion Public Properties
}