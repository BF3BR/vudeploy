using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vusvc.Data
{
    public class Player
    {
        /// <summary>
        /// The last display name set for the player
        /// 
        /// Note: If no name exists, use Player_{BackendId} as the name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The linked zeus id for this player
        /// 
        /// Note: This should remain hidden from public
        /// It's not secret or anything, we just want to keep privacy
        /// </summary>
        public Guid ZeusId { get; set; } = Guid.Empty;

        /// <summary>
        /// Backend guid, all lookups/identification should happen through this
        /// </summary>
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Previous names that this player has been seen by
        /// </summary>
        public List<string> PreviousNames { get; set; } = new List<string>();

        public Player()
        {

        }

        public Player(Guid p_ZeusId, string p_Name)
        {
            Name = p_Name;
            PreviousNames.Add(p_Name);
            Id = Guid.NewGuid();
            ZeusId = p_ZeusId;
        }
    }
}
