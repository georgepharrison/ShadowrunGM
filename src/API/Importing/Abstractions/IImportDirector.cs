using ShadowrunGM.API.Importing.Contracts;

namespace ShadowrunGM.API.Importing.Abstractions;

public interface IImportDirector
{
    #region Public Methods

    Task<ImportOutcome> RunAsync(ImportWorkItem item, IImportBuilder builder, Func<ImportProgress, Task> report, CancellationToken cancellationToken = default);

    #endregion Public Methods
}