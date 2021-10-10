using System;
using System.Collections.Generic;
using vusvc.Data;

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
        bool QueueLobby(Guid p_LobbyId);
        bool DequeueLobby(Guid p_LobbyId);
        Match GetMatchById(Guid p_MatchId);
        Match GetMatchByPlayerId(Guid p_PlayerId);
        Match GetMatchByServerZeusId(Guid p_ServerId);

        /// <summary>
        /// This searches all matches for a lobby id
        /// </summary>
        /// <param name="p_LobbyId"></param>
        /// <returns></returns>
        MatchState GetMatchStateByLobbyId(Guid p_LobbyId);
        MatchState GetMatchStateById(Guid p_MatchId);

        int GetMatchPlayerCount(Guid p_MatchId);

        // TODO: Figure out how we will be handling different match conditions and states

        IEnumerable<Match> GetMatches();


        //bool SetMatchOnline(Guid p_ServerZeusId);
        Dictionary<Guid, Guid> GetMatchPlayerTeams(Guid p_MatchId);
        
    }
}
