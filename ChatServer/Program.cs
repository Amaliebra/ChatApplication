
using ChatServer.Net;
using ChatServer.Net.IO;
using System.Net;
using System.Net.Sockets;

public class Program
{
    private TcpListener _server;
    private List<Client> _clients = new List<Client>();

    public Program(string ip, int port)
    {
        _server = new TcpListener(IPAddress.Any, port);
    }

    public static async Task Main(string[] args)
    {
        var program = new Program("127.0.0.1", 5000);
        await program.StartAsync();
    }
    private Client ClientForTcp(TcpClient tcpClient)
    {
        return _clients.FirstOrDefault(c => c.ClientSocket == tcpClient);
    }


    public async Task StartAsync()
    {
        _server.Start();
        Console.WriteLine("Server started");

        while (true)
        {
            Console.WriteLine("Waiting for a client...");
            var tcpClient = await _server.AcceptTcpClientAsync();


            if (_clients.Any(c => ((IPEndPoint)c.ClientSocket.RemoteEndPoint).Address.Equals(
                                     ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address)))
            {
                Console.WriteLine("Duplicate client detected, rejecting connection.");
                tcpClient.Close();
                continue;
            }
            var clientWrapper = new Client(tcpClient);
            _clients.Add(clientWrapper);
            Console.WriteLine($"Client connected: {((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address}");

            _ = Task.Run(() => HandleClientAsync(clientWrapper));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        TcpClient tcpClient = clientWrapper.ClientSocket;
        try
        {
            using var networkStream = tcpClient.GetStream();
            var packetReader = new PacketReader(networkStream);

            string senderUsername = clientWrapper.Username ?? "Unknown";
            while (true)
            {
                Console.WriteLine($"{DateTime.Now} - Waiting for client data...");
                var opCode = await packetReader.ReadOpcodeAsync();
                Console.WriteLine($"{DateTime.Now} - Opcode received: {opCode}");

                if (opCode == 5)
                {
                    var recipient = await packetReader.ReadStringAsync();
                    var messageText = await packetReader.ReadStringAsync();
                    Console.WriteLine($"{DateTime.Now} - Direct message from {senderUsername} to {recipient}: {messageText}");

                    var targetClient = _clients.FirstOrDefault(c => c.Username == recipient);
                    if (targetClient != null)
                    {
                        var forwardPacket = new PacketBuilder();
                        forwardPacket.WriteOpCode(5);
                        forwardPacket.WriteString(senderUsername);
                        forwardPacket.WriteString(messageText);
                        var packetBytes = forwardPacket.GetPacketBytes();

                        await targetClient.ClientSocket.GetStream().WriteAsync(packetBytes, 0, packetBytes.Length);
                    }
                    else
                    {
                        Console.WriteLine($"Recipient {recipient} not found.");
                    }
                }
                else
                {
                    var message = await packetReader.ReadStringAsync();
                    Console.WriteLine($"{DateTime.Now} - Received message: {message}");

                    foreach (var connectedClient in _clients)
                    {
                        var stream = connectedClient.ClientSocket.GetStream();
                        var packetBuilder = new PacketBuilder();
                        packetBuilder.WriteOpCode(opCode);
                        packetBuilder.WriteString(message);
                        var packetBytes = packetBuilder.GetPacketBytes();
                        await stream.WriteAsync(packetBytes, 0, packetBytes.Length);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Removing client from server");
            _clients.Remove(clientWrapper);
            tcpClient.Close();
        }
    }
    private void HandleDirectMessage(Client sender, string recipient, string messageText)
    {
        Console.WriteLine($"Direct message from {sender.Username} to {recipient}: {messageText}");

        var targetClient = _clients.FirstOrDefault(c => c.Username == recipient);
        if (targetClient != null)
        {

            var forwardPacket = new PacketBuilder();
            forwardPacket.WriteOpCode(5);

            forwardPacket.WriteString(sender.Username);
            forwardPacket.WriteString(messageText);

            var packetBytes = forwardPacket.GetPacketBytes();
            targetClient.ClientSocket.GetStream().WriteAsync(packetBytes, 0, packetBytes.Length);
        }
        else
        {
            Console.WriteLine($"Recipient {recipient} not found.");
        }
    }
}
