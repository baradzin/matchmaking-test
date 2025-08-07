using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchMaker.Contracts
{
    public record MatchCompleteDto(Guid MatchId, IReadOnlyList<string> UserIds);
}
