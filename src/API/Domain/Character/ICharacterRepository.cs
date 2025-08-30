using ShadowrunGM.ApiSdk.Common.Results;
using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Character;

/// <summary>
/// Repository interface for Character aggregates.
/// </summary>
public interface ICharacterRepository : IRepository<Character, CharacterId>
{
    /// <summary>
    /// Gets all characters for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result containing the list of characters or an error.</returns>
    Task<Result<IReadOnlyList<Character>>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a character by name.
    /// </summary>
    /// <param name="name">The character name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result containing the character or an error.</returns>
    Task<Result<Character>> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active characters (not archived or deleted).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result containing the list of active characters or an error.</returns>
    Task<Result<IReadOnlyList<Character>>> GetActiveCharactersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a character with the specified name exists.
    /// </summary>
    /// <param name="name">The character name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result containing true if the character exists; otherwise, false.</returns>
    Task<Result<bool>> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
}