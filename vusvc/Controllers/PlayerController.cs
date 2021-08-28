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
    public class PlayerController : ControllerBase
    {
        private IPlayerManager m_PlayerManager;

        public PlayerController(IPlayerManager p_PlayerManager)
        {
            m_PlayerManager = p_PlayerManager;
        }

        // GET: api/<PlayerController>
        [HttpGet]
        public ActionResult<Player[]> Get(string Key)
        {
            return m_PlayerManager.GetAllPlayers().ToArray();
        }

        /// <summary>
        /// This is what is returned to the user
        /// 
        /// This strips out some extra information such as the zeus id
        /// and the previous names
        /// </summary>
        public struct SafePlayerInfo
        {
            public string Name { get; set; }
            public Guid Id { get; set; }
        }

        // GET api/<PlayerController>/5
        [HttpGet("{id}")]
        public ActionResult<SafePlayerInfo> Get(Guid p_PlayerId)
        {
            var s_Player = m_PlayerManager.GetPlayerById(p_PlayerId);
            if (s_Player is null)
                return BadRequest();

            return new SafePlayerInfo
            {
                Id = s_Player.Id,
                Name = s_Player.Name,
            };
        }

        public struct CreatePlayerRequest
        {
            public Guid ZeusId { get; set; }
            public string Name { get; set; }
        }

        [HttpPost("CreatePlayer")]
        public ActionResult<SafePlayerInfo> CreatePlayer([FromBody]CreatePlayerRequest p_Request, string Key)
        {
            if (p_Request.ZeusId == Guid.Empty)
                return BadRequest();

            if (!m_PlayerManager.AddPlayer(p_Request.ZeusId, p_Request.Name, out Player? p_Player))
                return BadRequest();

            return new SafePlayerInfo
            {
                Id = p_Player.Id,
                Name = p_Player.Name
            };
        }
    }
}
