using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using vusvc.Data;
using vusvc.Extensions;
using vusvc.Managers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace vusvc.Controllers
{
    /// <summary>
    /// WARN/SECURITY: We acknoledge that all players may be able to lie about their zeus id/player id if it was ever leakaed
    /// in order to do things on another players behalf.
    /// 
    /// Will fix this in the future by using nonces and verifying against those to ensure that each player has a secret given by the server
    /// to validate their requests
    /// 
    /// (Or look into ASP Core identities)
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LobbyController : ControllerBase
    {
        public struct CreateLobbyApi
        {
            public Guid ZeusId { get; set; }
            public string Name { get; set; }
            public ushort MaxPlayers { get; set; }
        }

        private List<PlayerLobby> m_Lobbies;

        private IPlayerManager m_PlayerManager;
        private ILobbyManager m_LobbyManager;

        public LobbyController(IPlayerManager p_PlayerManager, ILobbyManager p_LobbyManager)
        {
            m_Lobbies = new List<PlayerLobby>();
            m_PlayerManager = p_PlayerManager;
            m_LobbyManager = p_LobbyManager;
        }        

        // GET: api/<LobbyController>
        [HttpGet]
        public ActionResult<PlayerLobby[]> Get(string Key)
        {
            return m_Lobbies.ToArray();
        }

        [HttpPost("CreateLobby")]
        [Consumes(MediaTypeNames.Application.Json)]
        
        public ActionResult<PlayerLobby> CreateLobby(CreateLobbyApi p_Request)
        {
            // Validate that we have some kind of name
            if (string.IsNullOrWhiteSpace(p_Request.Name) || string.IsNullOrEmpty(p_Request.Name))
                return BadRequest();

            // Check to see if we have a zeus id
            if (p_Request.ZeusId == Guid.Empty)
                return BadRequest();

            // Sanitize the name before we store it
            var s_SanitizedName = p_Request.Name.Sanitize();

            // Add the player to our player manager
            /*if (!m_PlayerManager.AddPlayer(p_Request.ZeusId, s_SanitizedName, out Player? s_Player))
                return BadRequest();

            // Get the player that was just created
            if (s_Player is null)
                return Problem(statusCode: 500);*/

            if (!m_LobbyManager.AddLobby(p_Request.ZeusId, p_Request.MaxPlayers, s_SanitizedName, out PlayerLobby? s_Lobby))
                return BadRequest();

            return s_Lobby;
        }
    }
}
