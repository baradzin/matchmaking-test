using MatchMaker.Core.Domain.Entities;

namespace MatchMaker.Core.Domain.Interfaces
{
    public interface IMatchRepository
    {
        Task SaveAsync(Match match, CancellationToken ct = default);
        Task<Match?> GetForUserAsync(UserId userId, CancellationToken ct = default);
    }
}
