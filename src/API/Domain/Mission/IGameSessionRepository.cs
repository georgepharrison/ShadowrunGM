using ShadowrunGM.ApiSdk.Common.Results;
using ShadowrunGM.Domain.Character;
using ShadowrunGM.Domain.Common;

namespace ShadowrunGM.Domain.Mission;

/// <summary>
/// Repository interface for GameSession aggregates.
/// </summary>
public interface IGameSessionRepository : IRepository<GameSession, SessionId>
{
    /// <summary>
    /// Gets all sessions for a specific character.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result containing the list of sessions or an error.</returns>
    Task<Result<IReadOnlyList<GameSession>>> GetByCharacterIdAsync(CharacterId characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the active session for a character.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result containing the active session or an error.</returns>
    Task<Result<GameSession>> GetActiveSessionAsync(CharacterId characterId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sessions within a date range.
    /// </summary>
    /// <param name="startDate">The start date.</param>
    /// <param name="endDate">The end date.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result containing the list of sessions or an error.</returns>
    Task<Result<IReadOnlyList<GameSession>>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a character has an active session.
    /// </summary>
    /// <param name="characterId">The character identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A Result containing true if an active session exists; otherwise, false.</returns>
    Task<Result<bool>> HasActiveSessionAsync(CharacterId characterId, CancellationToken cancellationToken = default);
}