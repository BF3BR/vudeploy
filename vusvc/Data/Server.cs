using System;
using System.Collections.Generic;

namespace vusvc.Data
{
    /// <summary>
    /// Representation of a server
    /// </summary>
    public class Server
    {
        /// <summary>
        /// The type of server that this is
        /// 
        /// Are we pulling people from a lobby server where players will be joined
        /// to another game server
        /// 
        /// A game server, which is actually running the game mode required
        /// 
        /// 
        /// </summary>
        public enum ServerInstanceType
        {
            /// <summary>
            /// Invalid state
            /// </summary>
            Undefined,

            /// <summary>
            /// This server is responsible for holding players before being joined to the game server
            /// </summary>
            Lobby,

            /// <summary>
            /// Game server running the requested game type
            /// </summary>
            Game,

            /// <summary>
            /// Count within this enum
            /// </summary>
            COUNT
        }

        /// <summary>
        /// Backend id for this server instance
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Zeus id that is parsed from the output log
        /// 
        /// Do not rely on this for any actual information
        /// </summary>
        public Guid ZeusId { get; set; } = Guid.Empty;

        /// <summary>
        /// Type of server
        /// </summary>
        public ServerInstanceType ServerType { get; set; } = ServerInstanceType.Undefined;

        /// <summary>
        /// Display name of server
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Game password
        /// </summary>
        public string GamePassword { get; set; }

        /// <summary>
        /// Rcon password
        /// </summary>
        public string RconPassword { get; set; }

        /// <summary>
        /// Game port (to prevent conflicts)
        /// </summary>
        public ushort GamePort { get; set; }

        /// <summary>
        /// Rcon port (for remote handling)
        /// </summary>
        public ushort RconPort { get; set; }

        /// <summary>
        /// Monitored harmony port (for VeniceEXT/VU functionality)
        /// </summary>
        public ushort MonitoredHarmonyPort { get; set; }

        /// <summary>
        /// Players that are in this server
        /// WARN: This is not expected to be reliable
        /// </summary>
        public List<Guid> PlayerIds { get; set; } = new List<Guid>();

        public string ErrorLog { get; set; }
        public string OutputLog { get; set; }
    }
}
