using System;
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

        public UsabilityTests()
        {
            m_Random = new Random();

            m_PlayerManager = new PlayerManager();
            m_ServerManager = new ServerManager();

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

        }
    }
}
