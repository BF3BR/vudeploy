using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vusvc.Data;

namespace vusvc.Managers
{
    public interface ILobbyManager
    {
        bool AddLobby(Guid p_PlayerId, ushort p_MaxPlayers, string p_Name, out PlayerLobby p_PlayerLobby);
        bool RemoveLobby(Guid p_LobbyId);
        bool UpdateLobby(Guid p_LobbyId, Guid p_PlayerId);

        bool JoinLobby(Guid p_LobbyId, Guid p_PlayerId, string p_Code);
        bool LeaveLobby(Guid p_LobbyId, Guid p_PlayerId);

        bool SetLobbyAdmin(Guid p_LobbyId, Guid p_PlayerId);

        void ExpireLobbies();

        PlayerLobby? GetLobbyById(Guid p_LobbyId);
        IEnumerable<PlayerLobby> GetLobbiesByName(string p_PartialName);

        IEnumerable<PlayerLobby> GetAllLobbies();
    }
}
