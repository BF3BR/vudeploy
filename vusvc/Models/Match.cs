using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vusvc.Models
{
    public class Match
    {
        public struct QueueLobbyRequest
        {
            public Guid LobbyId { get; set; }
        }

        public struct MatchStatusRequest
        {
            public Guid MatchId { get; set; }
            public Guid PlayerId { get; set; }
        }

        public struct FindMatchByPlayerIdRequest
        {
            public Guid PlayerId { get; set; }
        }
    }
}
