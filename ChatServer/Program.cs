
using ChatServer.Net.IO;
using System.Net;
using System.Net.Sockets;

public class Program
{
    private TcpListener _server;
    private List<TcpClient> _clients;

    public Program(string ip, int port)
    {
        _server = new TcpListener(IPAddress.Any, port);
        _clients = new List<TcpClient>();
    }

    public static async Task Main(string[] args)
    {
        var program = new Program("127.0.0.1", 5000);
        await program.StartAsync();
    }

    public async Task StartAsync()
    {
        _server.Start();
        Console.WriteLine("Server started");
        while (true)
        {
            var client = await _server.AcceptTcpClientAsync();
            _clients.Add(client);
            Console.WriteLine("Client connected");
            Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using var networkStream = client.GetStream();
        var packetReader = new PacketReader(networkStream);

        try
        {
            while (true)
            {
                var opCode = packetReader.ReadOpcode();
                var message = packetReader.ReadString();
                Console.WriteLine($"Received message: {message}");

                foreach (var connectedClient in _clients)
                {
                    var stream = connectedClient.GetStream();
                    var packetBuilder = new PacketBuilder();
                    packetBuilder.WriteOpCode(opCode);
                    packetBuilder.WriteString(message);
                    var packetBytes = packetBuilder.GetPacketBytes();
                    await stream.WriteAsync(packetBytes, 0, packetBytes.Length);
                }
            }
        }
        catch (Exception)
        {
            _clients.Remove(client);
            Console.WriteLine("Client disconnected");
        }
    }
}
