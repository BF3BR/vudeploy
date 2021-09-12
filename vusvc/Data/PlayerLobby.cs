using System;
using System.Collections.Generic;
using System.Linq;

namespace vusvc.Data
{
    /// <summary>
    /// Representation of a player lobby
    /// </summary>
    public class PlayerLobby
    {
        public enum LobbySearchLockType
        {
            /// <summary>
            /// No players should be added or removed from this lobby
            /// </summary>
            Locked,

            /// <summary>
            /// Players in this lobby are free to be merged, removed, or players added
            /// </summary>
            Unlocked,
        }

        /// <summary>
        /// Id of this lobby
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Display name of the lobby
        /// 
        /// Note: If none is set default to lobby_{Id}
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Maximum number of players allowed in a lobby
        /// </summary>
        public ushort MaxPlayers { get; set; } = 4;

        /// <summary>
        /// The admin player id
        /// </summary>
        public Guid AdminPlayerId { get; set; } = Guid.Empty;

        /// <summary>
        /// Lobby player ids
        /// </summary>
        public List<Guid> PlayerIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Passcode to join a lobby
        /// </summary>
        public string Code { get; set; } = "0000";

        /// <summary>
        /// Lobby creation time, this will know which lobbies to automatically retire
        /// 
        /// When a lobby is updated to keep it in existence, just set the creation time to current
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// This will tell the backend how we want to handle this lobby.
        /// 
        /// See documentation for: LobbySearchLockType for usage
        /// 
        /// By default, all lobbies are unlocked
        /// </summary>
        public LobbySearchLockType SearchLockType { get; set; } = LobbySearchLockType.Unlocked;

        // Private generator for new codes
        private static Random m_Random = new Random();

        public static string GenerateCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 4)
              .Select(s => s[m_Random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Updates the lobby's creation time
        /// 
        /// NOTE: This can and probably will be expanded in the future
        /// </summary>
        public void Update()
        {
            CreationTime = DateTime.Now;
        }
    }
}
