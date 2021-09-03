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
    public class LobbyController : Controller
    {
        #region Requests and Responses
        public struct CreateLobbyRequest
        {
            /// <summary>
            /// Player Id
            /// </summary>
            public Guid PlayerId { get; set; }

            /// <summary>
            /// Name of the lobby to create
            /// 
            /// If blank then it will automamtically generate one
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Maximum lobby size
            /// </summary>
            public ushort MaxPlayers { get; set; }
        }

        public struct CreateLobbyResponse
        {
            /// <summary>
            /// Lobby id
            /// </summary>
            public Guid LobbyId { get; set; }

            /// <summary>
            /// Lobby code
            /// </summary>
            public string Code { get; set; }
        }

        public struct RemoveLobbyRequest
        {
            /// <summary>
            /// Requesting player id to destroy lobby (must be an admin of the lobby, checked server side)
            /// </summary>
            public Guid PlayerId { get; set; }

            /// <summary>
            /// Lobby id to destroy
            /// </summary>
            public Guid LobbyId { get; set; }
        }

        public struct JoinLobbyRequest
        {
            /// <summary>
            /// The player id requesting to join
            /// </summary>
            public Guid PlayerId { get; set; }

            /// <summary>
            /// The lobby id to join
            /// </summary>
            public Guid LobbyId { get; set; }

            /// <summary>
            /// The lobby code (for private lobbies)
            /// </summary>
            public string Code { get; set; }
        }

        public struct LeaveLobbyRequest
        {
            /// <summary>
            /// The player id that is requesting to leave
            /// </summary>
            public Guid PlayerId { get; set; }

            /// <summary>
            /// The lobby id of the lobby to leave
            /// </summary>

            public Guid LobbyId { get; set; }
        }

        public struct LobbyStatusRequest
        {
            /// <summary>
            /// Lobby id
            /// </summary>
            public Guid LobbyId { get; set; }

            /// <summary>
            /// Lobby code
            /// </summary>
            public string Code { get; set; }
        }

        public struct LobbyStatusResponse
        {
            /// <summary>
            /// The lobby id
            /// </summary>
            public Guid LobbyId { get; set; }

            /// <summary>
            /// Maximum player count for this lobby
            /// </summary>
            public ushort MaxPlayerCount { get; set; }

            /// <summary>
            /// Names of other players in the lobby
            /// </summary>
            public string[] PlayerNames { get; set; }
        }

        /// <summary>
        /// Lobby update request will extend the existence of the lobby
        /// </summary>
        public struct LobbyUpdateRequest
        {
            /// <summary>
            /// Lobby id to update
            /// </summary>
            public Guid LobbyId { get; set; }

            /// <summary>
            /// Player id of the requesting player
            /// </summary>
            public Guid PlayerId { get; set; }
        }
        #endregion

        // List of lobbies
        private List<PlayerLobby> m_Lobbies;

        // Player manager
        private IPlayerManager m_PlayerManager;

        // Lobby managers
        private ILobbyManager m_LobbyManager;

        public LobbyController(IPlayerManager p_PlayerManager, ILobbyManager p_LobbyManager)
        {
            m_Lobbies = new List<PlayerLobby>();
            m_PlayerManager = p_PlayerManager;
            m_LobbyManager = p_LobbyManager;
        }        

        public IActionResult Index()
        {
            ViewData["Lobbies"] = m_Lobbies.ToArray();

            return View();
        }

        [HttpPost("Create")]
        [Consumes(MediaTypeNames.Application.Json)]
        
        public ActionResult<CreateLobbyResponse> CreateLobby(CreateLobbyRequest p_Request)
        {
            // Validate that we have some kind of name
            if (string.IsNullOrWhiteSpace(p_Request.Name) || string.IsNullOrEmpty(p_Request.Name))
                return BadRequest();

            // Check to see if we have a zeus id
            if (p_Request.PlayerId == Guid.Empty)
                return BadRequest();

            // Get the player
            var s_Player = m_PlayerManager.GetPlayerById(p_Request.PlayerId);
            if (s_Player is null)
                return BadRequest();

            // Sanitize the name before we store it
            var s_SanitizedName = p_Request.Name.Sanitize();

            // Create a new lobby
            if (!m_LobbyManager.AddLobby(s_Player.Id, p_Request.MaxPlayers, s_SanitizedName, out PlayerLobby? s_Lobby))
                return BadRequest();

            return new CreateLobbyResponse
            {
                LobbyId = s_Lobby.Id,
                Code = s_Lobby.Code
            };
        }

        [HttpPost("Remove")]
        [Consumes(MediaTypeNames.Application.Json)]
        public IActionResult RemoveLobby(RemoveLobbyRequest p_Request)
        {
            // Check to see if the lobby exists
            var s_Lobby = m_LobbyManager.GetLobbyById(p_Request.LobbyId);
            if (s_Lobby is null)
                return BadRequest();

            // Check to make sure that the requesting player is an admin and can destroy the lobby
            if (p_Request.PlayerId != s_Lobby.AdminPlayerId)
                return BadRequest();

            if (!m_LobbyManager.RemoveLobby(p_Request.LobbyId))
                return BadRequest();

            return Ok();
        }

        [HttpPost("Join")]
        [Consumes(MediaTypeNames.Application.Json)]
        public IActionResult JoinLobby(JoinLobbyRequest p_Request)
        {
            // Length check the code
            if (p_Request.Code.Length > 4)
                return BadRequest();

            if (!m_LobbyManager.JoinLobby(p_Request.LobbyId, p_Request.PlayerId, p_Request.Code))
                return BadRequest();

            return Ok();
        }

        [HttpPost("Leave")]
        [Consumes(MediaTypeNames.Application.Json)]
        public IActionResult LeaveLobby(LeaveLobbyRequest p_Request)
        {
            if (!m_LobbyManager.LeaveLobby(p_Request.LobbyId, p_Request.PlayerId))
                return BadRequest();

            return Ok();
        }

        [HttpPost("Status")]
        [Consumes(MediaTypeNames.Application.Json)]
        public ActionResult<LobbyStatusResponse> LobbyStatus(LobbyStatusRequest p_Request)
        {
            // Find if the lobby exists
            var s_Lobby = m_LobbyManager.GetLobbyById(p_Request.LobbyId);
            if (s_Lobby is null)
                return BadRequest();

            var s_LobbyCode = p_Request.Code;
            // Check the lobby code code
            if (s_LobbyCode.Length > 4)
                return BadRequest();

            // Check the lobby code against the lobby
            if (s_Lobby.Code != s_LobbyCode)
                return BadRequest();

            // We don't want to leak player id's, so we return player names instead
            var s_PlayerNames = s_Lobby.PlayerIds.Select(p_PlayerId => m_PlayerManager.GetPlayerById(p_PlayerId)?.Name).ToArray();

            return new LobbyStatusResponse
            {
                LobbyId = s_Lobby.Id,
                MaxPlayerCount = s_Lobby.MaxPlayers,
                PlayerNames = s_PlayerNames
            };
        }

        [HttpPost("Update")]
        [Consumes(MediaTypeNames.Application.Json)]
        public IActionResult LobbyUpdate(LobbyUpdateRequest p_Request)
        {
            if (!m_LobbyManager.UpdateLobby(p_Request.LobbyId, p_Request.PlayerId))
                return BadRequest();

            return Ok();
        }
    }
}
