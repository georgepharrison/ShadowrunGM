using ShadowrunGM.API.Importing.Contracts;
using FlowRight.Core.Results;

namespace ShadowrunGM.API.Importing.Abstractions;

public interface IImportBuilder
{
    #region Public Methods

    Result<ImportOutcome> Build();

    Task<Result<IImportBuilder>> ChunkAsync(CancellationToken cancellationToken = default);

    Task<Result<IImportBuilder>> ClassifyAsync(CancellationToken cancellationToken = default);

    Task<Result<IImportBuilder>> EmbedAndIndexAsync(Guid jobId, CancellationToken cancellationToken = default);

    Task<Result<IImportBuilder>> ExtractAsync(CancellationToken cancellationToken = default);

    Task<Result<IImportBuilder>> ParseAsync(string blobUri, CancellationToken cancellationToken = default);

    Task<Result<IImportBuilder>> PersistAsync(ImportWorkItem item, CancellationToken cancellationToken = default);

    Task<Result<IImportBuilder>> PersistStructuredAsync(CancellationToken cancellationToken = default);

    #endregion Public Methods
}