using System;
using System.Collections.Generic;
using vusvc.Models;

namespace vusvc.Managers
{
    public interface IPlayerManager
    {
        bool AddPlayer(Guid p_ZeusId, string p_Name, out Player? p_Player);
        Player? GetPlayerByZeusId(Guid p_ZeusId);
        Player? GetPlayerById(Guid p_Id);
        IEnumerable<Player> GetPlayersByName(string p_NameContains);
        bool Load(string p_Path);
        bool Save(string p_Path);

        IEnumerable<Player> GetAllPlayers();
    }
}
