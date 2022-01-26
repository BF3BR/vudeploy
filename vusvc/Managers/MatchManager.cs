using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using vusvc.Data;

namespace vusvc.Managers
{
    public class MatchManager : IMatchManager
    {
        public class QueueExt
        {
            public Guid LobbyId { get; set; }
            public Guid MatchId { get; set; }
        }

        public class MatchExt : Match
        {
            /// <summary>
            /// Current state of the match
            /// </summary>
            public MatchState State { get; set; }

            /// <summary>
            /// All lobby id's present in this match
            /// </summary>
            public List<Guid> LobbyIds { get; set; }

            /// <summary>
            /// ZeusPlayerId, LobbyId pairs to create teams on the server
            /// </summary>
            public Dictionary<Guid, Guid> PlayerLobbyIds { get; set; }

            /// <summary>
            /// Queue start time
            /// </summary>
            public DateTime QueueTimeStart { get; set; }

            /// <summary>
            /// Queue start timeout/ending
            /// </summary>
            public DateTime QueueTimeEnd { get; set; }

            /// <summary>
            /// Waiting for server, waiting for players time start
            /// </summary>
            public DateTime WaitTimeStart { get; set; }

            /// <summary>
            /// Waiting for server, waiting for players timeout, or end
            /// </summary>
            public DateTime WaitTimeEnd { get; set; }
        }

        // Matches that have been created and are pending a timer
        private List<MatchExt> m_Matches;

#if DEBUG
        public IList<MatchExt> Matches => m_Matches;
#endif

        // Reference to the lobby manager
        private ILobbyManager m_LobbyManager;
        private IPlayerManager m_PlayerManager;
        private IServerManager m_ServerManager;

        private Timer m_MatchUpdateTimer;

        private const int c_MaxPlayerCount = 100;
        private const int c_WaitTimeInMinutes = 1;
        private const int c_QueueTimeInMinutes = 5;
        private const int c_MatchUpdateTimeInSeconds = 2;

        public MatchManager(ILobbyManager p_LobbyManager, IPlayerManager p_PlayerManager, IServerManager p_ServerManager)
        {
            // Assign our lobby manager
            m_LobbyManager = p_LobbyManager;
            m_PlayerManager = p_PlayerManager;
            m_ServerManager = p_ServerManager;

            // Create our new containers
            //m_QueuedLobbies = new Queue<Guid>();
            m_Matches = new List<MatchExt>();

            // Create the new timers
            m_MatchUpdateTimer = new Timer(1000 * c_MatchUpdateTimeInSeconds);
            m_MatchUpdateTimer.Elapsed += OnMatchUpdate;
            m_MatchUpdateTimer.Start();
        }

        ~MatchManager()
        {
            // Stop the timer if it already has not been
            if (m_MatchUpdateTimer.Enabled)
                m_MatchUpdateTimer.Stop();
        }

        private void OnMatchUpdate(object sender, ElapsedEventArgs e)
        {
            // Get all queued matches
            var s_QueuedMatches = m_Matches.Where(p_Match => p_Match.State == MatchState.Queued);
            
            // Check to see if we have any matches
            if (!s_QueuedMatches.Any())
                return;

            foreach (var l_Match in s_QueuedMatches)
            {
                if (DateTime.Now > l_Match.QueueTimeEnd)
                {
                    // Launch the match
                    if (!LaunchMatch(l_Match))
                        Console.WriteLine($"Match ({l_Match.MatchId}) failed to launch.");
                }
            }
        }

        private bool LaunchMatch(MatchExt p_Match)
        {
            Debug.WriteLine($"Launching {p_Match.MatchId}.");

            // Create a new server
            if (!m_ServerManager.AddServer(out Server? s_Server, true, "0.0.0.0", "battleroyale", Server.ServerInstanceFrequency.Frequency30, Server.ServerInstanceType.Game))
            {
                Console.WriteLine($"Could not add server, returning match ({p_Match.MatchId}) back to queue...");
                p_Match.State = MatchState.Queued;
                return false;
            }

            // Handle when a server dies or crashes
            s_Server.ServerTerminated += OnServerTerminated;

            // Handle when the zeus id finally populates
            s_Server.ZeusIdUpdated += OnServerZeusIdUpdated;

            // Set the server id of this server so we can reference it later
            p_Match.ServerId = s_Server.ServerId;

            // Update the match state to waiting
            p_Match.State = MatchState.Waiting;

            return true;
        }

