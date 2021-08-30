using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using vusvc.Data;
using vusvc.Managers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace vusvc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServerController : Controller
    {
        public struct ServerView
        {

        }
        private readonly IServerManager m_ServerManager;

        public ServerController(IServerManager p_ServerManager)
        {
            m_ServerManager = p_ServerManager;
        }

        /// <summary>
        /// Server List View
        /// </summary>
        /// <param name="p_Key"></param>
        /// <returns></returns>
        public IActionResult GetServerList(string p_Key)
        {
            //if (p_Key != Program.c_AdminKey)
            //    return NotFound();

            ViewData["Servers"] = (m_ServerManager as ServerManager).Servers.ToArray();
            ViewData["ServerManager"] = m_ServerManager;

            return View();
        }

        // GET api/<ServerController>/5
        [HttpGet("{p_ServerId}")]
        public ActionResult<Server> Get(Guid p_ServerId, string p_Key)
        {
            //if (p_Key != Program.c_AdminKey)
            //    return NotFound();

            var s_Server = m_ServerManager.GetServerById(p_ServerId);
            if (s_Server is null)
                return BadRequest();

            return s_Server;
        }

        // GET api/<ServerController>/output/{p_ServerId}
        [HttpGet("Output/{p_ServerId}")]
        public ActionResult<string> GetOutput(Guid p_ServerId)
        {
            var s_Server = m_ServerManager.GetServerById(p_ServerId);
            if (s_Server is null)
                return BadRequest();

            return HttpUtility.HtmlEncode(s_Server.OutputLog);
        }

        // GET api/<ServerController>/error/{p_ServerId}
        [HttpGet("Error/{p_ServerId}")]
        public ActionResult<string> GetError(Guid p_ServerId)
        {
            var s_Server = m_ServerManager.GetServerById(p_ServerId);
            if (s_Server is null)
                return BadRequest();

            return HttpUtility.HtmlEncode(s_Server.ErrorLog);

        }

        public struct RemoveServerRequest
        {
            public Guid ServerId { get; set; }
            public bool Terminate { get; set; }
        }

        [HttpPost("Remove")]
        [Consumes(MediaTypeNames.Application.Json)]
        public IActionResult RemoveServer([FromBody] RemoveServerRequest p_Request)
        {
            //if (p_Key != Program.c_AdminKey)
            //    return NotFound();

            if (!m_ServerManager.RemoveServer(p_Request.ServerId, p_Request.Terminate))
                return BadRequest("could not remove server");

            return Ok();
        }

#if DEBUG
        // GET api/<ServerController>/DebugSpawnServer
        [HttpGet("DebugSpawnServer")]
        public ActionResult<Server> SpawnServer(string p_Key)
        {
            //if (p_Key != Program.c_AdminKey)
            //    return NotFound();

            if (!(m_ServerManager as ServerManager).Debug_SpawnServer(out Server? s_Server))
                return BadRequest();

            return s_Server;
        }
#endif

        public struct CreateServerRequest
        {
            public bool Unlisted { get; set; }
            public string Template { get; set; }
            public Server.ServerInstanceFrequency Frequency { get; set; }
            public Server.ServerInstanceType ServerType { get; set; }
        }

        [HttpPost("Create")]
        [Consumes(MediaTypeNames.Application.Json)]
        public ActionResult<Server> CreateServer([FromBody]CreateServerRequest p_Request, string p_Key)
        {
            //if (p_Key != Program.c_AdminKey)
            //    return NotFound();

            if (!m_ServerManager.AddServer(out Server? p_Server, p_Request.Unlisted, "0.0.0.0", p_Request.Template, p_Request.Frequency, p_Request.ServerType))
                return BadRequest("server creation failed");

            return p_Server;
        }
    }
}
