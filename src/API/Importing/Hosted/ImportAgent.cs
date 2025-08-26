using Microsoft.Extensions.AI;
using ShadowrunGM.API.Common.Timing;
using ShadowrunGM.API.Importing.Abstractions;
using ShadowrunGM.API.Importing.Contracts;
using ShadowrunGM.API.Importing.Jobs;

namespace ShadowrunGM.API.Importing.Hosted;

public sealed class ImportAgent(ILogger<ImportAgent> log, IImportQueue queue, IServiceProvider services) : BackgroundService
{
    #region Private Members

    private readonly ILogger<ImportAgent> _log = log;
    private readonly IImportQueue _queue = queue;
    private readonly IServiceProvider _services = services;

    #endregion Private Members

    #region Protected Methods

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Import agent started.");

        await foreach (ImportWorkItem workItem in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            _ = ProcessOneAsync(workItem, stoppingToken);
        }

        _log.LogInformation("Import agent stopping (queue drained).");
    }

    #endregion Protected Methods

    #region Private Methods

    private async Task ProcessOneAsync(ImportWorkItem workItem, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _services.CreateScope();
        IImportJobRepository jobRepository = scope.ServiceProvider.GetRequiredService<IImportJobRepository>();
        IImportDirector director = scope.ServiceProvider.GetRequiredService<IImportDirector>();
        IImportBuilderFactory builderFactory = scope.ServiceProvider.GetRequiredService<IImportBuilderFactory>();

        ImportJob? job = await jobRepository.GetAsync(workItem.JobId, cancellationToken);
        if (job is null)
        {
            _log.LogWarning("Import job {JobId} not found; skipping.", workItem.JobId);
            return;
        }

        ValueStopwatch stopwatch = ValueStopwatch.StartNew();
        try
        {
            await jobRepository.MarkInProgressAsync(workItem.JobId, ImportStep.Parse, cancellationToken);

            Task Report(ImportProgress progress) =>
                jobRepository.UpdateProgressAsync(workItem.JobId, progress.Step, progress.Percent, progress.Message, cancellationToken);

            IImportBuilder builder = builderFactory.CreateFor(workItem, scope.ServiceProvider);

            await director.RunAsync(workItem, builder, Report, cancellationToken);

            await jobRepository.MarkCompletedAsync(workItem.JobId, cancellationToken);

            _log.LogInformation("Import {JobId} completed in {Elapsed}ms.", workItem.JobId, stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _log.LogInformation("Import job {JobId} cancelled.", workItem.JobId);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Import job {JobId} failed after {Elapsed}ms.", workItem.JobId, stopwatch.ElapsedMilliseconds);
            await jobRepository.MarkFailedAsync(workItem.JobId, ex, cancellationToken);
        }
    }

    #endregion Private Methods
}
