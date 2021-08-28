using System;
using vusvc.Data;

namespace vusvc.Managers
{
    /// <summary>
    /// Interface for a server manager, this is what is used across the vusvc service
    /// </summary>
    public interface IServerManager
    {
        /// <summary>
        /// Gets a server by id
        /// </summary>
        /// <param name="p_ServerId">Server id</param>
        /// <returns>Server, or null if not found or error</returns>
        Server? GetServerById(Guid p_ServerId);

        /// <summary>
        /// Removes a server from the server manager
        /// </summary>
        /// <param name="p_ServerId">Server id to remove</param>
        /// <param name="p_Terminate">Should the server be terminated if running</param>
        bool RemoveServer(Guid p_ServerId, bool p_Terminate = true);

        /// <summary>
        /// Removes all servers from the server manager
        /// </summary>
        /// <param name="p_Terminate">Should the servers be terminated if running</param>
        void RemoveAllServers(bool p_Terminate = true);

        /// <summary>
        /// Terminates a servers process
        /// </summary>
        /// <param name="p_ServerId">Server id</param>
        /// <returns>True if success, false otherwise</returns>
        bool TerminateServer(Guid p_ServerId);

        /// <summary>
        /// Terminates all running servers
        /// </summary>
        void TerminateAllServers();
    }
}
