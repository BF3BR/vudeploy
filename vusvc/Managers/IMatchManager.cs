using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vusvc.Data;
using static vusvc.Managers.MatchManager;
using static vusvc.Managers.MatchManager.MatchExt;

namespace vusvc.Managers
{
    /// <summary>
    /// Interface for hosting a match
    /// 
    /// The match will span across multiple servers, lobbies from in-game, and web
    /// 
    /// It will be responsible for squad manaagement and team management
    /// </summary>
    public interface IMatchManager
    {
        bool AddMatch(out MatchExt p_Match, Guid p_ServerId, IEnumerable<PlayerMatchTeam> p_Teams);
        bool QueueLobby(Guid p_LobbyId);
        Match? GetMatchById(Guid p_MatchId);
        Match? GetMatchByPlayerId(Guid p_PlayerId);

        // TODO: Figure out how we will be handling different match conditions and states

        IEnumerable<Match> GetMatches();


        
    }
}
