namespace MatchMaker.Core.Domain.Entities
{
    public sealed record Match(MatchId Id, IReadOnlyCollection<UserId> Users, DateTimeOffset CreatedAt);
}
