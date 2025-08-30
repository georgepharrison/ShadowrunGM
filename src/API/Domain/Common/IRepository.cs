using ShadowrunGM.ApiSdk.Common.Results;

namespace ShadowrunGM.Domain.Common;

/// <summary>
/// Base repository interface for all aggregate roots.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate root.</typeparam>
/// <typeparam name="TId">The type of the aggregate's identifier.</typeparam>
public interface IRepository<TAggregate, TId> 
    where TAggregate : AggregateRoot
{
    /// <summary>
    /// Gets an aggregate by its identifier.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result containing the aggregate or an error.</returns>
    Task<Result<TAggregate>> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    Task<Result> AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing aggregate in the repository.
    /// </summary>
    /// <param name="aggregate">The aggregate to update.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    Task<Result> UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an aggregate from the repository.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result indicating success or failure.</returns>
    Task<Result> DeleteAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an aggregate with the specified identifier exists.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result containing true if the aggregate exists; otherwise, false.</returns>
    Task<Result<bool>> ExistsAsync(TId id, CancellationToken cancellationToken = default);
}