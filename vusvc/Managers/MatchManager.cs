﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using vusvc.Data;
using static vusvc.Managers.MatchManager.MatchExt;

namespace vusvc.Managers
{
    public class MatchManager : IMatchManager
    {
        /*
         * Phew, this class won't be that complicated to write just complicated to balance everything
         * 
         * This will need to be able to form a game, create a server, move all players into that server and handle the results of that match.
         * 
         * We are source player lobbies from anywhere, it should be platform agnostic. This means that we will need to be able to handle people who haave created lobby
         * on the website, in discord, or in a lobby server in VU itself. This poses somewhat of a challenge, but this design should be able to handle all of this with no issue
         * 
         * Player -> Creates Lobby on backend
         * Player -> Searches for a match
         * 
         * MatchManager -> Throws Lobby into a queue pool
         * MatchManager -> Starts match matchmaking timer (2-5m before game launches regardless if conditions are met)
         * MatchManager -> Checks the number of players and lock status of all lobbies in the queue pool
         * MatchManager -> Checks to see if there are enough players total to form teams
         * MatchManager -> Repeats step 3, 4 until conditions are met
         * MatchManager -> Forms teams and saves them to the match backend
         * MatchManager -> Creates a new server, or utilizes an idle/non-used server
         * MatchManager -> Waits for the server to request for the team/match information
         * MatchManager -> Sends team information to server
         * 
         * MatchManager -> Notifies all players in a lobby that their game is ready, if they are in-game already in a VU lobby server force join them
         * 
         * BR Server -> When join wait time (5m) is complete round restart and run the game
         * 
         * BR Server -> On round over send all stats, players, information to MatchManager
         * 
         * BR Server -> Kicks all players or attempts to re-join them to a lobby server (they will still be in a lobby, so if the leader searches again, it will pull everyone into the game server)
         * 
         * MatchManager -> Saves all stats, moves all lobbies back to the queue (provided they aren't expired)
        */

        public class MatchExt : Match
        {
            public enum Status
            {
                /// <summary>
                /// Invalid status state
                /// </summary>
                Invalid,

                /// <summary>
                /// This match is waiting for players before it launches (matchmaking period)
                /// </summary>
                WaitingForPlayers,

                /// <summary>
                /// Waiting on the sever creation, teams are locked in and ready to go
                /// </summary>
                WaitingForServer,

                /// <summary>
                /// Match is running, backend is waiting for completion, expiration, or status
                /// </summary>
                WaitingForMatch,

                /// <summary>
                /// Match has completed
                /// </summary>
                Completed,

                COUNT
            }

            public class PlayerMatchTeam
            {
                public int TeamId { get; set; }
                public int SquadId { get; set; }
                public Guid PlayerZeusId { get; set; }
            }

            /// <summary>
            /// MatchManager match status, this isn't needed/used in the db so we add it to the ext
            /// </summary>
            public Status MatchStatus { get; set; }

            /// <summary>
            /// Time to spend waiting on players before starting a match
            /// </summary>
            public int MatchmakingTimeInMinutes { get; set; } = 5;

            /// <summary>
            /// Time to wait for a server to become available, or be created
            /// </summary>
            public int ServerTimeoutInMinutes { get; set; } = 5;

            public DateTime MatchmakingStartTime { get; set; }
            public DateTime MatchmakingEndTime { get; set; }
            public DateTime ServerStartTime { get; set; }
            public DateTime ServerEndTime { get; set; }
        }

        private Queue<Guid> m_QueuedLobbyIds;
        private Timer m_MatchTimer;
        private Timer m_ServerTimer;


        public int MatchmakingCheckTimeInSeconds { get; set; } = 10;
        public int ServerCheckTimeInSeconds { get; set; } = 5;

        private ILobbyManager m_LobbyManager;
        private IPlayerManager m_PlayerManager;

        public const int c_MinLobbies = 2;
        public const int c_MatchSize = 32;


        public MatchManager(ILobbyManager p_LobbyManager, IPlayerManager p_PlayerManager)
        {
            m_LobbyManager = p_LobbyManager;
            m_PlayerManager = p_PlayerManager;

            m_MatchTimer = new Timer(MatchmakingCheckTimeInSeconds * 1000); // Check matchmaking every 10 seconds
            m_MatchTimer.Elapsed += OnMatchmakingTimerElapsed;

            m_ServerTimer = new Timer(ServerCheckTimeInSeconds * 1000); // Check server status every 5 seconds
            m_ServerTimer.Elapsed += OnServerTimerElapsed;
        }

