using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using vusvc.Data;
using vusvc.Managers;
using vusvc.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace vusvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayerController : Controller
    {
        // Player manager
        private IPlayerManager m_PlayerManager;

        public PlayerController(IPlayerManager p_PlayerManager)
        {
            m_PlayerManager = p_PlayerManager;
        }

        // GET: api/<PlayerController>
        [HttpGet]
        public ActionResult<SafePlayerInfo[]> Get()
        {
            var s_PlayersArray = m_PlayerManager.GetAllPlayers().Select(p_Player => new SafePlayerInfo
            {
                Name = p_Player.Name,
                // If someone used an admin key we return player id's otherwise blank them for security reasons
                PlayerId = /*p_Key == Program.c_AdminKey ? p_Player.Id : */Guid.Empty
            }).ToArray();

            return s_PlayersArray;
        }

        [HttpGet("Info/{p_PlayerId}")]
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
