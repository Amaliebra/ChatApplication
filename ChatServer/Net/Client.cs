﻿using ChatServer.Net.IO;
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
        public string Username { get; private set; }
        public Guid UID { get; private set; }
        public TcpClient ClientSocket { get; private set; }

        private PacketReader _packetReader;

        public event Action<Client, string> MessageReceived;
        public event Action<Client> Disconnected;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            var opcode = _packetReader.ReadOpcode();
            Username = _packetReader.ReadString();

            Console.WriteLine($"{Username} connected!");

            _ = ProcessAsync();
        }

        private async Task ProcessAsync()
        {
            try
            {
                while (true)
                {
                    var opcode = await _packetReader.ReadOpcodeAsync();
                    switch (opcode)
                    {
                        case 5:
                            var message = await _packetReader.ReadStringAsync();
                            Console.WriteLine($"[{DateTime.Now}]" +
                                $" {message}");

                            MessageReceived?.Invoke(this, message);
                            break;
                        default:
                            Console.WriteLine($"Unknown opcode: {opcode}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{UID}{Username} Disconnected {ex.Message}");
                Disconnected?.Invoke(this);
                ClientSocket.Close();
            }
        }
    }
}
