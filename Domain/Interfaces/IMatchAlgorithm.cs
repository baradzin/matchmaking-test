using MatchMaker.Core.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

namespace MatchMaker.Core.Domain.Interfaces
{
    public interface IMatchAlgorithm
    {
        int UsersPerMatch { get; }
        bool TryAddUser(UserId user, [NotNullWhen(true)] out Match? completedMatch);
    }
}
