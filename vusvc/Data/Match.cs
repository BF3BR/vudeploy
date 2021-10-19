using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vusvc.Data
{
    public enum MatchState
    {
        /// <summary>
        /// Invalid state
        /// </summary>
        Invalid = 0,

        /// <summary>
        /// Match is queued and pending matchmaking
        /// </summary>
        Queued,

        /// <summary>
        /// Server is waiting for startup, or players to join
        /// </summary>
        Waiting,

        /// <summary>
        /// Match has started
        /// </summary>
        InGame,
        COUNT
    }

    /// <summary>
    /// Representation of a completed match
    /// </summary>
    [Table("Matches")]
    public class Match
    {
        /// <summary>
        /// Match id
        /// </summary>
        [Key]
        [Required]
        public Guid MatchId { get; set; }

        /// <summary>
        /// Server id (will probably not exist any more)
        /// </summary>
        [Required]
        public Guid ServerId { get; set; }

        /// <summary>
        /// Game start time
        /// </summary>
        [Required]
        public DateTime GameStartTime { get; set; }

        /// <summary>
        /// Game end time
        /// </summary>
        [Required]
        public DateTime GameEndTime { get; set; }

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
