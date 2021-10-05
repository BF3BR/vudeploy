﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        bool AddMatch();
        bool QueueLobby(Guid p_LobbyId);
        Match? GetMatchById(Guid p_MatchId);
        Match? GetMatchByPlayerId(Guid p_PlayerId);

        // TODO: Figure out how we will be handling different match conditions and states

        IEnumerable<Match> GetMatches();


        
    }
}
