using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using vusvc.Data;
using vusvc.Managers;

namespace vusvc.Controllers
{
    public class MatchController : Controller
    {
        private ILobbyManager m_LobbyManager;
        private IMatchManager m_MatchManager;

        public MatchController(ILobbyManager p_LobbyManager, IMatchManager p_MatchManager)
        {
            m_LobbyManager = p_LobbyManager;
            m_MatchManager = p_MatchManager;
        }

        [HttpPost]
        public ActionResult QueueLobby(Guid p_LobbyId)
        {
            var s_Lobby = m_LobbyManager.GetLobbyById(p_LobbyId);
            if (s_Lobby is null)
                return BadRequest();

            if (!m_MatchManager.QueueLobby(p_LobbyId))
                return BadRequest(); // TODO: Find a better error for this

            return Ok();
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        public ActionResult<Match> GetMatchById(Guid p_MatchId)
        {
            var s_Match = m_MatchManager.GetMatchById(p_MatchId);
            if (s_Match is null)
                return BadRequest();

            return s_Match;
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        public ActionResult<Match> GetMatchByPlayerId(Guid p_PlayerId)
        {
            var s_Match = m_MatchManager.GetMatchByPlayerId(p_PlayerId);
            if (s_Match is null)
                return BadRequest();

            return s_Match;
        }

        // GET: MatchController
        public ActionResult Index()
        {
            return View();
        }

        // GET: MatchController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: MatchController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: MatchController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MatchController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: MatchController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: MatchController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: MatchController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
