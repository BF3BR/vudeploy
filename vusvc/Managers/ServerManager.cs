using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using vusvc.Data;

namespace vusvc.Managers
{
    public class ServerManager : IServerManager
    {
        public class Win32Server
        {
            public Server _Server { get; set; }
            public Process _Process { get; set; }
            public Task WaitTask { get; set; }
        }

        private List<Win32Server> m_Servers;

        public IEnumerable<Server> Servers => m_Servers.Select(p_Win32Server => p_Win32Server._Server);

        public int MaxServers { get; set; }

        private const int c_DefaultServerCount = 2;
        private const string c_ZeusMagic = "Successfully authenticated server with Zeus (Server GUID: ";

        public ServerManager(int p_MaxServerCount = c_DefaultServerCount)
        {
            m_Servers = new List<Win32Server>();

            MaxServers = p_MaxServerCount;
        }

        private string GenerateModList(IEnumerable<string> p_Mods)
        {
            var s_ModListData = string.Empty;
            foreach (var l_Mod in p_Mods)
                s_ModListData += l_Mod + Environment.NewLine;
            return s_ModListData;
        }

        private string GenerateStartup(string p_ServerName, string p_RconPassword, string p_GamePassword)
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

        public Win32Server? GetWin32ServerById(Guid p_ServerId)
        {
            return m_Servers.FirstOrDefault(p_Win32Server => p_Win32Server._Server.Id == p_ServerId);
        }

        public Server? GetServerById(Guid p_ServerId)
        {
            return m_Servers.FirstOrDefault(p_Win32Server => p_Win32Server._Server.Id == p_ServerId)?._Server;
        }

        private Server? GetServerByProcess(Process p_Process)
        {
            return m_Servers.FirstOrDefault(p_Win32Server => p_Win32Server._Process == p_Process)?._Server;
        }

        public bool SpawnServer(out Server? p_Server)
        {
            p_Server = null;

            var s_ExecutablePath = @"C:\Users\godiwik\AppData\Local\VeniceUnleashed\client\vu.com";
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
                    Id = Guid.NewGuid(),
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
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            var s_Process = sender as Process;

            // Find the server via process
            var s_Server = GetServerByProcess(s_Process);
            if (s_Server is null)
                return;

            Console.WriteLine($"Server: {s_Server.Id} with Zeus Id {s_Server.ZeusId} has terminated.");
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
            }

            // Validate we have any string data instead of adding blank lines or spaces
            if (string.IsNullOrWhiteSpace(s_StringData))
                return;

            // Add it to our servers output log
            s_Server.OutputLog += $"{s_StringData}{Environment.NewLine}";
        }

        public bool KillServer(Guid p_ServerId)
        {
            throw new NotImplementedException();
        }

        public void KillAllServers()
        {
            foreach (var l_Server in m_Servers)
            {
                l_Server._Process.Kill(true);
            }
        }
    }
}
