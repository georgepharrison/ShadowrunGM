using ShadowrunGM.API.Importing.Jobs;

namespace ShadowrunGM.API.Importing.Abstractions;

public interface IImportJobRepository
{
    #region Public Methods

    Task CreateRequestAsync(ImportJob job, CancellationToken cancellationToken = default);

    Task<ImportJob?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task MarkCompletedAsync(Guid id, CancellationToken cancellationToken = default);

    Task MarkFailedAsync(Guid id, Exception ex, CancellationToken cancellationToken = default);

    Task MarkInProgressAsync(Guid id, ImportStep step, CancellationToken cancellationToken = default);

    Task UpdateProgressAsync(Guid id, ImportStep step, int percent, string message, CancellationToken cancellationToken = default);

    #endregion Public Methods
}
