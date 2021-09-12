using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace vusvc.Data
{
    [Table("MatchStats")]
    public class PlayerMatchStats
    {
        /// <summary>
        /// Match stats id
        /// </summary>
        [Key]
        [Required]
        public Guid StatsId { get; set; }

        /// <summary>
        /// Player id
        /// </summary>
        [Required]
        public Guid PlayerId { get; set; }

        /// <summary>
        /// Match id
        /// </summary>
        [Required]
        public Guid MatchId { get; set; }

        /// <summary>
        /// Damage done to other players
        /// </summary>
        public ulong Damage { get; set; }

        /// <summary>
        /// Headshots
        /// </summary>
        public ulong Headshots { get; set; }

        /// <summary>
        /// Kills
        /// </summary>
        public ulong Kills { get; set; }

        /// <summary>
        /// Deaths
        /// </summary>
        public ulong Deaths { get; set; }

        /// <summary>
        /// Knockdowns
        /// </summary>
        public ulong Knockdowns { get; set; }

        /// <summary>
        /// Player score
        /// </summary>
        public long Score { get; set; }

        /// <summary>
        /// Accuracy
        /// </summary>
        public double Accuracy { get; set; }
        
        /// <summary>
        /// Team Id
        /// </summary>
        [Required]
        public int TeamId { get; set; }

        /// <summary>
        /// Squad Id
        /// </summary>
        [Required]
        public int SquadId { get; set; }
    }
}
