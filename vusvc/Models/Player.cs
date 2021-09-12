using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace vusvc.Models
{
    #region Requests and Responses
    /// <summary>
    /// This is what is returned to the user
    /// 
    /// This strips out some extra information such as the zeus id
    /// and the previous names
    /// </summary>
    public struct SafePlayerInfo
    {
        public Guid PlayerId { get; set; }
        public string Name { get; set; }

    }

    /// <summary>
    /// Create a new backend player request
    /// 
    /// BUG: There are no protections against a malicious admin that has players zeus id's
    /// from querying this with a known zeus id
    /// 
    /// TODO: Eventually find a new way in the future with identity management to secure this better
    /// </summary>
    public struct CreatePlayerRequest
    {
        /// <summary>
        /// The players ZEUS id (VU/account specific)
        /// </summary>
        public Guid ZeusId { get; set; }

        /// <summary>
        /// Current name of the player
        /// </summary>
        public string Name { get; set; }
    }
    #endregion

    [Table("Players")]
    public class Player
    {
        /// <summary>
        /// The last display name set for the player
        /// 
        /// Note: If no name exists, use Player_{BackendId} as the name
        /// </summary>
        [StringLength(64)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The linked zeus id for this player
        /// 
        /// Note: This should remain hidden from public
        /// It's not secret or anything, we just want to keep privacy
        /// </summary>
        [Required]
        public Guid ZeusId { get; set; } = Guid.Empty;

        /// <summary>
        /// Backend guid, all lookups/identification should happen through this
        /// </summary>
        [Key]
        [Required]
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Previous names that this player has been seen by
        /// </summary>
        public List<string> PreviousNames { get; set; } = new List<string>();

        public const int c_MaxNameLength = 64;

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
