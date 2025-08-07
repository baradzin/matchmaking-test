using MatchMaker.Core.Domain.Entities;
using MatchMaker.Core.Domain.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Redis
{
    public sealed class RedisMatchRepository(IConnectionMultiplexer mux) : IMatchRepository
    {
        private readonly IDatabase _db = mux.GetDatabase();
        private static string Key(UserId u) => $"match:{u.Value}";

        public async Task SaveAsync(Match m, CancellationToken ct = default)
        {
            var json = JsonSerializer.Serialize(m);
            foreach (var u in m.Users)
            {
                await _db.StringSetAsync(Key(u), json);
                Console.WriteLine($"[Redis] Saved key={Key(u)} json={json}");
            }
        }

        public async Task<Match?> GetForUserAsync(UserId u, CancellationToken ct = default)
        {
            var json = await _db.StringGetAsync(Key(u));
            return json.IsNullOrEmpty ? null
                                      : JsonSerializer.Deserialize<Match>(json!);
        }
    }
}