        private void OnMatchmakingTimerElapsed(object p_Sender, ElapsedEventArgs p_Args)
        {
            // Cast our callback object to a timer
            var s_MatchTimer = p_Sender as Timer;

            // There are no players queued up in the lobby, bail
            if (!m_QueuedLobbyIds.Any())
                return;

            
            // BEGIN FORM TEAMS CODE

            // Check the conditions to launch
            
            // Check that w eat least have 2 lobbies
            if (m_QueuedLobbyIds.Count < c_MinLobbies)
                return;

            // Get all of the lobby ids

            var s_NumPlayers = 0;
            var s_UnlockedLobbies = new List<PlayerLobby>();
            var s_LockedLobbies = new List<PlayerLobby>();
            var s_FullLobbies = new List<PlayerLobby>();
            var s_CompletedLobbies = new List<PlayerLobby>();

            for (var i = 0; i < m_QueuedLobbyIds.Count; ++i)
            {
                // Get the lobby from the lobby manager
                var l_Lobby = m_LobbyManager.GetLobbyById(m_QueuedLobbyIds.Dequeue());

                // If this lobby no longer exists just drop it and continue on
                if (l_Lobby is null)
                    continue;

                // Add to our total count of players
                s_NumPlayers += l_Lobby.PlayerIds.Count;

                // Check to see if this lobby is full (regardless of lock status)
                if (l_Lobby.PlayerIds.Count == l_Lobby.MaxPlayers)
                    s_FullLobbies.Add(l_Lobby);
                else
                {
                    // If the lobby is not full, see if the player manually locked the lobby or not
                    if (l_Lobby.SearchLockType == PlayerLobby.LobbySearchLockType.Locked)
                        s_LockedLobbies.Add(l_Lobby);
                    else if (l_Lobby.SearchLockType == PlayerLobby.LobbySearchLockType.Unlocked) // Otherwise add to the unlocked lobbies
                        s_UnlockedLobbies.Add(l_Lobby);
                }
            }

            if (s_NumPlayers == 0)
                return;

            // Filter the unlocked teams
            if (s_UnlockedLobbies.Count != 0)
            {
                s_UnlockedLobbies.Sort(new PlayerLobbyCountCompare());

                Func<PlayerLobby, PlayerLobby, bool> LobbyMerge = (PlayerLobby p_A, PlayerLobby p_B) =>
                {
                    if (p_A.PlayerIds.Count + p_B.PlayerIds.Count > p_A.MaxPlayers)
                        return false;

                    // Move all players from the other team
                    var s_SourcePlayerIds = p_B.PlayerIds.ToArray();

                    foreach (var l_PlayerId in s_SourcePlayerIds)
                    {
                        if (!m_LobbyManager.LeaveLobby(p_B.LobbyId, l_PlayerId))
                        {
                            Console.WriteLine("");
                            // Do not fail here
                        }

                        // This was done without user interaction
                        // TODO: Add API so when this happens the user can be notified of it
                        if (!m_LobbyManager.JoinLobby(p_A.LobbyId, l_PlayerId, p_A.Code))
                        {
                            Console.WriteLine("");
                            continue;
                        }
                    }
                    
                    return true;
                };

                // Merge teams
                var s_Low = 0;
                var s_High = s_UnlockedLobbies.Count - 1;

                while (s_Low < s_High)
                {
                    var s_HighTeam = s_UnlockedLobbies[s_High];
                    var s_LowTeam = s_UnlockedLobbies[s_Low];

                    if (LobbyMerge(s_HighTeam, s_LowTeam))
                    {
                        s_Low++;
                    }
                    else
                        s_High--;
                }

                // Remove all empty lobbies if they exist
                s_UnlockedLobbies.RemoveAll(p_UnlockedLobby => p_UnlockedLobby is null || !p_UnlockedLobby.PlayerIds.Any());


                // Add all of our locked, completed, and full lobbies to one daataset
                s_CompletedLobbies.AddRange(s_UnlockedLobbies);
                s_CompletedLobbies.AddRange(s_LockedLobbies);
                s_CompletedLobbies.AddRange(s_FullLobbies);

                // Finalize teams
                var s_Index = 0;
                var s_AvailableTeamIds = 100;


                foreach (var l_Lobby in s_CompletedLobbies)
                {
                    var s_TeamId = 0;
                    var s_SquadId = 0;

                    if (l_Lobby.PlayerIds.Count < 2)
                    {
                        s_TeamId = 1;
                        s_SquadId = 0;
                    }
                    else
                    {
                        s_TeamId = s_Index % (s_AvailableTeamIds - 1) + 2;
                        s_SquadId = s_Index / (s_AvailableTeamIds - 1) + 1;

                        s_Index++;
                    }

                    var s_FrostbiteData = new List<PlayerMatchTeam>();

                    foreach (var l_PlayerId in l_Lobby.PlayerIds)
                    {
                        var l_Player = m_PlayerManager.GetPlayerById(l_PlayerId);
                        if (l_Player is null)
                            continue;

                        var s_PlayerMatchTeam = new PlayerMatchTeam()
                        {
                            SquadId = s_SquadId,
                            TeamId = s_TeamId,
                            PlayerZeusId = l_Player.ZeusId
                        };

                        s_FrostbiteData.Add(s_PlayerMatchTeam);
                    }
                    
                    // TODO: Create the server

                    // TODO: Wait for server

                    // TODO: ;Send shit to server
                }
            }
            
            
            //var s_QueuedIds = m_QueuedLobbyIds.ToArray();
            //foreach (var l_QueuedId in s_QueuedIds)
            //{
            //    // Get the lobby
            //    var l_Lobby = m_LobbyManager.GetLobbyById(l_QueuedId);
            //    if (l_Lobby is null)
            //        continue;

            //    var l_LobbyLockType = l_Lobby.SearchLockType;
            //}
        }

        private void OnServerTimerElapsed(object sender, ElapsedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public bool QueueLobby(PlayerLobby p_Lobby)
        {
            return true;
        }

        public bool DequeLobby(Guid p_LobbyId)
        {
            return true;
        }
    }
}
