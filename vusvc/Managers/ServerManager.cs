using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using vusvc.Data;

namespace vusvc.Managers
{
    public class ServerManager : IServerManager
    {
        /// <summary>
        /// Temporary class for holding extra information about a server
        /// </summary>
        public class Win32Server
        {
            /// <summary>
            /// The server model that is used within vusvc
            /// </summary>
            public Server _Server { get; set; }

            /// <summary>
            /// Win32 process that is used for tracking and I/O
            /// </summary>
            public Process _Process { get; set; }

            /// <summary>
            /// Instance folder path of this server
            /// </summary>
            public string InstancePath { get; set; }

            /// <summary>
            /// Task set to be waited upon for process exit
            /// </summary>
            public Task WaitTask { get; set; }
        }

        // List of servers with extra information
        // TODO: In the future, make Win32Server generic and allow linux/osx support (or fuck it, smd)
        private List<Win32Server> m_Servers;

        /// <summary>
        /// Public view of the current servers, only providing the Server model hiding away process/task information
        /// </summary>
        public IEnumerable<Server> Servers => m_Servers.Select(p_Win32Server => p_Win32Server._Server);

        /// <summary>
        /// Maximum amount of servers that are allowed to be created
        /// </summary>
        public ushort MaxServers { get; set; }

        /// <summary>
        /// Starting game port (default: 25200)
        /// </summary>
        public ushort GamePortStart { get; set; }

        /// <summary>
        /// Ending game port (default: GamePortStart + MaxServers)
        /// </summary>
        public ushort GamePortEnd { get; set; }

        /// <summary>
        /// Starting rcon port (default: 47200)
        /// </summary>
        public ushort RconPortStart { get; set; }

        /// <summary>
        /// Ending rcon port (default: RconPortStart + MaxServers)
        /// </summary>
        public ushort RconPortEnd { get; set; }

        /// <summary>
        /// Starting Monitored Harmony port (default: 7948)
        /// </summary>
        public ushort MHarmonyPortStart { get; set; }

        /// <summary>
        /// Ending Monitoried Harmony port (default: MHarmonyPortStart + MaxServers)
        /// </summary>
        public ushort MHarmonyPortEnd { get; set; }

        /// <summary>
        /// Battlefield 3 game files directory (default: Pulled from registry)
        /// </summary>
        public string GameFilesDirectory { get; set; }

        /// <summary>
        /// Venice Unleashed files directory
        /// </summary>
        public string VUExecutableDirectory { get; set; }

        /// <summary>
        /// Directory where new instances are created
        /// </summary>
        public string InstancesDirectory { get; set; }

        // Default server port
        private const ushort c_DefaultServerPort = 25200;

        // Default rcon port
        private const ushort c_DefaultRconPort = 47200;

        // Default monotoried harmony port
        private const ushort c_DefaultMonitoriedHarmonyPort = 7948;

        // Default server count
        private const ushort c_DefaultServerCount = 4;

        // HACK: string to look for to pull the zeus id from server output
        private const string c_ZeusMagic = "Successfully authenticated server with Zeus (Server GUID: ";

        // Localhost bind address
        private const string c_LocalhostBindIp = "0.0.0.0";

