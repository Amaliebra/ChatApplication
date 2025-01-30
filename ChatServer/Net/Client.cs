using ChatServer.Net.IO;
using System.Net.Sockets;

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

            _ = InitializeAsync();
            _ = ProcessAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                var opcode = await _packetReader.ReadOpcodeAsync();
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
                    try
                    {
                        var opcode = await _packetReader.ReadOpcodeAsync();
                        switch (opcode)
                        {
                            case 5:
                                var message = await _packetReader.ReadStringAsync();
                                Console.WriteLine($"[{DateTime.Now}] {Username}: {message}");

                                MessageReceived?.Invoke(this, message);
                                break;
                            default:
                                Console.WriteLine($"Unknown opcode: {opcode}");
                                break;
                        }
                    }
                    catch (IOException)
                    {
                        Console.WriteLine($"⚠️ {Username} disconnected unexpectedly.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error processing client {Username}: {ex.Message}");
                        break;
                    }
                }
            }
            finally
            {
                Console.WriteLine($"🚪 Removing {Username} from the server.");
                Disconnected?.Invoke(this);
                ClientSocket.Dispose();
            }
        }
    }
}