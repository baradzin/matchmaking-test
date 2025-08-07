using MatchMaker.Core.Domain.Entities;
using MatchMaker.Core.Domain.Interfaces;

namespace MatchMaker.Core.Application
{
    public class MatchService(IMatchAlgorithm algorithm) : IMatchService
    {
        private readonly IMatchAlgorithm _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));

        public Match? HandleSearch(UserId userId)
        {
            ArgumentNullException.ThrowIfNull(userId);

            // Try to add the user to the algorithm's pool
            if (_algorithm.TryAddUser(userId, out var completedMatch))
                return completedMatch;

            return null;
        }
    }
}