        public ServerManager(ushort p_GamePortStart = c_DefaultServerPort, ushort p_RconPortStart = c_DefaultRconPort, ushort p_MHarmonyPortStart = c_DefaultMonitoriedHarmonyPort,  ushort p_MaxServerCount = c_DefaultServerCount, string p_GameFilesDir = "", string p_VuFilesDir = "", string p_InstancesDirectory = "")
        {
            // NOTE: THIS NEEDS TO BE DONE FIRST
            MaxServers = p_MaxServerCount;

            // Create a new list for our server extensions
            m_Servers = new List<Win32Server>();            

            // Set rcon ports
            RconPortStart = p_RconPortStart;
            RconPortEnd = (ushort)(RconPortStart + MaxServers);

            // Set mharmony ports
            MHarmonyPortStart = p_MHarmonyPortStart;
            MHarmonyPortEnd = (ushort)(MHarmonyPortStart + MaxServers);

            // Set game ports
            GamePortStart = p_GamePortStart;
            GamePortEnd = (ushort)(GamePortStart + MaxServers);

            // Get and check the games files directory
            GameFilesDirectory = string.IsNullOrWhiteSpace(p_GameFilesDir) ? GetBattlefield3Directory() : p_GameFilesDir;
            if (!Directory.Exists(GameFilesDirectory))
                Console.WriteLine($"err: GAME FILES DIRECTORY ({GameFilesDirectory}) DOES NOT EXIST! SERVERS MAY NOT LAUNCH CORRECTLY.");

            // Get and check the vu executable directory
            VUExecutableDirectory = string.IsNullOrWhiteSpace(p_VuFilesDir) ? GetVeniceUnleashedExecutablePath() : p_VuFilesDir;
            if (!File.Exists(VUExecutableDirectory))
                Console.Write($"err: VU EXECUTABLE DIRECTORY ({VUExecutableDirectory}) DOES NOT EXIST! SERVERS MAY NOT LAUNCH CORRECTLY.");

            // Get and check the instances directory
            InstancesDirectory = string.IsNullOrWhiteSpace(p_InstancesDirectory) ? "Instances" : p_InstancesDirectory;
            if (!Directory.Exists(InstancesDirectory))
                Console.Write($"err: INSTANCES DIRECTORY ({InstancesDirectory}) DOES NOT EXIST! SERVERS MAY NOT LAUNCH CORRECTLY.");
        }

        /// <summary>
        /// Generates a ModList.txt from a list of mod
        /// </summary>
        /// <param name="p_Mods">List of mods</param>
        /// <returns>ModList.txt contents</returns>
        private static string GenerateModList(IEnumerable<string> p_Mods)
        {
            var s_ModListData = string.Empty;
            foreach (var l_Mod in p_Mods)
                s_ModListData += l_Mod + Environment.NewLine;
            return s_ModListData;
        }

        /// <summary>
        /// Generates a Startup.txt to boot the server
        /// 
        /// NOTE: If any of the configuration is changed via rcon it will NOT
        /// be persisted in this Startup.txt
        /// </summary>
        /// <param name="p_ServerName">Display name of the server</param>
        /// <param name="p_RconPassword">Rcon password of the server</param>
        /// <param name="p_GamePassword">Game password of the server</param>
        /// <returns>Startup.txt contents</returns>
        private static string GenerateStartup(string p_ServerName, string p_RconPassword, string p_GamePassword)
        {
            return $"vars.serverName \"{p_ServerName}\"{Environment.NewLine}" +
                                $"vars.friendlyFire true{Environment.NewLine}" +
                                $"admin.password \"{p_RconPassword}\"{Environment.NewLine}" +
                                $"vars.gamePassword {p_GamePassword}";
        }

        /// <summary>
        /// MapName
        /// GameType
        /// Number of Rounds
        /// </summary>
        /// <param name="p_Maps"></param>
        /// <returns></returns>
        private string GenerateMapList(IEnumerable<(string, string, int)> p_Maps)
        {
            var s_MapListData = string.Empty;

            foreach (var l_MapTuple in p_Maps)
                s_MapListData += $"{l_MapTuple.Item1} {l_MapTuple.Item2} {l_MapTuple.Item3}{Environment.NewLine}";

            return s_MapListData;
        }

        /// <summary>
        /// Gets a Win32Server by server id
        /// </summary>
        /// <param name="p_ServerId">Server id</param>
        /// <returns>Win32Server on success, null otherwise</returns>
        public Win32Server? GetWin32ServerById(Guid p_ServerId)
        {
            return m_Servers.FirstOrDefault(p_Win32Server => p_Win32Server._Server.ServerId == p_ServerId);
        }

        /// <summary>
        /// Gets a Server model by server id
        /// </summary>
        /// <param name="p_ServerId">Server id</param>
        /// <returns></returns>
        public Server? GetServerById(Guid p_ServerId)
        {
            return m_Servers.FirstOrDefault(p_Win32Server => p_Win32Server._Server.ServerId == p_ServerId)?._Server;
        }

