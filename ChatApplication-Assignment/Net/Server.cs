using ChatClient.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Net
{
    class Server
    {
        private TcpClient _client;
        public PacketReader PacketReader { get; private set; }

        public event Func<Task> ConnectedEvent;
        public event Action<string> MessageReceivedEvent;
        public event Action DisconnectedEvent;

        public async Task ConnectToServerAsync(string username)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync("127.0.0.1", 5000);
                PacketReader = new PacketReader(_client.GetStream());

                if (!string.IsNullOrEmpty(username)) //----------------------------------------------------------------------Reminder to fix this
                {
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteString(username);
                    await _client.Client.SendAsync(connectPacket.GetPacketBytes(), SocketFlags.None);

                    Console.WriteLine("Connected to server");
                    System.Diagnostics.Debug.WriteLine("Connected to 127.0.0.1");
                }

                await ReadPacketAsync();
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Error connecting to server: {ex.SocketErrorCode}");
                System.Diagnostics.Debug.WriteLine($"Error connecting to server: {ex.SocketErrorCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting{ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error connecting to server:{ex.Message}");
                DisconnectedEvent?.Invoke();
            }
        }

        private async Task ReadPacketAsync()
        {
            try
            {
                while (true)
                {
                    var opcode = await PacketReader.ReadOpcodeAsync();
                    switch (opcode)
                    {
                        case 1:
                            ConnectedEvent?.Invoke();
                            break;
                        case 5:
                            var message = await PacketReader.ReadStringAsync();
                            MessageReceivedEvent?.Invoke(message);
                            break;
                        case 10:
                            DisconnectedEvent?.Invoke();
                            break;
                        default:
                            Console.WriteLine($"Unknown opcode: {opcode}");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading packet: {ex.Message}");
                DisconnectedEvent?.Invoke();
            }

        }

        public async Task SendMessageAsync(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteString(message);
            try
            {
                await _client.Client.SendAsync(messagePacket.GetPacketBytes(), SocketFlags.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                DisconnectedEvent?.Invoke();
            }
            
        }
    }
}
