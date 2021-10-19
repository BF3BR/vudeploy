using System;
using System.Collections.Generic;
using System.Diagnostics;
using vusvc.Data;
using vusvc.Managers;
using Xunit;

namespace vusvc.tests
{
    public class UsabilityTests
    {
        private ILobbyManager m_LobbyManager;
        private IMatchManager m_MatchManager;
        private IPlayerManager m_PlayerManager;
        private IServerManager m_ServerManager;

        private Random m_Random;

        private const int c_PlayerCount = 50;

        public UsabilityTests()
        {
            m_Random = new Random();

            m_PlayerManager = new PlayerManager();
            m_ServerManager = new ServerManager()
            {
                InstancesDirectory = @"C:\Users\godiwik\Documents\_source\vudeploy\vusvc\Instances"
            };
            

            m_LobbyManager = new LobbyManager(m_PlayerManager);
            m_MatchManager = new MatchManager(m_LobbyManager, m_PlayerManager, m_ServerManager);

        }

        [Fact]
        public void LobbyTest()
        {
            
        }

        [Fact]
        public void ServerTests()
        {

        }

        [Fact]
        public void PlayerTest()
        {
            var s_PlayerIds = new List<Guid>();
            for (var i = 0; i < c_PlayerCount; ++i)
            {
                var s_PlayerZeusId = Guid.NewGuid();
                var s_PlayerName = $"Player_{Util.RandomString(8)}";

                Assert.True(m_PlayerManager.AddPlayer(s_PlayerZeusId, s_PlayerName, out Models.Player? s_Player));

                s_PlayerIds.Add(s_Player.Id);
            }

            // Check for duplicates
            var s_DuplicatePlayerIds = new List<Guid>();

            // TODO: Finish
        }

        [Fact]
        public void StartToPlayerJoinTest()
        {
            var s_PlayerZeusId = Guid.NewGuid();
            var s_PlayerName = $"Player_{Util.RandomString(8)}";

            Assert.True(m_PlayerManager.AddPlayer(s_PlayerZeusId, s_PlayerName, out Models.Player? s_Player));

            Assert.True(m_LobbyManager.AddLobby(s_Player.Id, 10, "MyLobbyName", out Data.PlayerLobby s_Lobby));

            Assert.True(m_MatchManager.QueueLobby(s_Lobby.LobbyId));

            // Check to make sure we actually got queued
            Assert.NotEqual(MatchState.Invalid, m_MatchManager.GetMatchStateByLobbyId(s_Lobby.LobbyId));

            // At this point there should not be enough lobbies queued up to continue, so we should be in the queued state
            Assert.Equal(MatchState.Queued, m_MatchManager.GetMatchStateByLobbyId(s_Lobby.LobbyId));

            // TODO: Add another lobby which should allow to proceed

            // Create another player
            var s_PlayerZeusId2 = Guid.NewGuid();
            var s_PlayerName2 = $"Player_{Util.RandomString(8)}";

            Assert.True(m_PlayerManager.AddPlayer(s_PlayerZeusId2, s_PlayerName2, out Models.Player? s_Player2));

            Assert.True(m_LobbyManager.AddLobby(s_Player2.Id, 10, "MySecondLobby", out Data.PlayerLobby s_Lobby2));


            Assert.True(m_MatchManager.QueueLobby(s_Lobby.LobbyId));

            // Check to make sure we actually got queued
            Assert.NotEqual(MatchState.Invalid, m_MatchManager.GetMatchStateByLobbyId(s_Lobby.LobbyId));

            var s_StartTime = DateTime.Now;
            var s_EndTime = s_StartTime.AddSeconds(10);

            var s_CurrentMatchState = MatchState.Invalid;
            while ((s_CurrentMatchState = m_MatchManager.GetMatchStateByLobbyId(s_Lobby.LobbyId)) != MatchState.Invalid)
            {
                //Debug.WriteLine($"State: {s_CurrentMatchState}.");

                Assert.True(s_CurrentMatchState == MatchState.Queued || s_CurrentMatchState == MatchState.Waiting);

                // Kill checking after some time
                if (DateTime.Now > s_EndTime)
                    break;
            }

            Debug.WriteLine($"State: {s_CurrentMatchState}.");

            m_ServerManager.TerminateAllServers();

            s_CurrentMatchState = m_MatchManager.GetMatchStateByLobbyId(s_Lobby.LobbyId);
            Debug.WriteLine($"State: {s_CurrentMatchState}.");
        }
    }
}