        /// <summary>
        /// Get a Server model by system process
        /// </summary>
        /// <param name="p_Process">System process</param>
        /// <returns>Server on success, null otherwise</returns>
        private Server? GetServerByProcess(Process p_Process)
        {
            return m_Servers.FirstOrDefault(p_Win32Server => p_Win32Server._Process == p_Process)?._Server;
        }

        /// <summary>
        /// Checks to see if a port is aavailable for use
        /// 
        /// NOTE: This does not check if your remote port is open, only that the local port is not in use
        /// </summary>
        /// <param name="p_PortNumber">Port number to check</param>
        /// <param name="p_Udp">Is this a UDP port?</param>
        /// <returns>True on success, false otherwise</returns>
        private bool IsPortOpen(ushort p_PortNumber, bool p_Udp = false)
        {
            // Check UDP
            if (p_Udp)
                return !System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners().Any(p_EndPoint => p_EndPoint.Port == p_PortNumber);

            // Tcp
            return !System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Any(p_EndPoint => p_EndPoint.Port == p_PortNumber);
        }

        /// <summary>
        /// Gets the next available game port for use
        /// </summary>
        /// <param name="p_PortNumber">Output port number</param>
        /// <returns>True on port found, false if none are available</returns>
        public bool GetAvailableGamePort(out ushort p_PortNumber)
        {
            // Set a default output port
            p_PortNumber = 0;

            // Get all of the ports used by the game server
            var s_ServerUsedGamePorts = m_Servers.Select(p_Win32Server => p_Win32Server._Server.GamePort);

            // Faster check to see if there are any new servers available
            if (s_ServerUsedGamePorts.Count() >= MaxServers)
                return false;

            // Iteraate over the entire game port range
            for (var l_GamePort = GamePortStart; l_GamePort < GamePortEnd; ++l_GamePort)
            {
                // Check to see if the port is open for use on the pc
                if (!IsPortOpen(l_GamePort, true))
                    continue;

                // Assign our port number
                p_PortNumber = l_GamePort;
                return true;
            }

            // We did not find any available port numbers
            return false;
        }

        /// <summary>
        /// Gets the next available rcon port for use
        /// </summary>
        /// <param name="p_PortNumber">Output port number</param>
        /// <returns>True on port found, false if none are available</returns>
        public bool GetAvailableRconPort(out ushort p_PortNumber)
        {
            // Set a default output port
            p_PortNumber = 0;

            // Get all of the ports used by the game server
            var s_ServerUsedRconPorts = m_Servers.Select(p_Win32Server => p_Win32Server._Server.RconPort);

            // Faster check to see if there are any new servers available
            if (s_ServerUsedRconPorts.Count() >= MaxServers)
                return false;

            // Iteraate over the entire game port range
            for (var l_RconPort = RconPortStart; l_RconPort < RconPortEnd; ++l_RconPort)
            {
                // Check to see if the port is open for use on the pc
                if (!IsPortOpen(l_RconPort, false))
                    continue;

                // Assign our port number
                p_PortNumber = l_RconPort;
                return true;
            }

            // We did not find any available port numbers
            return false;
        }

        /// <summary>
        /// Gets the next available Monitored Harmony port for use
        /// </summary>
        /// <param name="p_PortNumber">Output port number</param>
        /// <returns>True on port found, false if none are available</returns>
        public bool GetAvailableMHarmonyPort(out ushort p_PortNumber)
        {
            // Set a default output port
            p_PortNumber = 0;

            // Get all of the ports used by the game server
            var s_ServerUsedMHarmonyPorts = m_Servers.Select(p_Win32Server => p_Win32Server._Server.MonitoredHarmonyPort);

            // Faster check to see if there are any new servers available
            if (s_ServerUsedMHarmonyPorts.Count() >= MaxServers)
                return false;

            // Iteraate over the entire game port range
            for (var l_MHarmonyPort = MHarmonyPortStart; l_MHarmonyPort < MHarmonyPortEnd; ++l_MHarmonyPort)
            {
                // Check to see if the port is open for use on the pc
                if (!IsPortOpen(l_MHarmonyPort, true))
                    continue;

                // Assign our port number
                p_PortNumber = l_MHarmonyPort;
                return true;
            }

            // We did not find any available port numbers
            return false;
        }

