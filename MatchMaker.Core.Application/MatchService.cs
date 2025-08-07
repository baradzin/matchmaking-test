using MatchMaker.Core.Domain.Entities;
using MatchMaker.Core.Domain.Interfaces;

namespace MatchMaker.Core.Application
{
    public class MatchService : IMatchService
    {
        private readonly IMatchAlgorithm _algorithm;

        public MatchService(IMatchAlgorithm algorithm)
        {
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
        }

        public Match? HandleSearch(UserId userId)
        {
            if (userId is null) throw new ArgumentNullException(nameof(userId));

            // Try to add the user to the algorithm's pool
            if (_algorithm.TryAddUser(userId, out var completedMatch))
                return completedMatch;

            return null;
        }
    }
}
