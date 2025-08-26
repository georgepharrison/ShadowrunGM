using ShadowrunGM.API.Importing.Abstractions;
using System.Collections.Concurrent;

namespace ShadowrunGM.API.Importing.Jobs;

public sealed class InMemoryImportJobRepository : IImportJobRepository
{
    #region Private Members

    private readonly ConcurrentDictionary<Guid, ImportJob> _store = [];

    #endregion Private Members

    #region Public Methods

    public Task CreateRequestAsync(ImportJob job, CancellationToken cancellationToken = default)
    {
        _store[job.Id] = job;
        return Task.CompletedTask;
    }

    public Task<ImportJob?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out ImportJob? job);
        return Task.FromResult(job);
    }

    public Task MarkCompletedAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(id, out ImportJob? job))
        {
            job.MarkCompleted();
        }
        return Task.CompletedTask;
    }

    public Task MarkFailedAsync(Guid id, Exception ex, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(id, out ImportJob? job))
        {
            job.MarkFailed(ex.GetType().Name, ex.Message);
        }
        return Task.CompletedTask;
    }

    public Task MarkInProgressAsync(Guid id, ImportStep step, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(id, out ImportJob? job))
        {
            job.MarkInProgress(step);
        }
        return Task.CompletedTask;
    }

    public Task UpdateProgressAsync(Guid id, ImportStep step, int percent, string message, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(id, out ImportJob? job))
        {
            job.UpdateProgress(step, percent);
        }
        return Task.CompletedTask;
    }

    #endregion Public Methods
}