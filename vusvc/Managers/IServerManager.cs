using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using vusvc.Data;

namespace vusvc.Managers
{
    public interface IServerManager
    {
        Server? GetServerById(Guid p_ServerId);

        bool KillServer(Guid p_ServerId);

        void KillAllServers();
    }
}
