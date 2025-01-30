using ChatClient.Net.IO;
using System.Net.Sockets;

namespace ChatClient.Net
{
    class Server
    {
        private TcpClient _client;
        public PacketReader PacketReader { get; private set; }

        public event Func<Task> ConnectedEvent;
        public event Action<string> MessageReceivedEvent;
        public event Action DisconnectedEvent;

        public async Task ConnectToServerAsync(string username, int maxRetries = 5, int delayRetries = 2000)
        {
            System.Diagnostics.Debug.WriteLine("Starting connection attempts...");
            int attempt = 0;

            while (attempt < maxRetries)
            {
                try
                {
                    _client = new TcpClient();
                    await _client.ConnectAsync("127.0.0.1", 5000);
                    PacketReader = new PacketReader(_client.GetStream());

                    Console.WriteLine($"Transmitting username: {username}");
                    if (!string.IsNullOrEmpty(username))
                    {
                        var connectPacket = new PacketBuilder();
                        connectPacket.WriteOpCode(0);
                        connectPacket.WriteString(username);
                        await _client.Client.SendAsync(connectPacket.GetPacketBytes(), SocketFlags.None);

                        Console.WriteLine("Connected to server");
                        System.Diagnostics.Debug.WriteLine("Connected to 127.0.0.1");

                        _ = ReadPacketAsync();
                        return;
                    }
                }
                catch (SocketException ex)
                {
                    attempt++;
                    Console.WriteLine($"Attempt {attempt}/{maxRetries} failed: {ex.SocketErrorCode}");
                    System.Diagnostics.Debug.WriteLine($"Attempt {attempt}/{maxRetries} failed: {ex.SocketErrorCode}");

                    if (attempt < maxRetries)
                    {
                        Console.WriteLine($"Retrying in {delayRetries}ms...");
                        await Task.Delay(delayRetries);
                    }
                    else
                    {
                        Console.WriteLine("All connection attempts failed.");
                        DisconnectedEvent?.Invoke();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"Error connecting: {ex.Message}");
                    DisconnectedEvent?.Invoke();
                    break;
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
