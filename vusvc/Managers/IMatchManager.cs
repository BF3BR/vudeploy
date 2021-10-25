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
#if DEBUG
        /// <summary>
        /// This is used for debugging purposes, in release builds the server will
        /// know it's own zeus id so it's a non-issue, but there's no easy way for
        /// testing to be able to access this
        /// </summary>
        /// <param name="p_LobbyId"></param>
        /// <returns></returns>
        Match GetMatchByLobbyId(Guid p_LobbyId);
#endif

        bool GetMatchInfoByZeusId(Guid p_ZeusId, out Guid? p_MatchId, out Dictionary<Guid, Guid>? p_PlayerLobbyIds);


        bool SetMatchStateById(Guid p_MatchId, MatchState p_State);
        bool SetMatchCompletedById(Guid p_MatchId, IEnumerable<Guid> p_Winners, IEnumerable<Guid> p_Players);

        /// <summary>
        /// Gets a matches player count
        /// </summary>
        /// <param name="p_MatchId">Match id</param>
        /// <returns>Number of players for the provided match</returns>
        int GetMatchPlayerCount(Guid p_MatchId);

        // TODO: Figure out how we will be handling different match conditions and states

        /// <summary>
        /// Gets a list of all current matches, should not be exposed to the public
        /// </summary>
        /// <returns>List of matches</returns>
        IEnumerable<Match> GetMatches();


        Dictionary<Guid, Guid> GetMatchPlayerTeams(Guid p_MatchId);
        
    }
}
