using ChatServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer.Net
{
    class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient TcpClient { get; set; }

        PacketReader _packetReader;
        public Client(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(tcpClient.GetStream());
        }
    }
}
