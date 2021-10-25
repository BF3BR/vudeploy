using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using vusvc.Data;
using vusvc.Managers;
using static vusvc.Models.Match;

namespace vusvc.Controllers
{
    public class MatchController : Controller
    {
        private ILobbyManager m_LobbyManager;
        private IMatchManager m_MatchManager;
        private IServerManager m_ServerManager;

        public MatchController(ILobbyManager p_LobbyManager, IMatchManager p_MatchManager, IServerManager p_ServerManager)
        {
            m_LobbyManager = p_LobbyManager;
            m_MatchManager = p_MatchManager;
            m_ServerManager = p_ServerManager;
        }

        [HttpPost("Queue")]
        [Consumes(MediaTypeNames.Application.Json)]
        public ActionResult QueueLobby(QueueLobbyRequest p_Request)
        {
            // Get the lobby to make sure it exists
            var s_Lobby = m_LobbyManager.GetLobbyById(p_Request.LobbyId);
            if (s_Lobby is null)
                return BadRequest();

            // If the person requesting to queue is not the admin player ignore the request
            if (s_Lobby.AdminPlayerId != p_Request.PlayerId)
                return BadRequest();

            if (!m_MatchManager.QueueLobby(s_Lobby.LobbyId))
                return BadRequest(); // TODO: Find a better error for this

            return Ok();
        }

        [HttpPost("Dequeue")]
        [Consumes(MediaTypeNames.Application.Json)]
        public ActionResult DequeueLobby(DequeueLobbyRequest p_Request)
        {
            // Get the lobby to make sure it exists
            var s_Lobby = m_LobbyManager.GetLobbyById(p_Request.LobbyId);
            if (s_Lobby is null)
                return BadRequest();

            // If the person requesting to queue is not the admin player ignore the request
            if (s_Lobby.AdminPlayerId != p_Request.PlayerId)
                return BadRequest();

            if (!m_MatchManager.DequeueLobby(s_Lobby.LobbyId))
                return BadRequest();

            return Ok();
        }

        [HttpPost("GetMatchById")]
        [Consumes(MediaTypeNames.Application.Json)]
        public ActionResult<Match> GetMatchById(Guid p_MatchId)
        {
            var s_Match = m_MatchManager.GetMatchById(p_MatchId);
            if (s_Match is null)
                return BadRequest();

            return s_Match;
        }

        [HttpPost("GetMatchByPlayerId")]
        [Consumes(MediaTypeNames.Application.Json)]
        public ActionResult<GetMatchByPlayerIdResponse> GetMatchByPlayerId(GetMatchByPlayerIdRequest p_Request)
        {
            var s_Match = m_MatchManager.GetMatchByPlayerId(p_Request.PlayerId);
            if (s_Match is null)
                return BadRequest();

            return new GetMatchByPlayerIdResponse
            {
                MatchId = s_Match.MatchId
            };
        }

        [HttpPost("GetMatchConnectionInfo")]
        [Consumes(MediaTypeNames.Application.Json)]
        public ActionResult<GetMatchConnectionInfoResponse> GetMatchConnectionInfo(GetMatchConnectionInfoRequest p_Request)
        {
            // Get the match if it exists
            var s_Match = m_MatchManager.GetMatchById(p_Request.MatchId);
            if (s_Match is null)
                return BadRequest();

            // Check to make sure that the player id is in the match
            if (!s_Match.Players.Contains(p_Request.PlayerId))
                return BadRequest();

            // If there is no server available we don't want to do anything
            if (s_Match.ServerId == Guid.Empty)
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);

            // Get the server
            var s_Server = m_ServerManager.GetServerById(s_Match.ServerId);
            if (s_Server is null)
                return BadRequest();

            // CHeck to see if the server has come online and populated the zeus id
            if (s_Server.ZeusId == Guid.Empty)
                return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);

            // Return the final connection response
            return new GetMatchConnectionInfoResponse
            {
                MHarmonyPort = s_Server.MonitoredHarmonyPort,
                Password = s_Server.GamePassword,
                Port = s_Server.GamePort,
                ZeusConnectionId = s_Server.ZeusId
            };
        }

        [HttpPost("GetMatchInfo")]
        [Consumes(MediaTypeNames.Application.Json)]
        public ActionResult<GetMatchInfoResponse> GetMatchInfo(GetMatchInfoRequest p_Request)
        {
            var s_Match = m_MatchManager.GetMatchByServerZeusId(p_Request.ZeusId);
            if (s_Match is null)
                return BadRequest();

            var s_Teams = m_MatchManager.GetMatchPlayerTeams(s_Match.MatchId);

            return new GetMatchInfoResponse
            {
                MatchId = s_Match.MatchId,
                PlayerLobbyIds = s_Teams
            };
        }

        [HttpPost("SetMatchState")]
        [Consumes(MediaTypeNames.Application.Json)]
        public ActionResult SetMatchState(SetMatchStateRequest p_Request)
        {
            // TODO: MatchManager needs to handle the transition/state change
            // This way we can update the pending->current etc

            var s_Match = m_MatchManager.GetMatchById(p_Request.MatchId);
            if (s_Match is null)
                return BadRequest();

            var s_Server = m_ServerManager.GetServerById(s_Match.ServerId);
            if (s_Server is null)
                return BadRequest();

            // Make sure the match server id matches the requesting server id
            if (s_Server.ZeusId != p_Request.ServerZeusId)
                return BadRequest();

            // We only want the server to be able to set the ingame or waiting states
            switch (p_Request.State)
            {
                case MatchState.InGame:
                case MatchState.Waiting:
                    (s_Match as MatchManager.MatchExt).State = p_Request.State;
                    break;
                default:
                    return BadRequest();
            }

            return Ok();
        }

        [HttpPost("SetMatchCompleted")]
        [Consumes(MediaTypeNames.Application.Json)]
        public IActionResult SetMatchCompleted(SetMatchCompletedRequest p_Request)
        {
            var s_Match = m_MatchManager.GetMatchById(p_Request.MatchId);
            if (s_Match is null)
                return BadRequest();

            var s_Server = m_ServerManager.GetServerById(s_Match.ServerId);
            if (s_Server is null)
                return BadRequest();

            if (s_Server.ZeusId != p_Request.ServerZeusId)
                return BadRequest();

            if (!m_MatchManager.SetMatchCompletedById(s_Match.MatchId, p_Request.Winners, p_Request.Players))
                return BadRequest();

            
            return Ok();
        }
    }
}