        public string GetBattlefield3Directory()
        {
            // Computer\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\EA Games\Battlefield 3
            // Install Dir
            // REG_SZ
            // C:\Program Files (x86)\Origin Games\Battlefield 3\
            
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\EA Games\Battlefield 3", "Install Dir", null) as string ?? string.Empty;
            }
            catch (Exception s_Exception)
            {
                Console.WriteLine($"err: could not get bf3 directory ({s_Exception}).");
            }

            return string.Empty;

        }

        public string GetVeniceUnleashedExecutablePath()
        {
            // Computer\HKEY_CLASSES_ROOT\vu\Shell\Open\Command
            // (default)
            // REG_SZ
            // "C:\Users\godiwik\AppData\Local\VeniceUnleashed\client\vu.exe" "%1"
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var s_LaunchArg = Registry.GetValue(@"HKEY_CLASSES_ROOT\vu\Shell\Open\Command", null, null) as string ?? string.Empty;
                    var s_StartIndex = s_LaunchArg.IndexOf('"');
                    if (s_StartIndex == -1)
                        s_StartIndex = 0;
                    else
                        s_StartIndex++;

                    var s_EndIndex = s_LaunchArg.IndexOf('"', s_StartIndex + 1);
                    if (s_EndIndex == -1)
                        s_EndIndex = s_LaunchArg.Length;

                    var s_PathLen = s_EndIndex - s_StartIndex;

                    var s_LaunchPath = s_LaunchArg.Substring(s_StartIndex, s_PathLen);//.Replace(".exe", ".com");


                    return s_LaunchPath;
                }
            }
            catch (Exception s_Exception)
            {
                Console.WriteLine($"err: could not get vu directory ({s_Exception}).");
            }

            return string.Empty;
        }

        public bool GetAvailableServerKey(out string p_Path)
        {
            p_Path = string.Empty;

            var s_ServerKeyDirectory = $"{InstancesDirectory}/Templates/";
            var s_Files = Directory.GetFiles(s_ServerKeyDirectory, "*.key");

            // For each potential server key we need to check if a server already has it in use
            foreach (var l_File in s_Files)
            {
                // Check if any server is using this server key
                if (m_Servers.Any(p_Win32Server => p_Win32Server._Server.KeyPath == l_File))
                    continue;

                // This key is not in use, return it
                p_Path = l_File;
                return true;
            }

            return false;
        }

        public bool AddServer(out Server? p_Server, bool p_Unlisted = true, string p_BindIp = c_LocalhostBindIp, string p_TemplateName = "", Server.ServerInstanceFrequency p_Frequency = Server.ServerInstanceFrequency.Frequency30, Server.ServerInstanceType p_ServerType = Server.ServerInstanceType.Undefined)
        {
            // Set our default output object
            p_Server = null;

            // Get game port
            if (!GetAvailableGamePort(out ushort s_GamePort))
                return false;

            // Get rcon port
            if (!GetAvailableRconPort(out ushort s_RconPort))
                return false;

            // Get Monitoried Harmony port
            if (!GetAvailableMHarmonyPort(out ushort s_MHarmonyPort))
                return false;

            // Get an available server key
            if (!GetAvailableServerKey(out string s_ServerKeyPath))
                return false;

            // Get the path to the game and to vu
            var s_GamePath = GetBattlefield3Directory();
            var s_VuPath = GetVeniceUnleashedExecutablePath();

            // Generate a new server id, preventing duplicates
            var s_ServerId = Guid.NewGuid();
            while (GetServerById(s_ServerId) != null)
                s_ServerId = Guid.NewGuid();

            var s_ServerIdString = s_ServerId.ToString("N");

            var s_ServerName = $"vusvc_{s_ServerIdString}";

            // Create rcon password
            var s_RconPassword = Guid.NewGuid().ToString("N");

            // Create game password
            var s_GamePassword = Guid.NewGuid().ToString("N");

            // Set the listen address as IP:PORT
            var s_ListenAddress = $"{p_BindIp}:{s_GamePort}";

            // These are default launch arguments for all servers
            var s_LaunchArguments = "-server -dedicated -headless -highResTerrain -skipChecksum -noUpdate -updateBranch dev";
            
            // If the server is unlisted add that flag
            if (p_Unlisted)
                s_LaunchArguments += " -unlisted";
            
            // Depending on the frequency add the required arguments
            switch (p_Frequency)
            {
                case Server.ServerInstanceFrequency.Frequency60:
                    s_LaunchArguments += " -high60";
                    break;
                case Server.ServerInstanceFrequency.Frequency120:
                    s_LaunchArguments += " -high120";
                    break;
            }

            // TODO: Automatically create the instance
            var s_InstanceDirectory = Path.GetFullPath($"{InstancesDirectory}/{s_ServerIdString}");
            if (!Directory.Exists(s_InstanceDirectory))
                Directory.CreateDirectory(s_InstanceDirectory);

            // Add our instance path
            s_LaunchArguments += $" -serverInstancePath {s_InstanceDirectory}";

            // Check to see if there was a template passed in, we will do a 1:1 copy of the supplied template
            if (!string.IsNullOrWhiteSpace(p_TemplateName))
            {
                // Get the template directory path
                var s_TemplateDirectory = $"{InstancesDirectory}/Templates/{p_TemplateName}";

                // Check to see if this template exists
                if (Directory.Exists(s_TemplateDirectory))
                    CopyAll(new DirectoryInfo(s_TemplateDirectory), new DirectoryInfo(s_InstanceDirectory));
            }

            // HACK: Eventually we will just make this apart of the templates directory and say fuck it
            // TODO: Make the ability to scan the configuration files and find and replace thigns like the name, rcon pw, game pw

            // Copy the server.key
            File.Copy(s_ServerKeyPath, $"{s_InstanceDirectory}/server.key");

            // Write out the Startup.txt
            File.WriteAllText($"{s_InstanceDirectory}/Admin/Startup.txt", GenerateStartup(s_ServerName, s_RconPassword, s_GamePassword));

            // Write out the ModList.txt
            File.WriteAllText($"{s_InstanceDirectory}/Admin/ModList.txt", GenerateModList(new[] { "VU-BattleRoyale" }));

            // Write out the MapList.txt
            File.WriteAllText($"{s_InstanceDirectory}/Admin/MapList.txt", GenerateMapList(new[] { ("XP3_Desert", "ConquestLarge0", 1) }));

            // Create the new process
            var s_Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = s_VuPath,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = s_LaunchArguments,
                    
                    //WorkingDirectory = Path.GetDirectoryName(s_VuPath) ?? "./"
                },
                EnableRaisingEvents = true
            };

            // Register our events
            s_Process.OutputDataReceived += OnProcessOuputReceived;
            s_Process.ErrorDataReceived += OnProcessErrorReceived;
            s_Process.Exited += OnProcessExited;

            // Start the server
            if (!s_Process.Start())
                return false;

            // Start reading async
            s_Process.BeginOutputReadLine();
            s_Process.BeginErrorReadLine();

            // Wait for the process to exit async
            var s_Task = s_Process.WaitForExitAsync();

            var s_Win32Server = new Win32Server
            {
                WaitTask = s_Task,
                _Process = s_Process,
                InstancePath = s_InstanceDirectory,
                _Server = new Server
                {
                    ServerId = s_ServerId,
                    GamePassword = s_GamePassword,
                    GamePort = s_GamePort,
                    MonitoredHarmonyPort = s_MHarmonyPort,
                    Name = s_ServerName,
                    PlayerIds = new List<Guid>(),
                    RconPassword = s_RconPassword,
                    RconPort = s_RconPort,
                    ServerType = p_ServerType,
                    KeyPath = s_ServerKeyPath,
                    ServerFrequency = p_Frequency
                }
            };

            p_Server = s_Win32Server._Server;

            m_Servers.Add(s_Win32Server);

            

            return true;
        }

        public bool Debug_SpawnServer(out Server? p_Server)
        {
            p_Server = null;

#if DEBUG
            var s_ExecutablePath = @"C:\Users\godiwik\AppData\Local\VeniceUnleashed\client\vu.exe";
            // UDP mharmony port
            var s_MonitoredHarmonyPort = (ushort)7948;
            var s_Unlisted = false;
            var s_Listen = "0.0.0.0:25200";

            var s_Process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = s_ExecutablePath,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    Arguments = "-server -dedicated -headless -highResTerrain -skipChecksum -noUpdate"
                },
                EnableRaisingEvents = true
            };

            // Set up the arguments


            // Register our events
            s_Process.OutputDataReceived -= OnProcessOuputReceived;
            s_Process.ErrorDataReceived -= OnProcessErrorReceived;
            s_Process.OutputDataReceived += OnProcessOuputReceived; 
            s_Process.ErrorDataReceived += OnProcessErrorReceived;
            s_Process.Exited += OnProcessExited;

            if (!s_Process.Start())
                return false;

            s_Process.BeginOutputReadLine();
            s_Process.BeginErrorReadLine();

            var s_Task = s_Process.WaitForExitAsync();

            // TODO: Automatically create the instance
            // TODO: Default instance directory to copy
            // TODO: Server.key management

            var s_Win32Server = new Win32Server
            {
                WaitTask = s_Task,
                _Process = s_Process,
                _Server = new Server
                {
                    ServerId = Guid.NewGuid(),
                    GamePassword = "",
                    GamePort = 25200,
                    MonitoredHarmonyPort = s_MonitoredHarmonyPort,
                    Name = "server",
                    PlayerIds = new List<Guid>(),
                    RconPassword = "cows",
                    RconPort = 25201,
                    ServerType = Server.ServerInstanceType.Undefined
                }
            };

            p_Server = s_Win32Server._Server;

            m_Servers.Add(s_Win32Server);
            
            return true;
