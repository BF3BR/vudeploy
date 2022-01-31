using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace vusvc.tests
{
    public class RconClient
    {
        public class RconPacket
        {
            public uint InternalSequence { get; set; }
            public uint Size { get; set; }
            public byte[] Data { get; set; }

            public List<string> Words { get; protected set; } =  new List<string>();
            private uint WordCount { get; set; }

            public const int c_HeaderSize = 12;

            public RconPacket()
            {
                Reset();
            }

            public RconPacket(List<string> p_Words, uint p_Sequence, bool p_Response)
            {
                InternalSequence = (uint)((p_Response ? 0x00000000 : 0x80000000) | (p_Response ? 0x40000000 : 0x00000000) | (p_Sequence & 0x3FFFFFFF));
                Words = p_Words;
                WordCount = (uint)Words.Count;
            }

            public void Reset()
            {
                InternalSequence = 0;
                Size = 0;
                WordCount = 0;
                Words.Clear();
            }

            public void InitHeader(BinaryReader p_Reader)
            {
                InternalSequence = p_Reader.ReadUInt32();
                Size = p_Reader.ReadUInt32();
                WordCount = p_Reader.ReadUInt32();
            }

            public bool InitData(BinaryReader p_Reader)
            {
                var s_DataLeft = p_Reader.BaseStream.Length - p_Reader.BaseStream.Position;

                if (s_DataLeft != Size - c_HeaderSize)
                    return false;

                for (var i = 0; i < WordCount; ++i)
                {
                    var l_Size = p_Reader.ReadUInt32();

                    var l_Word = p_Reader.ReadChars((int)l_Size);

                    Words.Add(new string(l_Word));

                    p_Reader.BaseStream.Seek(1, SeekOrigin.Current);
                }

                return true;
            }

            public bool FromServer()
            {
                return (InternalSequence & 0x80000000) != 0;
            }

            public bool Response()
            {
                return (InternalSequence & 0x40000000) != 0;
            }

            public uint Sequence()
            {
                return (InternalSequence & 0x3FFFFFFF);
            }

            public RconPacket CreateResponse(List<string> p_Words)
            {
                return new RconPacket(p_Words, Sequence(), true);
            }

            public byte[] Serialize()
            {
                using var s_Writer = new BinaryWriter(new MemoryStream());

                var s_SerializedWords = SerializeWords();

                Size = (uint)s_SerializedWords.Length + c_HeaderSize;

                s_Writer.Write(InternalSequence);
                s_Writer.Write(Size);
                s_Writer.Write(WordCount);
                s_Writer.Write(s_SerializedWords);

                return ((MemoryStream)s_Writer.BaseStream).ToArray();
            }

            protected byte[] SerializeWords()
            {
                using var s_Stream = new MemoryStream();
                using var s_Writer = new BinaryWriter(s_Stream);

                WordCount = (uint)Words.Count;

                for (var i = 0; i < WordCount; ++i)
                {
                    var l_WordSize = Words[i].Length;
                    var l_Word = Words[i];

                    s_Writer.Write(l_WordSize);
                    s_Writer.Write(l_Word);
                    s_Writer.Write((byte)0);
                }

                return s_Stream.ToArray();
            }
        }
        /// <summary>
        /// Hostname/IP of the server
        /// </summary>
        public string Host { get; protected set; }

        /// <summary>
        /// Port of the server
        /// </summary>
        public ushort Port { get; protected set; }

        public byte[] Buffer { get; protected set; } = new byte[c_MaxPacketSize + RconPacket.c_HeaderSize];

        // Maximum data packet size
        public const int c_MaxPacketSize = 0x4000;

        // Rcon tcp client
        protected TcpClient m_Client;

        // Stream of the client
        protected NetworkStream Stream => m_Client.GetStream();

        public RconClient(string p_HostName = "localhost", ushort p_Port = 47200)
        {
            Host = p_HostName;
            Port = p_Port;

            m_Client = new TcpClient();
        }

        public bool Connect(string p_Password)
        {
            try
            {
                m_Client.Connect(Host, Port);
            }
            catch (Exception s_Exception)
            {
                Console.WriteLine($"failed to connect to {Host}:{Port} {s_Exception.Message}");
                Debug.WriteLine($"failed to connect to {Host}:{Port} {s_Exception.Message}");
                return false;
            }

            return true;
        }

        public string SendCommand(string p_Command, params string[] p_Args)
        {
            var s_ArgsList = string.Join(" ", p_Args);
            var s_Final = p_Command + (string.IsNullOrWhiteSpace(s_ArgsList) ? " " : string.Empty) + s_ArgsList;



            return string.Empty;
        }

        public RconPacket? ReadPacket()
        {
            using var s_Reader = new BinaryReader(new MemoryStream(Buffer));

            // Read the header from the wire
            Stream.Read(Buffer, 0, RconPacket.c_HeaderSize);

            var s_Packet = new RconPacket();
            s_Packet.InitHeader(s_Reader);

            if (s_Packet.Size > c_MaxPacketSize)
                return null;

            if (s_Packet.Size <= RconPacket.c_HeaderSize)
                return null;

            // Read the data off of the wire
            Stream.Read(Buffer, RconPacket.c_HeaderSize, (int)s_Packet.Size);

            // Parse all of the data
            s_Packet.InitData(s_Reader);

            return s_Packet;
        }

        public bool WritePacket()
        {
            var s_Packet = new RconPacket();

            throw new NotImplementedException();
        }
    }

    public class RconTests
    {
        [Fact]
        public void TestServerInfo()
        {
            var s_Client = new RconClient();
            Assert.True(s_Client.Connect(String.Empty));

            Assert.NotEmpty(s_Client.SendCommand("serverinfo"));
        }
    }
}
