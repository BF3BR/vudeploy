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

            public DateTime WaitTimeStart { get; set; }
            public DateTime WaitTimeEnd { get; set; }
        }
        // Lobbies that have been queued up and can be used for creating new matches
        private Queue<Guid> m_QueuedLobbies;

        // Matches that have been created and are pending a timer
        private List<MatchExt> m_PendingMatches;

        // The current running matches
        private List<MatchExt> m_CurrentMatches;

        // Reference to the lobby manager
        private ILobbyManager m_LobbyManager;
        private IPlayerManager m_PlayerManager;
        private IServerManager m_ServerManager;

        private Timer m_MatchUpdateTimer;

        private const int c_MaxPlayerCount = 100;
        private const int c_WaitTimeInMinutes = 1;

        public MatchManager(ILobbyManager p_LobbyManager, IPlayerManager p_PlayerManager, IServerManager p_ServerManager)
        {
            // Assign our lobby manager
            m_LobbyManager = p_LobbyManager;
            m_PlayerManager = p_PlayerManager;
            m_ServerManager = p_ServerManager;

            // Create our new containers
            m_QueuedLobbies = new Queue<Guid>();
            m_PendingMatches = new List<MatchExt>();
            m_CurrentMatches = new List<MatchExt>();

            // Create the new timers
            m_MatchUpdateTimer = new Timer(1000 * 2);
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
            lock (m_QueuedLobbies)
            {
                lock (m_PendingMatches)
                {
                    // Check to see if there are any queued lobbies that need to be put in a pending match
                    while (m_QueuedLobbies.Any())
                    {
                        // Get the queued lobby id, we must find somewhere to put this
                        var l_QueuedLobbyId = m_QueuedLobbies.Dequeue();

                        // Get the lobby, if it doesn't exist bail
                        var l_Lobby = m_LobbyManager.GetLobbyById(l_QueuedLobbyId);
                        if (l_Lobby is null)
                            continue;

                        if (!m_PendingMatches.Any())
                        {
                            // TODO: Create a new match, add players, and prepare the queue
                            var l_Match = new MatchExt
                            {
                                MatchId = Guid.NewGuid(),
                                Players = new List<Guid>(),
                                State = MatchState.Queued,
                                PlayerLobbyIds = new Dictionary<Guid, Guid>(),
                                LobbyIds = new List<Guid>()
                            };

                            m_PendingMatches.Add(l_Match);
                        }

                        // First we need to see if we have any pending matches that need updating
                        foreach (var l_PendingMatch in m_PendingMatches)
                        {
                            // Get the current player count
                            var s_Count = l_PendingMatch.Players.Count;

                            // check if this pending match has space for players
                            if (s_Count >= c_MaxPlayerCount)
                                continue;

                            // Check to see if we added this lobby to the total player count, we won't overflow
                            if (s_Count + l_Lobby.PlayerIds.Count > c_MaxPlayerCount)
                                continue;

                            // Add all players to this match
                            l_PendingMatch.Players.AddRange(l_Lobby.PlayerIds);

                            // Add the array of zeus id, lobby id
                            foreach (var l_PlayerId in l_Lobby.PlayerIds)
                            {
                                var l_Player = m_PlayerManager.GetPlayerById(l_PlayerId);
                                if (l_Player is null)
                                    continue;

                                l_PendingMatch.PlayerLobbyIds.Add(l_Player.ZeusId, l_Lobby.LobbyId);
                            }
                            break;
                        }
                    }


                    // This is a hack to get around removing objects while iterating
                    var s_PendingMatchesToMove = new List<MatchExt>();

                    // Iterate over all of the pending matches
                    foreach (var l_PendingMatch in m_PendingMatches)
                    {
                        // Ensure that all of the matches are currently in the waiting state
                        if (l_PendingMatch.State != MatchState.Waiting)
                        {
#if DEBUG
                            Debug.WriteLine($"Skipping pending match ({l_PendingMatch.MatchId}) in ({l_PendingMatch.State}) state.");
#endif
                            continue;
                        }


                        // Check to see if we need to launch/start any matches
                        if (DateTime.Now > l_PendingMatch.WaitTimeEnd)
                        {
                            // TOOD: Launch this match
#if DEBUG
                            Debug.WriteLine($"Launching Match ({l_PendingMatch.MatchId}) -> InGame.");
#endif
                            l_PendingMatch.State = MatchState.InGame;

                            // Add to our running matches queue
                            s_PendingMatchesToMove.Add(l_PendingMatch);
                        }
                    }

                    // Remove all from pending matches
                    var s_RemovalCount = m_PendingMatches.RemoveAll(x => s_PendingMatchesToMove.Contains(x));
                    if (s_RemovalCount == 0)
                        Console.WriteLine("nothing removed");

                    foreach (var l_PendingMatch in s_PendingMatchesToMove)
                    {
                        if (!LaunchMatch(l_PendingMatch))
                        {
                            Console.WriteLine($"Failed to launch match ({l_PendingMatch.MatchId}).");
                            continue;
                        }
                    }
                }
            }
        }

        private bool LaunchMatch(MatchExt p_Match)
        {
            // Create a new server
            if (!m_ServerManager.AddServer(out Server? s_Server, true, "0.0.0.0", "battleroyale", Server.ServerInstanceFrequency.Frequency30, Server.ServerInstanceType.Game))
            {
                Console.WriteLine($"Could not add server, returning match ({p_Match.MatchId}) back to queue...");
                m_PendingMatches.Add(p_Match);
                return false;
            }

            // Handle when a server dies or crashes
            s_Server.ServerTerminated += OnServerTerminated;

            // Handle when the zeus id finally populates
            s_Server.ZeusIdUpdated += OnServerZeusIdUpdated;

            // Set the server id of this server so we can reference it later
            p_Match.ServerId = s_Server.ServerId;

            return true;
        }

        private void OnServerZeusIdUpdated(object p_Sender, EventArgs p_Args)
        {
            // Verify that we have the corrent event args
            if (!(p_Args is Server.ZeusIdUpdatedEventArgs))
                return;

            var s_Server = p_Sender as Server;

            var s_Match = GetMatchByServerId(s_Server.ServerId) as MatchExt;

            Console.WriteLine($"Match ({s_Match.MatchId}) server ({s_Server.ServerId}) has come online.");
        }

        private void OnServerTerminated(object p_Sender, EventArgs p_Args)
        {
            var s_Server = p_Sender as Server;

            var s_Match = GetMatchByServerId(s_Server.ServerId) as MatchExt;

            Console.WriteLine($"Match ({s_Match.MatchId}) server ({s_Server.ServerId}) has gone offline.");
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
            var s_PendingMatch = m_PendingMatches.FirstOrDefault(p_PendingMatch => p_PendingMatch.MatchId == p_MatchId);
            if (s_PendingMatch is not null)
                return s_PendingMatch.Players.Count;

            var s_CurrentMatch = m_CurrentMatches.FirstOrDefault(p_CurrentMatch => p_CurrentMatch.MatchId == p_MatchId);
            if (s_CurrentMatch is not null)
                return s_CurrentMatch.Players.Count;

            return 0;
        }

        public bool AddMatch()
        {
            throw new NotImplementedException();
        }

        public Match GetMatchById(Guid p_MatchId)
        {
            var s_PendingMatch = m_PendingMatches.FirstOrDefault(p_PendingMatch => p_PendingMatch.MatchId == p_MatchId);
            if (s_PendingMatch is not null)
                return s_PendingMatch;

            var s_CurrentMatch = m_CurrentMatches.FirstOrDefault(p_CurrentMatch => p_CurrentMatch.MatchId == p_MatchId);
            if (s_CurrentMatch is not null)
                return s_CurrentMatch;

            return null;
        }

        public Match GetMatchByPlayerId(Guid p_PlayerId)
        {
            var s_PendingMatch = m_PendingMatches.FirstOrDefault(p_PendingMatch => p_PendingMatch.Players.Contains(p_PlayerId));
            if (s_PendingMatch is not null)
                return s_PendingMatch;

            var s_CurrentMatch = m_CurrentMatches.FirstOrDefault(p_CurrentMatch => p_CurrentMatch.Players.Contains(p_PlayerId));
            if (s_CurrentMatch is not null)
                return s_CurrentMatch;

            return null;
        }

        public Match GetMatchByServerId(Guid p_ServerId)
        {
            var s_PendingMatch = m_PendingMatches.FirstOrDefault(p_PendingMatch => p_PendingMatch.ServerId == p_ServerId);
            if (s_PendingMatch is not null)
                return s_PendingMatch;

            var s_CurrentMatch = m_CurrentMatches.FirstOrDefault(p_CurrentMatch => p_CurrentMatch.ServerId == p_ServerId);
            if (s_CurrentMatch is not null)
                return s_CurrentMatch;

            return null;
        }

        public IEnumerable<Match> GetMatches()
        {
            var s_Matches = new List<Match>(m_PendingMatches);

            s_Matches.AddRange(m_CurrentMatches);

            return s_Matches;
        }

        public bool QueueLobby(Guid p_LobbyId)
        {
            // If this lobby was already queued, return success
            if (m_QueuedLobbies.Contains(p_LobbyId))
                return true;

            // Check to see if the lobby id provided exists
            var s_Lobby = m_LobbyManager.GetLobbyById(p_LobbyId);
            if (s_Lobby is null)
                return false;

            // Add this lobby to a queue
            m_QueuedLobbies.Enqueue(p_LobbyId);

            return true;
        }

        public bool DequeueLobby(Guid p_LobbyId)
        {
            // HACK: This removes an entry from the queue
            if (m_QueuedLobbies.Contains(p_LobbyId))
            {
                m_QueuedLobbies = new Queue<Guid>(m_QueuedLobbies.Where(x => x != p_LobbyId));
                return true;
            }

            // TODO: Finish maybe?

            return false;
        }

        public MatchState GetMatchStateByLobbyId(Guid p_LobbyId)
        {
            lock (m_QueuedLobbies)
            {
                // Check to see if we are in the queued state
                if (m_QueuedLobbies.Contains(p_LobbyId))
                    return MatchState.Queued;
            }

            lock (m_PendingMatches)
            {
                // Check to see if there is a pending match that we are apart of
                if (m_PendingMatches.Any(p_PendingMatch => p_PendingMatch.LobbyIds.Contains(p_LobbyId)))
                    return MatchState.Waiting;
            }

            // Check to see if we currently are in any match
            if (m_CurrentMatches.Any(p_CurrenetMatch => p_CurrenetMatch.LobbyIds.Contains(p_LobbyId)))
                return MatchState.InGame;

            return MatchState.Invalid;
        }

        public MatchState GetMatchStateById(Guid p_MatchId)
        {
            // Check to see if there is a pending match that we are apart of
            if (m_PendingMatches.Any(p_PendingMatch => p_PendingMatch.MatchId == p_MatchId))
                return MatchState.Waiting;

            // Check to see if we currently are in any match
            if (m_CurrentMatches.Any(p_CurrenetMatch => p_CurrenetMatch.MatchId == p_MatchId))
                return MatchState.InGame;

            return MatchState.Invalid;
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
    }
}
