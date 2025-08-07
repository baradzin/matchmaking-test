using MatchMaker.Core.Domain.Entities;
using MatchMaker.Core.Domain.Interfaces;

namespace MatchMaker.Core.Application
{
    public class MatchService : IMatchService
    {
        private readonly IMatchAlgorithm _algorithm;
        private readonly IMatchRepository _matchRepository;

        public MatchService(IMatchAlgorithm algorithm, IMatchRepository repository)
        {
            _algorithm = algorithm ?? throw new ArgumentNullException(nameof(algorithm));
            _matchRepository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Match?> HandleSearchAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            if (userId is null) throw new ArgumentNullException(nameof(userId));

            // Try to add the user to the algorithm's pool
            if (_algorithm.TryAddUser(userId, out var completedMatch))
            {
                await _matchRepository.SaveAsync(completedMatch, cancellationToken);
                return completedMatch;
            }

            return null;
        }

        public Task<Match?> GetMatchForUserAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            if (userId is null) throw new ArgumentNullException(nameof(userId));
            return _matchRepository.GetForUserAsync(userId, cancellationToken);
        }
    }
}
