using System;
using vusvc.Data;
using static vusvc.Data.Server;

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
        /// Gets a server by zeus id
        /// </summary>
        /// <param name="p_ZeusId">Zeus id</param>
        /// <returns>Server, or null if not found or error</returns>
        Server? GetServerByZeusId(Guid p_ZeusId);

        /// <summary>
        /// BUG: This is probably going to change in the future
        /// 
        /// Adds a new server
        /// </summary>
        /// <param name="p_Server">Created server object on success</param>
        /// <param name="p_Unlisted">Should this server be unlisted</param>
        /// <param name="p_BindIp">Binding IP address (ex: "0.0.0.0")</param>
        /// <param name="p_TemplateName">Template to copy from</param>
        /// <param name="p_Frequency">Frequency to spawn</param>
        /// <param name="p_ServerType">Server type</param>
        /// <returns>True on success, false otherwise</returns>
        bool AddServer(out Server? p_Server, bool p_Unlisted, string p_BindIp, string p_TemplateName, ServerInstanceFrequency p_Frequency, ServerInstanceType p_ServerType);

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
        /// <param name="p_DeleteInstanceDirectory">Should we delete the instance directory and contents</param>
        /// <returns>True if success, false otherwise</returns>
        bool TerminateServer(Guid p_ServerId, bool p_DeleteInstanceDirectory);

        /// <summary>
        /// Terminates all running servers
        /// </summary>
        void TerminateAllServers();
    }
}
