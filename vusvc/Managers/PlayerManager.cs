using System;
using System.Collections.Generic;
using System.Linq;
using vusvc.Data;
using System.Text.Json;
using System.IO;
using vusvc.Extensions;

namespace vusvc.Managers
{
    public class PlayerManager : IPlayerManager
    {
        // List of all of the players
        private List<Player> m_Players = new List<Player>();

        private const string c_DefaultDatabasePath = "./players.json";
        private const int c_DefaultMaxPlayerName = 32;

        public PlayerManager()
        {
            // Attempt to automatically load the database if it exsts
            if (!Load())
                Console.WriteLine($"WARN: No database provided, no {c_DefaultDatabasePath} found.");
        }

        public bool Load(string p_Path = c_DefaultDatabasePath)
        {
            try
            {
                var s_Data = File.ReadAllText(p_Path);
                var s_Players = JsonSerializer.Deserialize<Player[]>(s_Data);

                m_Players.Clear();
                m_Players.AddRange(s_Players);
            }
            catch (Exception p_Exception)
            {
                Console.WriteLine($"err: could not load database ({p_Path}) ({p_Exception}).");
                return false;
            }

            return true;
        }

        public bool Save(string p_Path = c_DefaultDatabasePath)
        {
            var s_Data = JsonSerializer.Serialize(m_Players, new JsonSerializerOptions
            {
                WriteIndented = true,
            });

            try
            {
                File.WriteAllText(p_Path, s_Data);
            }
            catch (Exception p_Exception)
            {
                Console.Write($"err: could not write database ({p_Path}) ({p_Exception}).");
                return false;
            }

            return true;
        }

        public bool AddPlayer(Guid p_ZeusId, string p_Name, out Player? p_Player)
        {
            // Set our default value
            p_Player = null;

            // Santize the name
            var s_SanitizedName = p_Name.Sanitize();
            if (s_SanitizedName.Length > c_DefaultMaxPlayerName)
                s_SanitizedName = s_SanitizedName.Substring(0, c_DefaultMaxPlayerName);

            // Attempt to get an existing player
            var s_ExistingPlayer = GetPlayerByZeusId(p_ZeusId);

            // If the player exists then we will try and update the previous name
            if (s_ExistingPlayer is not null)
            {
                // Assign our existing player
                p_Player = s_ExistingPlayer;

                // Check if the player name matches
                if (s_ExistingPlayer.Name == s_SanitizedName)
                    return true;

                // Check the existing names to see if this player already contains the current name
                if (s_ExistingPlayer.PreviousNames.Contains(s_SanitizedName))
                    return true;

                // Update the previous names
                s_ExistingPlayer.Name = s_SanitizedName;

                // Add this name to previous names
                s_ExistingPlayer.PreviousNames.Add(s_SanitizedName);

                return true;
            }

            // Create a new player
            var s_Player = new Player(p_ZeusId, s_SanitizedName);

            // Check to see if the id is a duplicate, and regenerate the id
            while (GetPlayerById(s_Player.Id) != null)
                s_Player.Id = Guid.NewGuid();

            // Add the player
            m_Players.Add(s_Player);

            // Assign our output player
            p_Player = s_Player;

            return true;
        }

        public Player? GetPlayerByZeusId(Guid p_ZeusId)
        {
            return m_Players.FirstOrDefault(p_Player => p_Player.ZeusId == p_ZeusId);
        }

        public Player? GetPlayerById(Guid p_Id)
        {
            return m_Players.FirstOrDefault(p_Player => p_Player.Id == p_Id);
        }

        public IEnumerable<Player> GetPlayersByName(string p_NameContains)
        {
            return m_Players.Where(p_Player => p_Player.Name.Contains(p_NameContains));
        }

        public IEnumerable<Player> GetAllPlayers()
        {
            return m_Players;
        }
    }
}
