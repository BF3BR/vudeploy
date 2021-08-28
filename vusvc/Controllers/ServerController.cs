using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // GET: api/<ServerController>
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["Servers"] = (m_ServerManager as ServerManager).Servers.ToArray();

            return View();
        }

        // GET api/<ServerController>/5
        [HttpGet("{p_ServerId}")]
        public ActionResult<Server> Get(Guid p_ServerId, string p_Key)
        {
            if (p_Key != Program.c_AdminKey)
                return NotFound();

            var s_Server = m_ServerManager.GetServerById(p_ServerId);
            if (s_Server is null)
                return BadRequest();

            return s_Server;
        }

        [HttpGet("output/{p_ServerId}")]
        public ActionResult<string> GetOutput(Guid p_ServerId)
        {
            var s_Server = m_ServerManager.GetServerById(p_ServerId);
            if (s_Server is null)
                return BadRequest();

            return HttpUtility.HtmlEncode(s_Server.OutputLog);
        }

        [HttpGet("error/{p_ServerId}")]
        public ActionResult<string> GetError(Guid p_ServerId)
        {
            var s_Server = m_ServerManager.GetServerById(p_ServerId);
            if (s_Server is null)
                return BadRequest();

            return HttpUtility.HtmlEncode(s_Server.ErrorLog);

        }

        [HttpGet("SpawnServer")]
        public ActionResult<Server> SpawnServer()
        {
            if (!(m_ServerManager as ServerManager).SpawnServer(out Server? s_Server))
                return BadRequest();

            return s_Server;
        }

        // POST api/<ServerController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<ServerController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<ServerController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
