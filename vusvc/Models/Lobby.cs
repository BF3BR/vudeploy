using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace vusvc.Models
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

    #region Database
    [Table("Players")]
    public class DbPlayer
    {
        [Key]
        [Required]
        public Guid PlayerId { get; set; }

        [StringLength(32)]
        public String PlayerName { get; set; }
    }
    #endregion
}
