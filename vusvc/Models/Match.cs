using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vusvc.Data;

namespace vusvc.Models
{
    public class Match
    {
        /// <summary>
        /// Queues a lobby for a new match
        /// </summary>
        public struct QueueLobbyRequest
        {
            /// <summary>
            /// Created lobby id holding all players
            /// </summary>
            public Guid LobbyId { get; set; }

            /// <summary>
            /// Player that is requeusting to queue
            /// (Only the admin can queue for matchmaking)
            /// </summary>
            public Guid RequestingPlayerId { get; set; }
        }

        /// <summary>
        /// Dequeues a lobby from searching for a new match
        /// </summary>
        public struct DequeueLobbyRequest
        {
            /// <summary>
            /// Lobby id
            /// </summary>
            public Guid LobbyId { get; set; }

            /// <summary>
            /// Player that is requeusting to queue
            /// (only the admin can queue for matchmaking)
            /// </summary>
            public Guid RequestingPlayerId { get; set; }
        }

        /// <summary>
        /// Requests the match state
        /// </summary>
        public struct GetMatchStateRequest
        {
            /// <summary>
            /// Match id to get state of
            /// </summary>
            public Guid MatchId { get; set; }

            /// <summary>
            /// Requesting player id
            /// </summary>
            public Guid PlayerId { get; set; }
        }

        /// <summary>
        /// Match state response
        /// </summary>
        public struct GetMatchStatusResponse
        {
            /// <summary>
            /// The current state of the match
            /// </summary>
            public MatchState State { get; set; }
        }

        /// <summary>
        /// For people who crash, they can request the last available match they were in if it's still active
        /// </summary>
        public struct GetMatchByPlayerIdRequest
        {
            /// <summary>
            /// Player id
            /// </summary>
            public Guid PlayerId { get; set; }
        }

        /// <summary>
        /// Get match by player id response
        /// </summary>
        public struct GetMatchByPlayerIdResponse
        {
            /// <summary>
            /// The match id information of a match
            /// </summary>
            public Guid MatchId { get; set; }
        }

        /// <summary>
        /// Once a player gets a match id, get the connection info for frostbite
        /// 
        /// The server will validate that the requesting player id is in the specififed match id
        /// </summary>
        public struct GetMatchConnectionInfoRequest
        {
            /// <summary>
            /// Requesting player id
            /// </summary>
            public Guid PlayerId { get; set; }

            /// <summary>
            /// Match id to get connection info for
            /// </summary>
            public Guid MatchId { get; set; }
        }

        /// <summary>
        /// Respoonse for frostbite connection information
        /// </summary>
        public struct GetMatchConnectionInfoResponse
        {
            /// <summary>
            /// The servers zeus connection id
            /// </summary>
            public Guid ZeusConnectionId { get; set; }

            /// <summary>
            /// Password of the server
            /// </summary>
            public string Password { get; set; }

            /// <summary>
            /// Port of the server (unused in vext)
            /// </summary>
            public ushort Port { get; set; }

            /// <summary>
            /// MHarmony port of server (unused in vext)
            /// </summary>
            public ushort MHarmonyPort { get; set; }
        }

        /// <summary>
        /// SERVER REQUEST
        /// 
        /// This is requested by the server to get a waiting matches team information
        /// </summary>
        public struct GetMatchInfoRequest
        {
            /// <summary>
            /// Server Zeus Id
            /// </summary>
            public Guid ZeusId { get; set; }
        }

        /// <summary>
        /// SERVER RESPONSE
        /// 
        /// This is the response of the match id and team information that the server will need
        /// to make further requests
        /// </summary>
        public struct GetMatchInfoResponse
        {
            /// <summary>
            /// Match id for this server
            /// </summary>
            public Guid MatchId { get; set; }

            /// <summary>
            /// Player team guids in
            /// 
            /// PlayerZeusId, PlayerLobbyId
            /// </summary>
            public Dictionary<Guid, Guid> PlayerLobbyIds { get; set; }
        }

        public struct SetMatchStateRequest
        {
            public Guid ServerZeusId { get; set; }
            public Guid MatchId { get; set; }
            public MatchState State { get; set; }
        }

        public struct SetMatchCompletedRequest
        {
            public Guid ServerZeusId { get; set; }
            public Guid MatchId { get; set; }
            public List<Guid> Winners { get; set; }
            public List<Guid> Players { get; set; }
        }
    }
}
