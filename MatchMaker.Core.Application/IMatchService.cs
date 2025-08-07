using MatchMaker.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchMaker.Core.Application
{
    public interface IMatchService
    {
        /// <summary>
        /// Processes a matchmaking search request. Returns a Match if formed, or null if buffered.
        /// </summary>
        Task<Match?> HandleSearchAsync(UserId userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the last match for the given user.
        /// </summary>
        Task<Match?> GetMatchForUserAsync(UserId userId, CancellationToken cancellationToken = default);
    }
}
