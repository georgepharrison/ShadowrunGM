using ShadowrunGM.API.Importing.Contracts;

namespace ShadowrunGM.API.Importing.Abstractions;

public interface IStructuredPersister
{
    #region Public Methods

    Task<PersistResult> SaveAsync(ImportWorkItem item, IReadOnlyList<LabeledChunk> labeled, CancellationToken cancellationToken = default);

    #endregion Public Methods
}