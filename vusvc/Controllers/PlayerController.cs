using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vusvc.Data;
using vusvc.Managers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace vusvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : Controller
    {
        #region Requests and Responses
        /// <summary>
        /// This is what is returned to the user
        /// 
        /// This strips out some extra information such as the zeus id
        /// and the previous names
        /// </summary>
        public struct SafePlayerInfo
        {
            public Guid PlayerId { get; set; }
            public string Name { get; set; }
            
        }

        /// <summary>
        /// Create a new backend player request
        /// 
        /// BUG: There are no protections against a malicious admin that has players zeus id's
        /// from querying this with a known zeus id
        /// 
        /// TODO: Eventually find a new way in the future with identity management to secure this better
        /// </summary>
        public struct CreatePlayerRequest
        {
            /// <summary>
            /// The players ZEUS id (VU/account specific)
            /// </summary>
            public Guid ZeusId { get; set; }

            /// <summary>
            /// Current name of the player
            /// </summary>
            public string Name { get; set; }
        }
        #endregion

        // Player manager
        private IPlayerManager m_PlayerManager;

        public PlayerController(IPlayerManager p_PlayerManager)
        {
            m_PlayerManager = p_PlayerManager;
        }

        // GET: api/<PlayerController>
        [HttpGet("{p_Key}")]
        public ActionResult<SafePlayerInfo[]> Get(string? p_Key)
        {
            var s_PlayersArray = m_PlayerManager.GetAllPlayers().Select(p_Player => new SafePlayerInfo
            {
                Name = p_Player.Name,
                // If someone used an admin key we return player id's otherwise blank them for security reasons
                PlayerId = p_Key == Program.c_AdminKey ? p_Player.Id : Guid.Empty
            }).ToArray();

            return s_PlayersArray;
        }

        // POST api/<PlayerController>/Info
        [HttpPost("Info")]
        public IActionResult Info(Guid p_PlayerId)
        {
            var s_Player = m_PlayerManager.GetPlayerById(p_PlayerId);
            if (s_Player is null)
                return BadRequest();

            var s_SafePlayerInfo = new SafePlayerInfo
            {
                PlayerId = s_Player.Id,
                Name = s_Player.Name,
            };

            ViewData["PlayerInfo"] = s_SafePlayerInfo;

            return View();
        }

        [HttpPost("Create")]
        public ActionResult<SafePlayerInfo> CreatePlayer([FromBody]CreatePlayerRequest p_Request, string Key)
        {
            if (p_Request.ZeusId == Guid.Empty)
                return BadRequest();

            if (!m_PlayerManager.AddPlayer(p_Request.ZeusId, p_Request.Name, out Player? p_Player))
                return BadRequest();

            return new SafePlayerInfo
            {
                PlayerId = p_Player.Id,
                Name = p_Player.Name
            };
        }
    }
}