#else
            return false;
#endif
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            var s_Process = sender as Process;

            s_Process.CancelOutputRead();
            s_Process.CancelErrorRead();

            // Find the server via process
            var s_Server = GetServerByProcess(s_Process);
            if (s_Server is null)
                return;

            var s_ServerId = s_Server.ServerId;
            var s_ZeusId = s_Server.ZeusId;

            Console.WriteLine($"Server: {s_ServerId} with Zeus Id {s_ZeusId} has terminated.");

            // Notify all event listeners that this server has terminated
            s_Server.OnTerminated();

            // Force remove the server
            if (!RemoveServer(s_Server.ServerId, true))
                Console.Write($"err: server removal failed ({s_ServerId}).");
        }

        private void OnProcessErrorReceived(object sender, DataReceivedEventArgs e)
        {
            var s_Process = sender as Process;

            // Find the server via process
            var s_Server = GetServerByProcess(s_Process);
            if (s_Server is null)
                return;

            var s_StringData = e.Data ?? string.Empty;
            if (string.IsNullOrWhiteSpace(s_StringData))
                return;

            s_Server.ErrorLog += $"{s_StringData}{Environment.NewLine}";
        }

        private void OnProcessOuputReceived(object sender, DataReceivedEventArgs e)
        {
            // Get the process
            var s_Process = sender as Process;

            // Find the server via process
            var s_Server = GetServerByProcess(s_Process);
            if (s_Server is null)
                return;

            var s_StringData = e.Data ?? string.Empty;
            if (s_StringData.Contains(c_ZeusMagic))
            {
                var s_StartIndex = s_StringData.IndexOf(c_ZeusMagic);
                if (s_StartIndex == -1)
                    return;

                s_StartIndex += c_ZeusMagic.Length;

                var s_ZeusIdString = s_StringData.Substring(s_StartIndex, 32);
                var s_ZeusId = Guid.Parse(s_ZeusIdString);

                //Console.WriteLine($"ZEUS ID FOUND: {s_ZeusId}");

                s_Server.ZeusId = s_ZeusId;

                // Fire events for this server
                s_Server.OnZeusIdUpdated(s_ZeusId);
            }

#if DEBUG
            Debug.WriteLine($"{s_StringData}{Environment.NewLine}");
#endif

            // Validate we have any string data instead of adding blank lines or spaces
            if (string.IsNullOrWhiteSpace(s_StringData))
                return;

            // Add it to our servers output log
            s_Server.OutputLog += $"{s_StringData}{Environment.NewLine}";
        }

        /// <summary>
        /// Stops a servers process
        /// </summary>
        /// <param name="p_ServerId">Server id</param>
        /// <returns>True on success, false otherwise</returns>
        public bool TerminateServer(Guid p_ServerId, bool p_DeleteInstanceDirectory = false)
        {
            // Get the Win32Server reference
            var s_Server = m_Servers.FirstOrDefault(p_Win32Server => p_Win32Server._Server.ServerId == p_ServerId);
            if (s_Server is null)
                return false;

            // Kill the process
            s_Server._Process.Kill(true);

            //// Wait for the process to free all resources before continuing
            //if (s_Server.WaitTask.Status != TaskStatus.WaitingForActivation ||
            //    s_Server.WaitTask.Status != TaskStatus.RanToCompletion)
            //    s_Server.WaitTask.GetAwaiter().GetResult();

            if (p_DeleteInstanceDirectory)
            {
                try
                {
                    // If the directory exists delete it
                    if (Directory.Exists(s_Server.InstancePath))
                        Directory.Delete(s_Server.InstancePath, true);
                }
                catch (Exception p_Exception)
                {
                    Console.WriteLine($"err: could not delete instance for server ({p_ServerId}), ({p_Exception}).");
                }

                // Clear the instance path
                s_Server.InstancePath = string.Empty;
            }

            return true;
        }

        /// <summary>
        /// Stops all servers processes
        /// </summary>
        public void TerminateAllServers()
        {
            foreach (var l_Server in m_Servers)
            {
                l_Server._Process.Kill(true);
            }

            Task.WaitAll(m_Servers.Select(p_Win32Server => p_Win32Server.WaitTask).ToArray());
        }

        /// <summary>
        /// Removes a server from the server manager
        /// </summary>
        /// <param name="p_ServerId">Server id</param>
        /// <param name="p_Terminate">Should the server be terminated</param>
        public bool RemoveServer(Guid p_ServerId, bool p_Terminate = true)
        {
            // Determine if we need to terminate this server
            if (p_Terminate)
            {
                // Attempt to kill the process
                if (!TerminateServer(p_ServerId, true))
                    Console.WriteLine($"warn: server ({p_ServerId}) could not be terminated.");
            }

            // Remove the server from our list
            return m_Servers.RemoveAll(p_Win32Server => p_Win32Server._Server.ServerId == p_ServerId) > 0;
        }

        /// <summary>
        /// Removes all servers from the server manaager
        /// </summary>
        /// <param name="p_Terminate">Should the servers be terminated</param>
        public void RemoveAllServers(bool p_Terminate = true)
        {
            // Terminate all of the servers if needed
            if (p_Terminate)
            {
                var s_ServerIdList = m_Servers.Select(p_Win32Server => p_Win32Server._Server.ServerId).ToArray();

                foreach (var l_ServerId in s_ServerIdList)
                    TerminateServer(l_ServerId, true);
            }

            // Clear the list
            m_Servers.Clear();
        }

        /// <summary>
        /// This was shamelessly stolen from stackoverflow
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        private static void CopyAll(DirectoryInfo source, DirectoryInfo target)
        {
            if (source.FullName.ToLower() == target.FullName.ToLower())
            {
                return;
            }

            // Check if the target directory exists, if not, create it.
            if (Directory.Exists(target.FullName) == false)
            {
                Directory.CreateDirectory(target.FullName);
            }

            // Copy each file into it's new directory.
            foreach (FileInfo fi in source.GetFiles())
            {
                //Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
                fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
            }

            // Copy each subdirectory using recursion.
            foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
            {
                DirectoryInfo nextTargetSubDir =
                    target.CreateSubdirectory(diSourceSubDir.Name);
                CopyAll(diSourceSubDir, nextTargetSubDir);
            }
        }

        public Server? GetServerByZeusId(Guid p_ZeusId)
        {
            return m_Servers.FirstOrDefault(p_Server => p_Server._Server.ZeusId == p_ZeusId)?._Server;
        }
    }
}
