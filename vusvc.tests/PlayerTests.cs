using System;
using System.Diagnostics;
using System.Linq;
using vusvc.Managers;
using Xunit;

namespace vusvc.tests
{
    public class PlayerTests
    {
        private ILobbyManager m_LobbyManager;
        private IMatchManager m_MatchManager;
        private IPlayerManager m_PlayerManager;
        private IServerManager m_ServerManager;

        private Random m_Random;
        public PlayerTests()
        {
            Debug.WriteLine("init");

            m_Random = new Random();

            m_PlayerManager = new PlayerManager();
            /*m_ServerManager = new ServerManager();

            m_LobbyManager = new LobbyManager(m_PlayerManager);
            m_MatchManager = new MatchManager(m_LobbyManager, m_PlayerManager, m_ServerManager);*/
            
        }

        private string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[m_Random.Next(s.Length)]).ToArray());
        }

        ~PlayerTests()
        {
            // Terminate all servers
            //m_ServerManager.TerminateAllServers();

            Debug.WriteLine("destroy");
        }

        [Fact]
        public void PlayerManager_CreatePlayer()
        {
            Debug.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);

            var s_ZeusId = Guid.NewGuid();
            var s_Name = $"Player_{RandomString(8)}";
            Assert.True(m_PlayerManager.AddPlayer(s_ZeusId, s_Name, out Models.Player? s_Player));

            Assert.NotEqual(s_Player.Id, Guid.Empty);
            Assert.Equal(s_Player.Name, s_Name);
            Assert.Equal(s_Player.ZeusId, s_ZeusId);
            Assert.Contains(s_Name, s_Player.PreviousNames);
        }

        [Fact]
        public void PlayerManager_CreateAndGetPlayer()
        {
            Debug.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);

            var s_ZeusId = Guid.NewGuid();
            var s_Name = $"Player_{RandomString(8)}";
            Assert.True(m_PlayerManager.AddPlayer(s_ZeusId, s_Name, out Models.Player? s_Player));

            Assert.True(m_PlayerManager.AddPlayer(s_ZeusId, s_Name, out Models.Player? s_SecPlayer));

            Assert.Equal(s_Player.Id, s_SecPlayer.Id);
        }

        [Fact]
        public void PlayerManager_GetPlayerByZeusId()
        {
            Debug.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);

            var s_ZeusId = Guid.NewGuid();
            var s_Name = $"Player_{RandomString(8)}";
            Assert.True(m_PlayerManager.AddPlayer(s_ZeusId, s_Name, out Models.Player? s_Player));

            Assert.Equal(m_PlayerManager.GetPlayerByZeusId(s_ZeusId)?.Id, s_Player.Id);
        }

        [Fact]
        public void PlayerManager_GetPlayerById()
        {
            Debug.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);

            var s_ZeusId = Guid.NewGuid();
            var s_Name = $"Player_{RandomString(8)}";
            Assert.True(m_PlayerManager.AddPlayer(s_ZeusId, s_Name, out Models.Player? s_Player));

            Assert.Equal(s_Player.Id, m_PlayerManager.GetPlayerById(s_Player.Id)?.Id);
        }

        [Fact]
        public void PlayerManager_GetPlayersByName()
        {
            Debug.WriteLine(System.Reflection.MethodBase.GetCurrentMethod().Name);

            var s_ZeusId = Guid.NewGuid();
            var s_Name = $"Player_{RandomString(8)}";
            Assert.True(m_PlayerManager.AddPlayer(s_ZeusId, s_Name, out Models.Player? s_Player));

            Assert.Contains(s_Player, m_PlayerManager.GetPlayersByName(s_Name));
        }

        [Fact]
        public void PlayerManager_SaveLoad()
        {

        }

        [Fact]
        public void PlayerManager_GetAllPlayers()
        {

        }
    }
}