        private void OnServerZeusIdUpdated(object p_Sender, EventArgs p_Args)
        {
            // Verify that we have the corrent event args
            if (!(p_Args is Server.ZeusIdUpdatedEventArgs))
                return;

            var s_Server = p_Sender as Server;

            var s_Match = GetMatchByServerId(s_Server.ServerId) as MatchExt;

            Debug.WriteLine($"Match ({s_Match.MatchId}) server ({s_Server.ServerId}) has come online, waiting for ready.");
        }

        private void OnServerTerminated(object p_Sender, EventArgs p_Args)
        {
            Debug.WriteLine("Server Termination Detected.");

            var s_Server = p_Sender as Server;
            if (s_Server is null)
            {
                Debug.WriteLine("No server object.");
                return;
            }

            var s_Match = GetMatchByServerId(s_Server.ServerId) as MatchExt;
            if (s_Match is null)
            {
                Debug.WriteLine("Could not get match by server id.");
                return;
            }

            s_Match.State = MatchState.Invalid;

            Debug.WriteLine($"Match ({s_Match.MatchId}) server ({s_Server.ServerId}) has gone offline.");
        }

        /// <summary>
        /// Get the total amount of players inside of a match
        /// 
        /// This first checks pending matches, and if none are found with the matching id current matches are checked next.
        /// </summary>
        /// <param name="p_MatchId"></param>
        /// <returns></returns>
        public int GetMatchPlayerCount(Guid p_MatchId)
        {
            return m_Matches.Sum(p_Match => p_Match.Players.Count);
        }

        public Match? GetMatchById(Guid p_MatchId)
        {
            return m_Matches.FirstOrDefault(p_Match => p_Match.MatchId == p_MatchId);
        }

        public Match GetMatchByPlayerId(Guid p_PlayerId)
        {
            return m_Matches.FirstOrDefault(p_Match => p_Match.Players.Contains(p_PlayerId));
        }

        public Match GetMatchByServerId(Guid p_ServerId)
        {
            return m_Matches.FirstOrDefault(p_Match => p_Match.ServerId == p_ServerId);
        }

        public IEnumerable<Match> GetMatches()
        {
            return m_Matches;
        }

        public bool QueueLobby(Guid p_LobbyId)
        {
            // If this lobby was already queued, return success
            if (m_Matches.Any(p_Match => p_Match.LobbyIds.Contains(p_LobbyId)))
                return true;

            // Check to see if the lobby id provided exists
            var s_Lobby = m_LobbyManager.GetLobbyById(p_LobbyId);
            if (s_Lobby is null)
                return false;

            // Get queued matches
            var s_QueuedMatch = m_Matches.FirstOrDefault(p_Match => p_Match.State == MatchState.Queued && (p_Match.Players.Count + s_Lobby.PlayerIds.Count <= c_MaxPlayerCount));
            if (s_QueuedMatch is null)
            {
                // There are no available queued matches create a new one
                var s_Match = new MatchExt
                {
                    LobbyIds = new List<Guid>(),
                    MatchId = Guid.NewGuid(),
                    PlayerLobbyIds = new Dictionary<Guid, Guid>(),
                    Players = new List<Guid>(),
                    QueueTimeStart = DateTime.Now,
                    QueueTimeEnd = DateTime.Now.AddSeconds(3), //  AddMinutes(c_QueueTimeInMinutes),
                    State = MatchState.Queued,
                    Winners = new List<Guid>()
                };

                m_Matches.Add(s_Match);

                s_QueuedMatch = s_Match;
                
            }

            // Handle adding to existing queue
            s_QueuedMatch.LobbyIds.Add(s_Lobby.LobbyId);
            foreach (var l_PlayerId in s_Lobby.PlayerIds)
            {
                var l_Player = m_PlayerManager.GetPlayerById(l_PlayerId);
                if (l_Player is null)
                    continue;

                s_QueuedMatch.PlayerLobbyIds.Add(l_Player.ZeusId, s_Lobby.LobbyId);
            }

            return true;
        }

