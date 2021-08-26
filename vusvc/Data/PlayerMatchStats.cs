using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vusvc.Data
{
    public class PlayerMatchStats
    {
        /// <summary>
        /// Match stats id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Player id
        /// </summary>
        public Guid PlayerId { get; set; }

        /// <summary>
        /// Match id
        /// </summary>
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
        public long Accuracy { get; set; }
    }
}
