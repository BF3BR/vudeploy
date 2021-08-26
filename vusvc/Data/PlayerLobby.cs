﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace vusvc.Data
{
    /// <summary>
    /// Representation of a player lobby
    /// </summary>
    public class PlayerLobby
    {
        private static Random m_Random = new Random();

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

        public static string GenerateCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 4)
              .Select(s => s[m_Random.Next(s.Length)]).ToArray());
        }
    }
}
