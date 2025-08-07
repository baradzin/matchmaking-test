using MatchMaker.Core.Domain.Entities;
using MatchMaker.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace Infrastructure.Algorithms
{
    public sealed class RoundRobinAlgorithm : IMatchAlgorithm
    {
        public int UsersPerMatch { get; }

        private readonly List<UserId> _pool = [];

        private readonly ILogger<RoundRobinAlgorithm> _logger;

        public RoundRobinAlgorithm(ILogger<RoundRobinAlgorithm> logger, int usersPerMatch = 3)
        {
            _logger = logger;
            UsersPerMatch = usersPerMatch;
            _logger.LogInformation("RoundRobinAlgorithm initialized with UsersPerMatch {UsersPerMatch}", usersPerMatch);
        }

        public bool TryAddUser(UserId user, [NotNullWhen(true)] out Match? completed)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _pool.Add(user);

            _logger.LogInformation("User {UserId} added to pool ({Count}/{Capacity})", user.Value, _pool.Count, UsersPerMatch);

            if (_pool.Count < UsersPerMatch)
            {
                completed = null;
                return false;
            }

            completed = new Match(new MatchId(Guid.NewGuid()), _pool.AsReadOnly(), DateTimeOffset.UtcNow);

            _logger.LogInformation("Match {MatchId} completed with users: {Users}",
               completed.Id, string.Join(", ", completed.Users.Select(u => u.Value)));

            _pool.Clear();

            return true;
        }
    }
}