        public bool DequeueLobby(Guid p_LobbyId)
        {
            var s_Lobby = m_LobbyManager.GetLobbyById(p_LobbyId);
            if (s_Lobby is null)
                return false;

            var s_Match = GetMatchByLobbyId(p_LobbyId) as MatchExt;
            if (s_Match is null)
                return false;

            // Remove the lobby
            if (!s_Match.LobbyIds.Remove(s_Lobby.LobbyId))
            {
                Console.WriteLine($"Could not remove lobby ({s_Lobby.LobbyId}) from Match ({s_Match.MatchId}).");
            }

            // Remove the players
            foreach (var l_PlayerId in s_Lobby.PlayerIds)
            {
                // Get the player by id
                var l_Player = m_PlayerManager.GetPlayerById(l_PlayerId);
                if (l_Player is null)
                    continue;

                // Remove by zeus id
                if (!s_Match.PlayerLobbyIds.Remove(l_Player.ZeusId))
                {
                    Console.WriteLine($"Could not remove player zeus id ({l_Player.ZeusId}) from Match ({s_Match.MatchId})");
                }
            }

            return true;
        }

        public MatchState GetMatchStateByLobbyId(Guid p_LobbyId)
        {
            return m_Matches.FirstOrDefault(p_Match => p_Match.LobbyIds.Contains(p_LobbyId))?.State ?? MatchState.Invalid;
        }

#if DEBUG
        public Match GetMatchByLobbyId(Guid p_LobbyId)
#else
        private Match GetMatchByLobbyId(Guid p_LobbyId)
#endif
        {
            return m_Matches.FirstOrDefault(p_Match => p_Match.LobbyIds.Contains(p_LobbyId));
        }

        public MatchState GetMatchStateById(Guid p_MatchId)
        {
            return m_Matches.FirstOrDefault(p_Match => p_Match.MatchId == p_MatchId)?.State ?? MatchState.Invalid;
        }

        public bool SetMatchStateById(Guid p_MatchId, MatchState p_State)
        {
            // Get match
            var s_Match = GetMatchById(p_MatchId) as MatchExt; // m_Matches.FirstOrDefault(p_Match => p_Match.MatchId == p_MatchId);
            if (s_Match is null)
                return false;

            // If we are transitioning from waiting to ingame update the game start time
            if (s_Match.State == MatchState.Waiting && p_State == MatchState.InGame)
                s_Match.GameStartTime = DateTime.Now;

            // Update the match state
            s_Match.State = p_State;

            return true;
        }

        public Match GetMatchByServerZeusId(Guid p_ServerId)
        {
            var s_Server = m_ServerManager.GetServerById(p_ServerId);
            if (s_Server is null)
                return null;

            return GetMatchByServerId(s_Server.ServerId);
        }

        public Dictionary<Guid, Guid> GetMatchPlayerTeams(Guid p_MatchId)
        {
            var s_Match = GetMatchById(p_MatchId);
            if (s_Match is null)
                return new Dictionary<Guid, Guid>();

            return (s_Match as MatchExt)?.PlayerLobbyIds;
        }

        public bool GetMatchInfoByZeusId(Guid p_ZeusId, out Guid? p_MatchId, out Dictionary<Guid, Guid> p_PlayerLobbyIds)
        {
            p_MatchId = null;
            p_PlayerLobbyIds = new Dictionary<Guid, Guid>();

            var s_Server = m_ServerManager.GetServerByZeusId(p_ZeusId);
            if (s_Server is null)
                return false;

            var s_Match = m_Matches.FirstOrDefault(p_Match => p_Match.ServerId == s_Server.ServerId);
            if (s_Match is null)
                return false;

            p_MatchId = s_Match.MatchId;
            p_PlayerLobbyIds = s_Match.PlayerLobbyIds;

            return true;
        }

        public bool SetMatchCompletedById(Guid p_MatchId, IEnumerable<Guid> p_Winners, IEnumerable<Guid> p_Players)
        {
            var s_Match = GetMatchById(p_MatchId) as MatchExt;
            if (s_Match is null)
                return false;

            // Set the winners and players
            s_Match.Winners = new List<Guid>(p_Winners);
            s_Match.Players = new List<Guid>(p_Players);

            // Update the match state and end time
            s_Match.State = MatchState.Completed;
            s_Match.GameEndTime = DateTime.Now;

            return true;
        }
    }
}
