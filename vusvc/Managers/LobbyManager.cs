using System;
using System.Collections.Generic;
using System.Linq;
using vusvc.Data;
using vusvc.Extensions;

namespace vusvc.Managers
{
    /// <summary>
    /// A lobby is the representation of a group of players.
    /// 
    /// They can be created or destroyed at will, and they expire after a default of c_DefaultExpirationTimeInMinutes
    /// 
    /// The lobby system will be responsible for keeping players together even if they come from different endpoints.
    /// 
    /// An endpoint could be anything from a webpage, or any of the lobby/idle servers for BattleRoyale or your mod to join to be put in a match.
    /// 
    /// Players don't physically have to be on the same game server in order to join a match server as a squad.
    /// </summary>
    public class LobbyManager : ILobbyManager
    {
        // List of all of the lobbies
        private List<PlayerLobby> m_Lobbies;

        // Arb random number to cap the maximum lobbies
        private const int c_MaxLobbies = 30;

        // Maximum player count in a lobby
        private const int c_DefaultMaxPlayerCount = 4;

        // Maximum lobby name length
        private const int c_DefaultMaxLobbyNameLength = 32;

        // Default expiration time
        private const int c_DefaultExpirationTimeInMinutes = 5;

        private IPlayerManager m_PlayerManager;

        public LobbyManager(IPlayerManager p_PlayerManager)
        {
            m_Lobbies = new List<PlayerLobby>();
            m_PlayerManager = p_PlayerManager;
        }

        public bool AddLobby(Guid p_PlayerId, ushort p_MaxPlayers, string p_Name, out PlayerLobby? p_PlayerLobby)
        {
            // Set our output to a default value
            p_PlayerLobby = null;

            // Get the player
            var s_CreatorPlayer = m_PlayerManager.GetPlayerById(p_PlayerId);

            // Check to see if the player exists already
            if (s_CreatorPlayer is null)
                return false;

            // Validate the lobby count
            if (m_Lobbies.Count >= c_MaxLobbies)
                return false;

            var s_MaxPlayerCount = Math.Max(p_MaxPlayers, (ushort)1);
            if (s_MaxPlayerCount > c_DefaultMaxPlayerCount)
                s_MaxPlayerCount = c_DefaultMaxPlayerCount;

            // Generate a new lobby id
            var s_LobbyId = Guid.NewGuid();

            // Validate that we do not have duplicates
            while (GetLobbyById(s_LobbyId) != null)
                s_LobbyId = Guid.NewGuid();

            // Generate a new join code for this lobby
            var s_Code = PlayerLobby.GenerateCode();

            // Get the sanitized name
            var s_SanitizedName = p_Name.Sanitize();
            if (s_SanitizedName.Length > c_DefaultMaxLobbyNameLength)
                s_SanitizedName = s_SanitizedName.Substring(0, c_DefaultMaxLobbyNameLength);

            // Create a new lobby with all of our information
            var s_Lobby = new PlayerLobby
            {
                AdminPlayerId = s_CreatorPlayer.Id,
                LobbyId = s_LobbyId,
                MaxPlayers = s_MaxPlayerCount,
                Name = s_SanitizedName,
                Code = s_Code,
                PlayerIds = { s_CreatorPlayer.Id },
                CreationTime = DateTime.Now,
                SearchLockType = (s_MaxPlayerCount == 1 ? PlayerLobby.LobbySearchLockType.Locked : PlayerLobby.LobbySearchLockType.Unlocked),
                SearchState = PlayerLobby.LobbySearchState.None
            };

            p_PlayerLobby = s_Lobby;

            m_Lobbies.Add(s_Lobby);

            return true;
        }

        public void ExpireLobbies()
        {
            m_Lobbies.RemoveAll(p_Lobby => (DateTime.Now - p_Lobby.CreationTime) > TimeSpan.FromMinutes(c_DefaultExpirationTimeInMinutes));
        }

        public IEnumerable<PlayerLobby> GetLobbiesByName(string p_PartialName)
        {
            return m_Lobbies.Where(p_Lobby => p_Lobby.Name.Contains(p_PartialName));
        }

        public PlayerLobby? GetLobbyById(Guid p_LobbyId)
        {
            return m_Lobbies.FirstOrDefault(p_Lobby => p_Lobby.LobbyId == p_LobbyId);
        }

        public bool JoinLobby(Guid p_LobbyId, Guid p_PlayerId, string p_Code)
        {
            // Get the lobby
            var s_Lobby = GetLobbyById(p_LobbyId);

            // Check if the lobby exists
            if (s_Lobby is null)
                return false;

            // Validate the code
            if (s_Lobby.Code != p_Code)
                return false;

            // Check if the player already is in this lobby
            if (s_Lobby.PlayerIds.Contains(p_PlayerId))
                return true;

            // Check to see if the lobby is full or not
            if (s_Lobby.PlayerIds.Count >= c_DefaultMaxPlayerCount)
                return false;

            // Add the player to the lobby
            s_Lobby.PlayerIds.Add(p_PlayerId);

            return true;
        }

        public bool LeaveLobby(Guid p_LobbyId, Guid p_PlayerId)
        {
            // Get the lobby
            var s_Lobby = GetLobbyById(p_LobbyId);

            // If the lobby exist, we returned that we left (who cares at this point)
            if (s_Lobby is null)
                return true;

            // Remove the player from the lobby
            var s_PlayerIdsRemoved = s_Lobby.PlayerIds.RemoveAll(p_Pid => p_Pid == p_PlayerId);
            if (s_PlayerIdsRemoved <= 0)
                return true;


            // Check to see if the admin is the one leaving
            if (s_Lobby.AdminPlayerId == p_PlayerId)
            {
                // Do we have any other players?

                // If we do not, remove this lobby
                if (!s_Lobby.PlayerIds.Any())
                    return RemoveLobby(p_LobbyId);

                // If we do then select the next player
                s_Lobby.AdminPlayerId = s_Lobby.PlayerIds.First();
            }

            return true;
        }

        public bool RemoveLobby(Guid p_LobbyId)
        {
            // Remove all of the lobbies with this guid
            var s_LobbiesRemoved = m_Lobbies.RemoveAll(p_Lid => p_Lid.LobbyId == p_LobbyId);

            // Return if we removed anything
            return s_LobbiesRemoved > 0;
        }

        public bool SetLobbyAdmin(Guid p_LobbyId, Guid p_PlayerId)
        {
            // Get the lobby
            var s_Lobby = GetLobbyById(p_LobbyId);

            // Make sure that the lobby exists
            if (s_Lobby is null)
                return false;

            // Check to make sure that the player w are trying to make an admin is actually a part of the lobby
            if (!s_Lobby.PlayerIds.Contains(p_PlayerId))
                return false;

            // Set the new player admin
            s_Lobby.AdminPlayerId = p_PlayerId;

            return true;
        }

        public bool UpdateLobby(Guid p_LobbyId, Guid p_PlayerId)
        {
            // Get the lobby and validate it exists
            var s_Lobby = GetLobbyById(p_LobbyId);
            if (s_Lobby is null)
                return false;

            // Deny players attempting to update lobbies they are not currently in
            if (!s_Lobby.PlayerIds.Contains(p_PlayerId))
                return false;

            // Update the creation time so this lobby does not expire
            s_Lobby.Update();

            return true;
        }

        public IEnumerable<PlayerLobby> GetAllLobbies()
        {
            return m_Lobbies;
        }
    }
}
