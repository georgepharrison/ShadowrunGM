using ShadowrunGM.API.Importing.Abstractions;
using ShadowrunGM.API.Importing.Contracts;
using ShadowrunGM.API.Importing.Jobs;
using ShadowrunGM.ApiSdk.Common.Results;

namespace ShadowrunGM.API.Importing.Builder;

public sealed class ImportDirector : IImportDirector
{
    #region Public Methods

    public async Task<ImportOutcome> RunAsync(ImportWorkItem item, IImportBuilder builder, Func<ImportProgress, Task> report, CancellationToken cancellationToken = default)
    {
        await report(new(ImportStep.Parse, 5, "Parsing"));
        Result<IImportBuilder> result = await builder.ParseAsync(item.BlobUri, cancellationToken);
        if (result.IsFailure) throw new ImportStepException(ImportStep.Parse, result.Error);

        await report(new(ImportStep.Chunk, 25, "Chunking"));
        result = await builder.ChunkAsync(cancellationToken);
        if (result.IsFailure) throw new ImportStepException(ImportStep.Chunk, result.Error);

        await report(new(ImportStep.Classify, 45, "Classifying"));
        result = await builder.ClassifyAsync(cancellationToken);
        if (result.IsFailure) throw new ImportStepException(ImportStep.Classify, result.Error);

        await report(new(ImportStep.Persist, 70, "Persisting"));
        result = await builder.PersistAsync(item, cancellationToken);
        if (result.IsFailure) throw new ImportStepException(ImportStep.Persist, result.Error);

        await report(new(ImportStep.Persist, 70, "Extracting structured items"));
        await builder.ExtractAsync(cancellationToken);

        await report(new(ImportStep.Persist, 80, "Persisting structured items"));
        await builder.PersistStructuredAsync(cancellationToken);

        await report(new(ImportStep.EmbedIndex, 95, "Embedding & indexing"));
        result = await builder.EmbedAndIndexAsync(item.JobId, cancellationToken);
        if (result.IsFailure) throw new ImportStepException(ImportStep.EmbedIndex, result.Error);

        await report(new(ImportStep.Finalize, 99, "Finalizing"));

        Result<ImportOutcome> outcomeResult = builder.Build();

        return outcomeResult.Match(
            onSuccess: success => success,
            onFailure: error => throw new ImportStepException(ImportStep.EmbedIndex, error));
    }

    #endregion Public Methods
}
