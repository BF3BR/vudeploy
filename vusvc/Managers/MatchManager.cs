using System;
using System.Collections.Generic;
using vusvc.Data;

namespace vusvc.Managers
{
    public class MatchManager : IMatchManager
    {
        public bool AddMatch()
        {
            throw new NotImplementedException();
        }

        public Match GetMatchById(Guid p_MatchId)
        {
            throw new NotImplementedException();
        }

        public Match GetMatchByPlayerId(Guid p_PlayerId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Match> GetMatches()
        {
            throw new NotImplementedException();
        }

        public bool QueueLobby(Guid p_LobbyId)
        {
            throw new NotImplementedException();
        }
    }
}
