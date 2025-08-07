using MatchMaker.Core.Domain.Entities;

namespace MatchMaker.Core.Application
{
    public interface IMatchService
    {
        /// <summary>
        /// Processes a matchmaking search request. Returns a Match if formed, or null if buffered.
        /// </summary>
        Match? HandleSearch(UserId userId);

        /// <summary>
        /// Retrieves the last match for the given user.
        /// </summary>
        //Task<Match?> GetMatchForUserAsync(UserId userId, CancellationToken cancellationToken = default);
    }
}
