using ChatClient.Net.IO;
using System.Net.Sockets;
using ChatClient.MVVM.Model;
using ChatClient.MVVM.ViewModel;
using ChatClient.Net;

namespace ChatClient.Net
{
    class Server
    {
        private TcpClient _client;
        public PacketReader PacketReader { get; private set; }
        public event Func<Task> ConnectedEvent; /// FIX THISS REMINDERRRRRRRRRRRRRRRRRRRRRRRRRRRRRRR
        public event Action<string> MessageReceivedEvent;
        public event Action DisconnectedEvent;
        public event Action<List<string>> UserListUpdatedEvent;
        private List<string> _connectedUsers = new();

        public async Task ConnectToServerAsync(string username, int maxRetries = 5, int delayRetries = 2000)
        {
            System.Diagnostics.Debug.WriteLine("Starting connection attempts...");
            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    if (_client != null && _client.Connected)
                    {
                        Console.WriteLine("Already connected to the server.");
                        return;
                    }

                    _client = new TcpClient();
                    await _client.ConnectAsync("127.0.0.1", 5000);
                    PacketReader = new PacketReader(_client.GetStream());

                    await Task.Delay(200);

                    Console.WriteLine($"Transmitting username: {username}");
                    var packetBuilder = new PacketBuilder();
                    packetBuilder.WriteOpCode(0); 
                    packetBuilder.WriteString(username);
                    await _client.GetStream().WriteAsync(packetBuilder.GetPacketBytes());
                    if (!_connectedUsers.Contains(username))
                    {
                        _connectedUsers.Add(username);
                        UserListUpdatedEvent?.Invoke(new List<string>(_connectedUsers));
                    }
                }
                catch (Exception ex)
                {
                    attempt++;
                    Console.WriteLine($"Connection attempt {attempt} failed: {ex.Message}");
                    await Task.Delay(delayRetries);
                }
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
                            if (ConnectedEvent != null)
                                await ConnectedEvent.Invoke();
                            break;
                        case 5:
                            var message = await PacketReader.ReadStringAsync();
                            Console.WriteLine($"[{DateTime.Now}] {message}");
                            MessageReceivedEvent?.Invoke(message);
                            break;
                        case 10:
                            DisconnectedEvent?.Invoke();
                            return;
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
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            if (_client == null || !_client.Connected)
            {
                Console.WriteLine("Not connected to server");
                DisconnectedEvent?.Invoke();
                return;
            }

            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(5);
            messagePacket.WriteString(message);

            try
            {
                await _client.Client.SendAsync(messagePacket.GetPacketBytes(), SocketFlags.None);
            }
            catch(SocketException ex)
            {
                Console.WriteLine(ex.SocketErrorCode);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
                DisconnectedEvent?.Invoke();
            }
            
        }
    }
}
