using System;
using System.Collections.Generic;

namespace vusvc.Data
{
    /// <summary>
    /// Representation of a completed match
    /// </summary>
    public class Match
    {
        /// <summary>
        /// Match id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Server id (will probably not exist any more)
        /// </summary>
        public Guid ServerId { get; set; }

        /// <summary>
        /// Game start time
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Game end time
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Player id's of the winners
        /// </summary>
        public List<Guid> Winners { get; set; }

        /// <summary>
        /// Player id's of all players
        /// </summary>
        public List<Guid> Players { get; set; }
    }
}
