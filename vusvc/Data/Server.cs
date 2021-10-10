using System;
using System.Collections.Generic;

namespace vusvc.Data
{
    /// <summary>
    /// Representation of a server
    /// </summary>
    public class Server
    {
        public class ZeusIdUpdatedEventArgs : EventArgs
        {
            public Guid ZeusId { get; set; }
        }

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
        /// Enum for all of the high frequency options that are available to vu
        /// </summary>
        public enum ServerInstanceFrequency
        {
            /// <summary>
            /// 30-hz, Battlefield default
            /// </summary>
            Frequency30,

            /// <summary>
            /// 60-hz
            /// </summary>
            Frequency60,

            /// <summary>
            /// 120-hz
            /// </summary>
            Frequency120,

            /// <summary>
            /// Count within this enum
            /// </summary>
            COUNT
        }

        /// <summary>
        /// Backend id for this server instance
        /// </summary>
        public Guid ServerId { get; set; } = Guid.Empty;

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
        /// Server frequncy (default: 30hz)
        /// </summary>
        public ServerInstanceFrequency ServerFrequency { get; set; } = ServerInstanceFrequency.Frequency30;

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

        /// <summary>
        /// Error log data
        /// </summary>
        public string ErrorLog { get; set; }

        /// <summary>
        /// Output log data
        /// </summary>
        public string OutputLog { get; set; }

        /// <summary>
        /// Server.key path
        /// </summary>
        public string KeyPath { get; set; }

        /// <summary>
        /// Event for handling when a server terminates
        /// </summary>
        public event EventHandler ServerTerminated;

        /// <summary>
        /// Event for handling when a zeus id changes
        /// </summary>
        public event EventHandler ZeusIdUpdated;

        public virtual void OnTerminated()
        {
            ServerTerminated?.Invoke(this, new EventArgs());
        }

        public virtual void OnZeusIdUpdated(Guid p_ZeusId)
        {
            ZeusIdUpdated?.Invoke(this, new ZeusIdUpdatedEventArgs
            {
                ZeusId = p_ZeusId
            });
        }
    }
}
