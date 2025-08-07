namespace MatchMaker.ApiService.DTOs
{
    public sealed record MatchResponseDto(Guid MatchId, IReadOnlyList<string> UserIds);
}
