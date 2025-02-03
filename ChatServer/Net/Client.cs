using ChatServer.Net.IO;
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatServer.Net
{
    class Client
    {
        public string Username { get; private set; }
        public Guid UID { get; private set; }
        public TcpClient ClientSocket { get; }

        private PacketReader _packetReader;

        public event Action<Client, string, string> DirectMessageReceived;
        public event Action<Client> Disconnected;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            _ = InitializeAsync();
            _ = ProcessAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                byte opcode = await _packetReader.ReadOpcodeAsync();
                if (opcode != 1)
                {
                    throw new Exception("Expected registration opcode.");
                }

                Username = await _packetReader.ReadStringAsync();
                Console.WriteLine($"{Username} connected!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize client: {ex.Message}");
                Disconnected?.Invoke(this);
                ClientSocket.Dispose();
            }
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

                            var recipient = await _packetReader.ReadStringAsync();
                            var messageText = await _packetReader.ReadStringAsync();
                            DirectMessageReceived?.Invoke(this, recipient, messageText);
                            break;
                        default:
                            Console.WriteLine($"Unknown opcode: {opcode}");
                            break;
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine($"{Username} disconnected unexpectedly.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing client {Username}: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"Removing {Username} from the server.");
                Disconnected?.Invoke(this);
                ClientSocket.Dispose();
            }
        }
    }
}
